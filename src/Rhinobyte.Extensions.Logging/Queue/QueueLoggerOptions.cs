using System;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// Options for a <see cref="QueueLogger{TMessageEntry}"/>
/// </summary>
public class QueueLoggerOptions
{
	/// <summary>
	/// An optional batch size of log message entries for the background processor to handle in a group.
	/// <para>
	/// When not null the background processor loop will attempt to dequeue log message entries until no entires are available or until the batch size is reached. It will then pass the entire
	/// batch collection of log message entries to the <see cref="IAsyncLogMessageProcessor{TMessageEntry, TOptions}.ProcessLogMessageEntriesAsync(System.Collections.Generic.ICollection{TMessageEntry}, System.Threading.CancellationToken)"/>
	/// method as a single call.
	/// </para>
	/// <para>
	/// This option is generally intended to be used in conjunction with the optional <see cref="BackgroundProcessorLoopDelayInterval"/> value in order to process batches at fixed intervals.
	/// </para>
	/// </summary>
	public int? BackgroundProcessorBatchSize { get; set; }

	/// <summary>
	/// An optional delay interval between iterations of the background processing loop.
	/// <para>
	/// When not null the background processor loop will delay for the specified interval before the next attempt to wait on the async try dequeue method.
	/// </para>
	/// <para>
	/// This option is generally intended to be used in conjunction with the optional <see cref="BackgroundProcessorBatchSize"/> option in order to process batches of log message entries at fixed intervals instead of
	/// continuously processing items as they become available. 
	/// </para>
	/// <para>
	/// This option will be ignored if using the <see cref="QueueLoggerProcessorType.BackgroundThread"/> processor type.
	/// </para>
	/// </summary>
	public TimeSpan? BackgroundProcessorLoopDelayInterval { get; set; }

	/// <summary>
	/// Name of the log message formatter to use.
	/// </summary>
	public string? FormatterName { get; set; }

	/// <summary>
	/// An optional value indicating the maximum upper bound of log message entries that can be held in the log message queue.
	/// <para>
	/// When not null the <see cref="QueueFullBehavior"/> will be used to determine how <see cref="ILogMessageQueue{TMessageEntry}.EnqueueMessage(TMessageEntry)"/> behaves if the queue has reached the max queue size.
	/// </para>
	/// <para>
	/// <strong>IMPORTANT NOTE:</strong> This option can only be specified at the time of the initial logging provider setup. Configuration reload changes will be ignored for this option.
	/// </para>
	/// </summary>
	/// <remarks>
	/// A value less than one will be ignored and treated as if the property is null.
	/// </remarks>
	public int? MaxQueueSize { get; set; }

	/// <summary>
	/// The timeout threshold allowed for the background processor types to attempt processing any remaining log message entries from the queue when the background prcoess or is stopping or being disposed.
	/// </summary>
	public TimeSpan? ProcessRemainingTimeoutThreshold { get; set; }

	/// <summary>
	/// The behavior that will be applied when <see cref="MaxQueueSize"/> is not null and the max queue size has been reached.
	/// <para>
	/// <strong>IMPORTANT NOTE:</strong> This option can only be specified at the time of the initial logging provider setup. Configuration reload changes will be ignored for this option.
	/// </para>
	/// </summary>
	public QueueFullBehavior QueueFullBehavior { get; set; }

	/// <summary>
	/// Because the <see cref="ILogMessageQueue{TMessageEntry}.EnqueueMessage(TMessageEntry)"/> must be synchronous to be used
	/// in the ILogger.Log method we have to block the thread ourselves when using a bounded queue and the <see cref="QueueFullBehavior.Wait"/> option.
	/// In such a scenario this value is is used to specify the maximum time in milliseconds to wait to enqueue the message.
	/// <para>
	/// A null or negative value will be ignored and a default wait of 1 minute will be used instead.
	/// </para>
	/// </summary>
	public int? QueueFullWaitTimeoutThreshold { get; set; }
}
