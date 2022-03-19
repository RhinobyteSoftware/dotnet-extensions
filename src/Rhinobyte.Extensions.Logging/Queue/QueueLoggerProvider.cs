using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// Abstract base class for custom logging providers that need to use a message queue in order to process the log messages on a separate thread without blocking the calling thread.
/// </summary>
/// <typeparam name="TMessageEntry">The type of the log message entry constructed by the log message formatter</typeparam>
/// <typeparam name="TMessageFormatter">The type of the <see cref="ILogMessageFormatter{TMessageEntry}"/> used by the derived logger provider type</typeparam>
/// <typeparam name="TOptions">The type of the <see cref="QueueLoggerOptions"/> used by the derived logger provider type</typeparam>
public abstract class QueueLoggerProvider<TMessageEntry, TMessageFormatter, TOptions> : ILoggerProvider, ISupportExternalScope
	where TMessageFormatter : ILogMessageFormatter<TMessageEntry>
	where TOptions : QueueLoggerOptions
{
	private readonly string _defaultFormatterName;
	private ConcurrentDictionary<string, TMessageFormatter> _formatters;
	private bool _isDisposed;
	private readonly ConcurrentDictionary<string, QueueLogger<TMessageEntry>> _loggers;
	private readonly IOptionsMonitor<TOptions> _optionsMonitor;
	private readonly ILogMessageQueue<TMessageEntry, TOptions> _messageQueue;
	private readonly IDisposable _optionsReloadToken;
	/// Ok to default to NullScopeProvider, the LoggerFactory will automatically set this to the LoggerFactoryScopeProvider because we implement ISupportExternalScope
	private IExternalScopeProvider _scopeProvider = NullScopeProvider.Instance;

	/// <summary>
	/// Base constructor for a <see cref="QueueLoggerProvider{TMessageEntry, TMessageFormatter, TOptions}"/> implementation
	/// </summary>
	/// <param name="defaultFormatterName">The default log formatter name to use in the event the options property is null or invalid</param>
	/// <param name="logMessageFormatters">The collection of registered log message formatters for the queue logger implementation to use</param>
	/// <param name="optionsMonitor">The queue logger options monitor</param>
	/// <param name="messageQueue">The message queue that the loggers will pump message entries into</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	protected QueueLoggerProvider(
		string defaultFormatterName,
		IEnumerable<TMessageFormatter> logMessageFormatters,
		IOptionsMonitor<TOptions> optionsMonitor,
		ILogMessageQueue<TMessageEntry, TOptions> messageQueue)
	{
		if (optionsMonitor is null) throw new ArgumentNullException(nameof(optionsMonitor));
		if (optionsMonitor.CurrentValue is null) throw new ArgumentException($"{nameof(optionsMonitor)}.{nameof(optionsMonitor.CurrentValue)} is null");
		_optionsMonitor = optionsMonitor;

		_messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));

		_loggers = new ConcurrentDictionary<string, QueueLogger<TMessageEntry>>();
		_defaultFormatterName = defaultFormatterName;

#if !NET5_0_OR_GREATER
		_formatters = null!; // Nullable reference hack since the [MemberNotNull(..)] attribute isn't publicy accessible until net5.0 and higher... *sigh*
#endif
		SetFormatters(defaultFormatterName, logMessageFormatters);

		HandleOptionsReload(_optionsMonitor.CurrentValue);
		_optionsReloadToken = _optionsMonitor.OnChange(HandleOptionsReload);

		if (_messageQueue is IBackgroundProcessor backgroundProcessorQueue)
			backgroundProcessorQueue.StartProcessing();
	}

	/// <summary>
	/// Gets the cached instance or create a new instance of the <see cref="QueueLogger{TMessageEntry}"/> for the requested logger <paramref name="categoryName"/>
	/// </summary>
	/// <returns>
	/// The <see cref="ILogger"/> instance
	/// </returns>
	public ILogger CreateLogger(string categoryName)
	{
		var formatterName = _optionsMonitor.CurrentValue.FormatterName;
		if (formatterName == null || !_formatters.TryGetValue(formatterName, out var logMessageFormatter))
			logMessageFormatter = _formatters[_defaultFormatterName];

		return _loggers.TryGetValue(categoryName, out var logger)
			? logger
			: _loggers.GetOrAdd(categoryName, new QueueLogger<TMessageEntry>(categoryName, logMessageFormatter, _messageQueue, _scopeProvider));
	}


	/// <summary>
	/// Overridable cleanup code portion of the dispose pattern.
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				if (_messageQueue is IBackgroundProcessor backgroundProcessorQueue)
					backgroundProcessorQueue.StopProcessing(true);

				_optionsReloadToken?.Dispose();
			}

			_isDisposed = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Callback for the <see cref="OptionsMonitorExtensions.OnChange{TOptions}(IOptionsMonitor{TOptions}, Action{TOptions})"/> listener registration.
	/// </summary>
	/// <remarks>
	/// warning:  ReloadLoggerOptions can be called before the constructor call has completed. Before registering the listener or calling this method, all of the stateful members this method depends on need to be initialized
	/// </remarks>
	protected void HandleOptionsReload(TOptions options)
	{
		_ = options ?? throw new ArgumentNullException(nameof(options));

		if (options.FormatterName == null || !_formatters.TryGetValue(options.FormatterName, out var logMessageFormatter))
			logMessageFormatter = _formatters[_defaultFormatterName];

		foreach (var logger in _loggers)
		{
			logger.Value.LogMessageFormatter = logMessageFormatter;
		}

		_messageQueue.HandleOptionsReload(options);
	}

#if NET5_0_OR_GREATER
	[MemberNotNull(nameof(_formatters))]
#endif
	private void SetFormatters(string defaultFormatterName, IEnumerable<TMessageFormatter> logMessageFormatters)
	{
		if (string.IsNullOrWhiteSpace(defaultFormatterName)) throw new ArgumentException($"{nameof(defaultFormatterName)} cannot be null or whitespace.", nameof(defaultFormatterName));
		if (logMessageFormatters is null) throw new ArgumentNullException(nameof(logMessageFormatters));
		if (!logMessageFormatters.Any()) throw new ArgumentException($"{nameof(logMessageFormatters)} must have at least one item", nameof(logMessageFormatters));

		var formattersDictionary = new ConcurrentDictionary<string, TMessageFormatter>(StringComparer.OrdinalIgnoreCase);
		foreach (var formatter in logMessageFormatters)
			_ = formattersDictionary.TryAdd(formatter.Name, formatter);

		if (!formattersDictionary.ContainsKey(defaultFormatterName))
			throw new ArgumentException($"{nameof(logMessageFormatters)} must contain a formatter for the supplied default formatter name: {defaultFormatterName}");
		_formatters = formattersDictionary;
	}

	/// <inheritdoc />
	public void SetScopeProvider(IExternalScopeProvider scopeProvider)
	{
		_scopeProvider = scopeProvider;

		foreach (var logger in _loggers)
			logger.Value.ScopeProvider = _scopeProvider;
	}
}
