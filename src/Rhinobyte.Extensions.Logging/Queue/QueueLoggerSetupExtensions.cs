using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// Extension methods for the logging builder and service collection.
/// <para>
/// Custom providers derived from the <see cref="QueueLoggerProvider{TMessageEntry, TMessageFormatter, TOptions}"/> can re-use these to dry up their own extension methods.
/// </para>
/// </summary>
public static class QueueLoggerSetupExtensions
{
	/// <summary>
	/// Extension method used to register a subclass logging provider that utilizes the QueueLoggerProvider base code.
	/// </summary>
	public static ILoggingBuilder AddBackgroundTaskQueueProvider<TProvider, TProcessor, TMessageFormatter, TMessageEntry, TOptions>(
		this ILoggingBuilder loggingBuilder)
		where TProvider : QueueLoggerProvider<TMessageEntry, TMessageFormatter, TOptions>
		where TProcessor : class, ISyncLogMessageProcessor<TMessageEntry, TOptions>
		where TMessageFormatter : class, ILogMessageFormatter<TMessageEntry, TOptions>
		where TOptions : QueueLoggerOptions
	{
		_ = loggingBuilder ?? throw new ArgumentNullException(nameof(loggingBuilder));

		loggingBuilder.AddConfiguration();

		loggingBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILogMessageFormatter<TMessageEntry, TOptions>, TMessageFormatter>());
		loggingBuilder.Services.TryAddSingleton<ISyncLogMessageProcessor<TMessageEntry, TOptions>, TProcessor>();
		loggingBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TProvider>());
		LoggerProviderOptions.RegisterProviderOptions<TOptions, TProvider>(loggingBuilder.Services);

		return loggingBuilder;
	}
}
