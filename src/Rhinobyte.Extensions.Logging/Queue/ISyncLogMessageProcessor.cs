using System.Collections.Generic;
using System.Threading;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// An interface representing the background processor actions used to process the log message entries from a <see cref="ILogMessageQueue{TMessageEntry}"/>
/// <para>
/// Implementations must implement this interface in order to be used with the <see cref="QueueLoggerProcessorType.BackgroundThread"/> processor type.
/// </para>
/// </summary>
/// <typeparam name="TMessageEntry">The type of the message entry stored in the queue</typeparam>
/// <typeparam name="TOptions">The logger options type used to differentiate the service registrations</typeparam>
public interface ISyncLogMessageProcessor<TMessageEntry, TOptions>
	where TOptions : QueueLoggerOptions
{
	/// <summary>
	/// Process a single <typeparamref name="TMessageEntry"/> instance from the queue
	/// </summary>
	void ProcessLogMessageEntry(TMessageEntry logMessageEntry, CancellationToken cancellationToken);

	/// <summary>
	/// Process a collection of <typeparamref name="TMessageEntry"/> instances from the queue
	/// <para>
	/// Used by the background task processor to process a batch of log message entries in a given flush interval
	/// </para>
	/// <para>
	/// Also used by the background processor types to flush all remaining entries in the queue when the background processor is stopping or being disposed
	/// </para>
	/// </summary>
	void ProcessLogMessageEntries(ICollection<TMessageEntry> logMessageEntries, CancellationToken cancellationToken);
}
