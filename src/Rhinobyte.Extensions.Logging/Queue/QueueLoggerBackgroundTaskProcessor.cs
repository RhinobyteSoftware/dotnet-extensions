using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// An implementation of the ILogMessageQueue that internally handles processing of the log message queue using a background task started via <see cref="Task.Run(Action)"/>
/// </summary>
/// <typeparam name="TMessageEntry">The log message entry type</typeparam>
/// <typeparam name="TOptions">The logger options type</typeparam>
public class QueueLoggerBackgroundTaskProcessor<TMessageEntry, TOptions> : ILogMessageQueue<TMessageEntry, TOptions>, IBackgroundProcessor, IDisposable
	where TOptions : QueueLoggerOptions
{
	private int? _batchSize;
	private CancellationTokenSource? _cancellationTokenSource;
	private readonly List<TMessageEntry> _currentBatch = [];
	private bool _isDisposed;
	private readonly IAsyncLogMessageProcessor<TMessageEntry, TOptions> _logMessageProcessor;
	private int? _loopDelayInterval;
	private readonly InnerLogMessageQueue<TMessageEntry, TOptions> _messageQueue;
	private TimeSpan _processRemainingTimeoutThreshold;
	private Task? _processQueueTask;

	/// <summary>
	/// Intantiate a new instance of the QueueLoggerBackgroundTaskProcessor.
	/// </summary>
	public QueueLoggerBackgroundTaskProcessor(
		IAsyncLogMessageProcessor<TMessageEntry, TOptions> logMessageProcessor,
		IOptions<TOptions> options)
	{
		_logMessageProcessor = logMessageProcessor ?? throw new ArgumentNullException(nameof(logMessageProcessor));

		_ = options ?? throw new ArgumentNullException(nameof(options));

		if (options.Value is null) throw new ArgumentException($"{nameof(options)}.{nameof(options.Value)} is null");

		_messageQueue = new InnerLogMessageQueue<TMessageEntry, TOptions>(options.Value);

		HandleOptionsReload(options.Value);
	}

	/// <summary>
	/// True if the processor background task is active, false otherwise.
	/// </summary>
	public bool IsProcessing { get; private set; }

	/// <summary>
	/// Overridable cleanup code method for the standard dispose pattern.
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				if (IsProcessing)
					StopProcessing(true);

				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
				_messageQueue.Dispose();
				_processQueueTask?.Dispose();
				_processQueueTask = null;
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
	/// Enqueue the message entry into queue to be processed by the background task.
	/// </summary>
	/// <remarks>
	/// Does not enqueue the message entry if the <see cref="BlockingCollection{T}"/> backing field is null or has <see cref="BlockingCollection{T}.IsAddingCompleted"/> signaled.
	/// </remarks>
	/// <param name="messageEntry">The log message entry to enqueue</param>
	/// <exception cref="ObjectDisposedException"></exception>
	public void EnqueueMessage(TMessageEntry messageEntry)
	{
		// If the queue isn't active, ignore the message
		if (_messageQueue is not null && !_messageQueue.IsAddingCompleted)
		{
			try
			{
				_messageQueue.EnqueueMessage(messageEntry);
				return;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Debug.Write(exc);
			}
		}

		try
		{
			if (_logMessageProcessor is ISyncLogMessageProcessor<TMessageEntry, TOptions> syncLogMessageProcessor)
				syncLogMessageProcessor.ProcessLogMessageEntry(messageEntry, default);
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
		{
			Debug.Write(exc);
		}
	}

	/// <summary>
	/// Handle the options being reloaded. Called by the QueueLoggerProvider to cut down on the number of options monitor change listener registrations needed.
	/// </summary>
	/// <param name="options">The reloaded options</param>
	public void HandleOptionsReload(TOptions options)
	{
		if (options is null)
			return;

		_batchSize = options.BackgroundProcessorBatchSize > 0
			? options.BackgroundProcessorBatchSize.Value
			: null;

		_loopDelayInterval = options.BackgroundProcessorLoopDelayInterval > TimeSpan.Zero
			? (int)options.BackgroundProcessorLoopDelayInterval.Value.TotalMilliseconds
			: null;

		_processRemainingTimeoutThreshold = options.ProcessRemainingTimeoutThreshold > TimeSpan.Zero
			? options.ProcessRemainingTimeoutThreshold.Value
			: TimeSpan.FromSeconds(60);

		// Call ReloadLoggerOptions on the wrapped InnerLogMessageQueue as well
		_messageQueue.HandleOptionsReload(options);
	}

	private async Task ProcessMessageQueueAsync()
	{
		Debug.Assert(_cancellationTokenSource is not null);

		var cancellationToken = _cancellationTokenSource!.Token;
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				if (_loopDelayInterval is not null)
					await Task.Delay(_loopDelayInterval.Value, cancellationToken).ConfigureAwait(false);

				if (cancellationToken.IsCancellationRequested)
					return;

				var message = await _messageQueue.DequeueOrWaitAsync(cancellationToken).ConfigureAwait(false);
				if (message is null)
					continue;

				if (_batchSize is null)
				{
					await _logMessageProcessor.ProcessLogMessageEntryAsync(message, cancellationToken).ConfigureAwait(false);
					continue;
				}

				_currentBatch.Add(message);
				var limit = _batchSize;
				while (limit > 0 && _messageQueue!.TryDequeue(out message))
				{
					_currentBatch.Add(message!);
					--limit;
				}

				await _logMessageProcessor.ProcessLogMessageEntriesAsync(_currentBatch, cancellationToken).ConfigureAwait(false);
				_currentBatch.Clear();
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Debug.Write(exc);
				if (_batchSize is not null)
					_currentBatch.Clear();
			}
		}
	}

	/// <summary>
	/// Initializes the necessary state and starts the background processing task using <see cref="Task.Run(Action)"/>
	/// </summary>
	/// <exception cref="ObjectDisposedException"></exception>
	public void StartProcessing()
	{
		if (_isDisposed)
			throw new ObjectDisposedException(this.GetType().Name);

		if (IsProcessing || _processQueueTask is not null)
			return;

		_cancellationTokenSource ??= new CancellationTokenSource();

		// TODO: Is it better to use Task.Run() here or to directly call the async method?
		_processQueueTask = Task.Run(ProcessMessageQueueAsync, _cancellationTokenSource.Token);

		IsProcessing = true;
	}

	/// <summary>
	/// Signales the stateful members to stop processing and disposes the stateful fields.
	/// </summary>
	/// <param name="flushRemaining">When true is passed the stop processing method will attempt to call the <see cref="TryFlushRemaining"/> method to process the remaining log message entries in the queue</param>
	/// <exception cref="ObjectDisposedException"></exception>
	public void StopProcessing(bool flushRemaining)
	{
		if (_isDisposed)
			return;

		if (!IsProcessing || _processQueueTask is null)
			return;

		_messageQueue!.CompleteAdding();

		_cancellationTokenSource!.Cancel();

		try
		{
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
			if (!_processQueueTask.IsCompleted)
				_ = _processQueueTask.Wait(_loopDelayInterval ?? 1500);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
		}
		catch (OperationCanceledException)
		{ }
		catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
		{
		}

		try
		{
			//if (flushRemaining && !_messageQueue.IsAddingCompleted)
			if (flushRemaining)
				Task.Run(TryFlushRemaining).ConfigureAwait(false).GetAwaiter().GetResult();
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
		{
			Debug.WriteLine(exc);
		}

		try
		{
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
			_messageQueue.Dispose();
			//_messageQueue = null;
			_processQueueTask.Dispose();
			_processQueueTask = null;
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
		{
			Debug.Write(exc);
		}

		IsProcessing = false;
	}

	private async Task TryFlushRemaining()
	{
		var limit = _batchSize;
		while (limit > 0 && _messageQueue!.TryDequeue(out var message))
		{
			_currentBatch.Add(message!);
			--limit;
		}

		if (_currentBatch.Count < 1)
			return;

		try
		{
			using var timeoutCancellationTokenSource = new CancellationTokenSource(_processRemainingTimeoutThreshold);
			await _logMessageProcessor.ProcessLogMessageEntriesAsync(_currentBatch, timeoutCancellationTokenSource.Token).ConfigureAwait(false);
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
		{
			Debug.Write(exc);
		}

		_currentBatch.Clear();
	}
}
