using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// Default impelmentation of the <see cref="ILogMessageQueue{TMessageEntry, TOptions}"/>.
/// <para>
/// When the queue size is unbounded it uses <see cref="System.Threading.Channels.Channel"/> under the hood for the async wait and dequeue behaviors.
/// </para>
/// <para>
/// When the queue size is bounded and the queue full behavior is not <see cref="QueueFullBehavior.Wait"/> it also uses <see cref="System.Threading.Channels.Channel"/>
/// under the hood since there is no need to block during the <see cref="EnqueueMessage(TMessageEntry)"/> call.
/// </para>
/// <para>
/// When the queue size is bounded and the queue full behavior is not <see cref="QueueFullBehavior.Wait"/> it uses custom <see cref="SemaphoreSlim"/> counters since
/// the EnqueueMessage method needs to be called synchronously from the logger and the dequeue behavior can be async.
/// </para>
/// </summary>
/// <typeparam name="TMessageEntry">The type of the log message entry to be stored in the queue</typeparam>
/// <typeparam name="TOptions">The logger options type</typeparam>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public sealed class InnerLogMessageQueue<TMessageEntry, TOptions> : ILogMessageQueue<TMessageEntry, TOptions>, IDisposable
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
	where TOptions : QueueLoggerOptions
{
	private const int DefaultQueueFullWaitTimeoutThreshold = 60_000; // 1Minute

	private readonly CancellationTokenSource? _consumersCancellationTokenSource;
	private readonly ConcurrentQueue<TMessageEntry>? _boundedQueue;
	private SemaphoreSlim? _freeNodes;
	private bool _isDisposed;
	private readonly Channel<TMessageEntry>? _messageChannel;
	private readonly ChannelReader<TMessageEntry>? _messageChannelReader;
	private readonly ChannelWriter<TMessageEntry>? _messageChannelWriter;
	private readonly int _maxQueueSize;
	private SemaphoreSlim? _occupiedNodes;
	private readonly CancellationTokenSource? _producersCancellationTokenSource;
	private readonly QueueFullBehavior _queueFullBehavior;
	private int _queueFullWaitTimeoutThreshold;

	/// <summary>
	/// Construct the <see cref="InnerLogMessageQueue{TMessageEntry, TOptions}"/> instance.
	/// </summary>
	/// <remarks>
	/// The MaxQueueSize, QueueFullBehavior, and QueueFullWaitTimeoutThreshold options are not reloadable so there isn't any need to
	/// use IOptionsMonitor or register a reload change token listener
	/// </remarks>
	public InnerLogMessageQueue(
		IOptions<TOptions> optionsMonitor)
		: this(optionsMonitor is not null ? optionsMonitor.Value : throw new ArgumentNullException(nameof(optionsMonitor)))
	{
	}

	internal InnerLogMessageQueue(
		TOptions optionsValue)
	{
		_ = optionsValue ?? throw new ArgumentNullException(nameof(optionsValue));

		_maxQueueSize = optionsValue.MaxQueueSize ?? 0;
		_queueFullBehavior = optionsValue.QueueFullBehavior;

		if (_maxQueueSize > 0 && _queueFullBehavior == QueueFullBehavior.Wait)
		{
			// When the options are set to use bounded queue with wait-to-enqueue-when-full behavior we'll use our own counter semaphores.
			// Done this way because the out-of-the-box solutions don't cover a mix of sync enqueue and async dequeue behavior. They are all
			// either fully sync (BlockingCollection) or fully async (channels).
			_boundedQueue = new ConcurrentQueue<TMessageEntry>();
			_consumersCancellationTokenSource = new CancellationTokenSource();
			_freeNodes = new SemaphoreSlim(_maxQueueSize);
			_occupiedNodes = new SemaphoreSlim(0);
			_producersCancellationTokenSource = new CancellationTokenSource();

			_queueFullWaitTimeoutThreshold = optionsValue.QueueFullWaitTimeoutThreshold ?? DefaultQueueFullWaitTimeoutThreshold;
			if (_queueFullWaitTimeoutThreshold < -1)
				_queueFullWaitTimeoutThreshold = DefaultQueueFullWaitTimeoutThreshold;
		}
		else if (_maxQueueSize > 0)
		{
			var channelOptions = new BoundedChannelOptions(_maxQueueSize)
			{
				AllowSynchronousContinuations = false,
				FullMode = GetBoundedChannelFullMode(optionsValue.QueueFullBehavior),
				SingleReader = true
			};
			_messageChannel = Channel.CreateBounded<TMessageEntry>(channelOptions);
		}
		else
		{
			var channelOptions = new UnboundedChannelOptions()
			{
				AllowSynchronousContinuations = false,
				SingleReader = true
			};
			_messageChannel = Channel.CreateUnbounded<TMessageEntry>(channelOptions);

			_messageChannelReader = _messageChannel.Reader;
			_messageChannelWriter = _messageChannel.Writer;
		}
	}


	/// <summary>
	/// Whether or not the queue has been marked as complete for adding to it.
	/// </summary>
	public bool IsAddingCompleted { get; private set; }


	/// <summary>
	/// Mark the underlying channel as done writing.
	/// </summary>
	public void CompleteAdding()
	{
		if (IsAddingCompleted)
			return;

		_consumersCancellationTokenSource?.Cancel();
		_producersCancellationTokenSource?.Cancel();
		if (_messageChannelWriter is not null)
		{
			if (_messageChannelWriter.TryComplete())
			{
				IsAddingCompleted = true;
			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				CompleteAdding();
				_consumersCancellationTokenSource?.Dispose();
				_producersCancellationTokenSource?.Dispose();
				_freeNodes?.Dispose();
				_freeNodes = null;
				_occupiedNodes?.Dispose();
				_occupiedNodes = null;
			}

			_isDisposed = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Return the next item to consume or wait asynchronously until an item becomes available.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async Task<TMessageEntry?> DequeueOrWaitAsync(CancellationToken cancellationToken)
	{
		if (_isDisposed)
			throw new ObjectDisposedException(nameof(InnerLogMessageQueue<TMessageEntry, TOptions>));

		if (_messageChannelReader is not null)
			return await _messageChannelReader.ReadAsync(cancellationToken).ConfigureAwait(false);


		Debug.Assert(_boundedQueue is not null);
		Debug.Assert(_consumersCancellationTokenSource is not null);
		Debug.Assert(_freeNodes is not null);
		Debug.Assert(_occupiedNodes is not null);

		using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _consumersCancellationTokenSource!.Token);
		await _occupiedNodes!.WaitAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);

		// We've already decremented the _occupiedNodes so we want to access _boundedQueue directly and not go through our own TryDequeue method which would decrement it a 2nd time
		if (_boundedQueue!.TryDequeue(out var dequeuedItem))
		{
			// Update the counts
			_ = _freeNodes!.Release();
			return dequeuedItem;
		}

		return default;
	}

	/// <inheritdoc/>
	public void EnqueueMessage(TMessageEntry messageEntry)
	{
		if (_isDisposed)
			throw new ObjectDisposedException(nameof(InnerLogMessageQueue<TMessageEntry, TOptions>));

		_ = messageEntry ?? throw new ArgumentNullException(nameof(messageEntry));

		if (IsAddingCompleted)
			return;

		if (_messageChannelWriter is not null)
		{
			_ = _messageChannelWriter.TryWrite(messageEntry);
			return;
		}

		Debug.Assert(_boundedQueue is not null);
		Debug.Assert(_freeNodes is not null);
		Debug.Assert(_occupiedNodes is not null);
		Debug.Assert(_producersCancellationTokenSource is not null);
		var producersCancellationToken = _producersCancellationTokenSource!.Token;

		bool waitForSemaphoreWasSuccessful;
		try
		{
			waitForSemaphoreWasSuccessful = _freeNodes!.Wait(0, default);
			if (!waitForSemaphoreWasSuccessful && _queueFullWaitTimeoutThreshold > 0)
				waitForSemaphoreWasSuccessful = _freeNodes.Wait(_queueFullWaitTimeoutThreshold, producersCancellationToken);
		}
		catch (OperationCanceledException)
		{
			// CompleteAdding was called, nothing else to do here
			return;
		}

		if (!waitForSemaphoreWasSuccessful)
			return;

		try
		{
			producersCancellationToken.ThrowIfCancellationRequested();
			_boundedQueue!.Enqueue(messageEntry);
			_ = _occupiedNodes!.Release();
		}
		catch (OperationCanceledException)
		{
			// Increment freeNodes back up by one since we failed to enqueue anything
			_ = _freeNodes.Release();
			return;
		}
		catch
		{
			_ = _freeNodes.Release();
			throw;
		}
	}

	/// <summary>
	/// Map the <paramref name="queueFullBehavior"/> to the corresponding <see cref="BoundedChannelFullMode"/> value
	/// </summary>
	internal static BoundedChannelFullMode GetBoundedChannelFullMode(QueueFullBehavior queueFullBehavior)
	{
		switch (queueFullBehavior)
		{
			case QueueFullBehavior.DropNewest:
				return BoundedChannelFullMode.DropNewest;

			case QueueFullBehavior.DropOldest:
				return BoundedChannelFullMode.DropOldest;

			case QueueFullBehavior.Wait:
				return BoundedChannelFullMode.Wait;

			case QueueFullBehavior.DontQueue:
			default:
				return BoundedChannelFullMode.DropWrite;
		}
	}

	/// <summary>
	/// Helper function to measure and calculate the remaining milliseconds until timeout
	/// </summary>
	internal static int GetRemainingTimeout(uint startTime, int originalWaitMillisecondsTimeout)
	{
		var elapsedMilliseconds = (((uint)Environment.TickCount) - startTime);
		if (elapsedMilliseconds > int.MaxValue)
			return 0;

		var remainingTimeoutThreshold = originalWaitMillisecondsTimeout - (int)elapsedMilliseconds;
		if (remainingTimeoutThreshold <= 0)
			return 0;

		return remainingTimeoutThreshold;
	}

	/// <summary>
	/// Handle the options being reloaded. Called directly by the QueueLoggerProvider to cut down on the number of options monitor change listener registrations needed.
	/// </summary>
	/// <param name="options">The reloaded options</param>
	public void HandleOptionsReload(TOptions options)
	{
		if (options is null)
			return;

		// QueueFullBehavior and MaxQueueSize do not support reloading at this time

		var queueFullWaitTimeoutThreshold = options.QueueFullWaitTimeoutThreshold ?? DefaultQueueFullWaitTimeoutThreshold;
		if (queueFullWaitTimeoutThreshold < -1)
			queueFullWaitTimeoutThreshold = DefaultQueueFullWaitTimeoutThreshold;

		_queueFullWaitTimeoutThreshold = queueFullWaitTimeoutThreshold;
	}

	/// <summary>
	/// Try to dequeue an item
	/// </summary>
#if !NET48 && !NETSTANDARD2_0
	public bool TryDequeue([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TMessageEntry? item)
#else
	public bool TryDequeue(out TMessageEntry? item)
#endif
	{
		if (_messageChannelReader is not null)
		{
			return _messageChannelReader.TryRead(out item);
		}

		Debug.Assert(_boundedQueue is not null);

		var didDequeue = _boundedQueue!.TryDequeue(out item);
		if (didDequeue)
		{
			// Update the counts
			_ = _freeNodes?.Release();
			_ = _occupiedNodes?.Wait(0, default);
		}

		return didDequeue;
	}
}
