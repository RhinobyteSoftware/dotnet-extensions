using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// Queue logger abstract base class used to format and enqueue log message entries for deferred processing
/// </summary>
/// <typeparam name="TMessageEntry">The type of message entry that will be enqueued by this logger</typeparam>
public class QueueLogger<TMessageEntry> : ILogger
{
	private readonly string _categoryName;
	private readonly ILogMessageQueue<TMessageEntry> _messageQueue;

	[ThreadStatic]
	private static StringWriter? _threadStaticStringWriter;

	/// <summary>
	/// Construct a queue logger instance.
	/// </summary>
	public QueueLogger(
		string categoryName,
		ILogMessageFormatter<TMessageEntry> logMessageFormatter,
		ILogMessageQueue<TMessageEntry> messageQueue,
		IExternalScopeProvider scopeProvider)
	{
		_categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
		LogMessageFormatter = logMessageFormatter ?? throw new ArgumentNullException(nameof(logMessageFormatter));
		_messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
		ScopeProvider = scopeProvider; // Null scope provider allowed
	}

	internal ILogMessageFormatter<TMessageEntry> LogMessageFormatter { get; set; }
	internal IExternalScopeProvider ScopeProvider { get; set; }

	/// <inheritdoc/>
	public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

	/// <inheritdoc/>
	public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

	/// <summary>
	/// Constructs a log message entry using the configured <see cref="LogMessageFormatter"/> and enqueue 
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	/// <param name="logLevel"></param>
	/// <param name="eventId"></param>
	/// <param name="state"></param>
	/// <param name="exception"></param>
	/// <param name="formatter"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		_ = formatter ?? throw new ArgumentNullException(nameof(formatter));

		var logEntry = new LogEntry<TState>(logLevel, _categoryName, eventId, state, exception, formatter);

		TMessageEntry? messageEntry;
		if (!LogMessageFormatter.IsTextWriterNeeded)
		{
			messageEntry = LogMessageFormatter.FormatEntry(logEntry, ScopeProvider, null);
		}
		else
		{
			_threadStaticStringWriter ??= new StringWriter();
			messageEntry = LogMessageFormatter.FormatEntry(logEntry, ScopeProvider, _threadStaticStringWriter);
		}

		if (messageEntry is not null)
			_messageQueue.EnqueueMessage(messageEntry);

		if (_threadStaticStringWriter is not null)
		{
			var stringBuilder = _threadStaticStringWriter.GetStringBuilder();
			_ = stringBuilder.Clear();

			var capacityResetValue = LogMessageFormatter.IsTextWriterNeeded ? 1024 : 0;
			if (stringBuilder.Capacity > capacityResetValue)
				stringBuilder.Capacity = capacityResetValue;
		}
	}
}
