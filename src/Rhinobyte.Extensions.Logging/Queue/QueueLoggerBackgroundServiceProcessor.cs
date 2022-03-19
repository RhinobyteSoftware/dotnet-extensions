// TODO: Decide if there's any value in including a hosted background service processor type like this...
// If so do we want to put it in this library and add a dependency to Microsoft.Extensions.Hosting.Abstractions or put it in a separate library to limit dependenices...

//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Rhinobyte.Extensions.Logging.Queue;

//public class QueueLoggerBackgroundServiceProcessor<TMessageEntry, TOptions> : BackgroundService, ILogMessageQueue<TMessageEntry, TOptions>, IDisposable
//	where TOptions : QueueLoggerOptions
//{
//	private bool isDisposed;
//	private readonly ILogMessageProcessor<TMessageEntry, TOptions> _logMessageProcessor;
//	private readonly InnerLogMessageQueue<TMessageEntry, TOptions> _messageQueue;
//	private readonly IOptionsMonitor<TOptions> _optionsMonitor;
//	private readonly IDisposable _optionsReloadToken;

//	public QueueLoggerBackgroundServiceProcessor(
//		ILogMessageProcessor<TMessageEntry, TOptions> logMessageProcessor,
//		InnerLogMessageQueue<TMessageEntry, TOptions> messageQueue,
//		IOptionsMonitor<TOptions> optionsMonitor)
//	{
//		_ = logMessageProcessor ?? throw new ArgumentNullException(nameof(logMessageProcessor));
//		_ = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));

//		if (optionsMonitor is null) throw new ArgumentNullException(nameof(optionsMonitor));
//		if (optionsMonitor.CurrentValue is null) throw new ArgumentException($"{nameof(optionsMonitor)}.{nameof(optionsMonitor.CurrentValue)} is null");
//		_optionsMonitor = optionsMonitor;

//		ReloadLoggerOptions(_optionsMonitor.CurrentValue);
//		_optionsReloadToken = _optionsMonitor.OnChange(ReloadLoggerOptions);
//	}

//	/// <summary>
//	/// Overridable cleanup code method for the standard dispose pattern.
//	/// </summary>
//	protected virtual void Dispose(bool disposing)
//	{
//		if (!isDisposed)
//		{
//			if (disposing)
//			{
//				// TODO: dispose managed state (managed objects)
//			}

//			isDisposed = true;
//		}
//	}

//	/// <inheritdoc />
//	public void Dispose()
//	{
//		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//		Dispose(disposing: true);
//		GC.SuppressFinalize(this);
//	}

///// <summary>
///// Handle the options being reloaded. Called by the QueueLoggerProvider to cut down on the number of options monitor change listener registrations needed.
///// </summary>
///// <param name="options">The reloaded options</param>
//public void HandleOptionsReload(TOptions options)
//{
//	if (options is null)
//		return;

//	_batchSize = options.BackgroundProcessorBatchSize > 0
//		? options.BackgroundProcessorBatchSize.Value
//		: null;

//	_loopDelayInterval = options.BackgroundProcessorLoopDelayInterval > TimeSpan.Zero
//		? (int)options.BackgroundProcessorLoopDelayInterval.Value.TotalMilliseconds
//		: null;

//	_processRemainingTimeoutThreshold = options.ProcessRemainingTimeoutThreshold > TimeSpan.Zero
//		? options.ProcessRemainingTimeoutThreshold.Value
//		: TimeSpan.FromSeconds(60);

//	// Call ReloadLoggerOptions on the wrapped InnerLogMessageQueue as well
//	_messageQueue.HandleOptionsReload(options);
//}

//protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//{
//while (!stoppingToken!.IsCancellationRequested)
//{
//	try
//	{
//		if (_loopDelayInterval is not null)
//			await Task.Delay(_loopDelayInterval.Value, stoppingToken).ConfigureAwait(false);

//		if (_cancellationTokenSource.IsCancellationRequested)
//			return;

//		var message = await _messageQueue.DequeueOrWaitAsync(stoppingToken).ConfigureAwait(false);
//		if (message is null)
//			continue;

//		if (_batchSize is null)
//		{
//			await _logMessageProcessor.ProcessLogMessageEntryAsync(message, stoppingToken).ConfigureAwait(false);
//			continue;
//		}

//		_currentBatch.Add(message);
//		var limit = _batchSize;
//		while (limit > 0 && _messageQueue!.TryDequeue(out message))
//		{
//			_currentBatch.Add(message!);
//			--limit;
//		}

//		try
//		{
//			await _logMessageProcessor.ProcessLogMessageEntriesAsync(_currentBatch, stoppingToken).ConfigureAwait(false);
//		}
//#pragma warning disable CA1031 // Do not catch general exception types
//		catch (Exception exc)
//#pragma warning restore CA1031 // Do not catch general exception types
//		{
//			Debug.Write(exc);
//		}

//		_currentBatch.Clear();
//	}
//#pragma warning disable CA1031 // Do not catch general exception types
//	catch (Exception exc)
//#pragma warning restore CA1031 // Do not catch general exception types
//	{
//		Debug.Write(exc);
//	}
//}
//}


