namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// An interface representing a queue of log message entries to be processed separately without blocking the thread that made the logging call
/// </summary>
/// <typeparam name="TMessageEntry">The type of the message entry stored in the queue</typeparam>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface ILogMessageQueue<TMessageEntry>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
	/// <summary>
	/// Enqueue a message entry to be processed separately without blocking the thread that made the log call
	/// </summary>
	/// <param name="messageEntry">The message entry to enqueue</param>
	void EnqueueMessage(TMessageEntry messageEntry);
}

/// <inheritdoc/>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface ILogMessageQueue<TMessageEntry, TOptions> : ILogMessageQueue<TMessageEntry>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
	where TOptions : QueueLoggerOptions
{
	/// <summary>
	/// Handle the options being reloaded. Called by the QueueLoggerProvider to cut down on the number of options monitor change listener registrations needed.
	/// </summary>
	/// <param name="options">The reloaded options</param>
	void HandleOptionsReload(TOptions options);
}

/// <summary>
/// An interface for the QueueLoggerProvider to identify if the queue type is also a background processor that needs to be started and stopped.
/// </summary>
public interface IBackgroundProcessor
{
	/// <summary>
	/// Start the background processing loop.
	/// </summary>
	void StartProcessing();

	/// <summary>
	/// Stop the background processing loop.
	/// </summary>
	/// <param name="flushRemaining">If true then the stop routine will attemmpt to process any remaining log message entries in the queue.</param>
	void StopProcessing(bool flushRemaining);
}
