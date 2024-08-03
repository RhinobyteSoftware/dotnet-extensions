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
public class QueueLoggerBackgroundThreadProcessor<TMessageEntry, TOptions> : ILogMessageQueue<TMessageEntry, TOptions>, IDisposable
	where TOptions : QueueLoggerOptions
{
	private int? _batchSize;
	private readonly CancellationTokenSource _cancellationTokenSource;
	private readonly List<TMessageEntry> _currentBatch = [];
	private bool _isDisposed;
	private readonly ISyncLogMessageProcessor<TMessageEntry, TOptions> _logMessageProcessor;
	private readonly BlockingCollection<TMessageEntry> _messageQueue;
	private TimeSpan _processRemainingTimeoutThreshold;
	private readonly Thread _processQueueThread;

	/// <summary>
	/// Intantiate a new instance of the QueueLoggerBackgroundTaskProcessor.
	/// </summary>
	public QueueLoggerBackgroundThreadProcessor(
		ISyncLogMessageProcessor<TMessageEntry, TOptions> logMessageProcessor,
		IOptions<TOptions> options)
	{
		_logMessageProcessor = logMessageProcessor ?? throw new ArgumentNullException(nameof(logMessageProcessor));

		_ = options ?? throw new ArgumentNullException(nameof(options));

		if (options.Value is null) throw new ArgumentException($"{nameof(options)}.{nameof(options.Value)} is null");

		_cancellationTokenSource = new CancellationTokenSource();
		_messageQueue = new BlockingCollection<TMessageEntry>(options.Value.MaxQueueSize ?? 1024);

		HandleOptionsReload(options.Value);

		_processQueueThread = new Thread(ProcessMessageQueue)
		{
			IsBackground = true,
			Name = "Console logger queue processing thread"
		};
		_processQueueThread.Start();
	}

	/// <summary>
	/// Overridable cleanup code method for the standard dispose pattern.
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
				_messageQueue.CompleteAdding();

				try
				{
					_ = _processQueueThread.Join(1500);
				}
				catch (ThreadStateException)
				{ /* Ignored */ }

				TryFlushRemaining();

				_cancellationTokenSource.Dispose();
				_messageQueue?.Dispose();
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
		if (!_messageQueue.IsAddingCompleted)
		{
			try
			{
				_messageQueue.Add(messageEntry);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (InvalidOperationException exc)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Debug.Write(exc);
			}
		}

		try
		{
			_logMessageProcessor.ProcessLogMessageEntry(messageEntry, default);
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

		_processRemainingTimeoutThreshold = options.ProcessRemainingTimeoutThreshold > TimeSpan.Zero
			? options.ProcessRemainingTimeoutThreshold.Value
			: TimeSpan.FromSeconds(60);
	}

	private void ProcessMessageQueue()
	{
		Debug.Assert(_cancellationTokenSource is not null);

		while (!_cancellationTokenSource!.IsCancellationRequested)
		{
			try
			{
				if (_cancellationTokenSource.IsCancellationRequested)
					return;

				if (!_messageQueue.TryTake(out var message, Timeout.Infinite, _cancellationTokenSource.Token) || message is null)
					continue;

				if (_batchSize is null)
				{
					_logMessageProcessor.ProcessLogMessageEntry(message, _cancellationTokenSource.Token);
					continue;
				}

				_currentBatch.Add(message);
				var limit = _batchSize;
				while (limit > 0 && _messageQueue!.TryTake(out message))
				{
					_currentBatch.Add(message!);
					--limit;
				}

				_logMessageProcessor.ProcessLogMessageEntries(_currentBatch, _cancellationTokenSource.Token);
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

	private void TryFlushRemaining()
	{
		try
		{
			if (_messageQueue.IsCompleted)
				return;

			var limit = _batchSize ?? 1024;
			while (limit > 0 && _messageQueue!.TryTake(out var message))
			{
				_currentBatch.Add(message!);
				--limit;
			}

			if (_currentBatch.Count < 1)
				return;

			using var timeoutCancellationTokenSource = new CancellationTokenSource(_processRemainingTimeoutThreshold);
			_logMessageProcessor.ProcessLogMessageEntries(_currentBatch, timeoutCancellationTokenSource.Token);

			_currentBatch.Clear();
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
		{
			Debug.Write(exc);
		}
	}
}
