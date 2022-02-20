using System.Threading.Channels;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// The behavior to use when <see cref="ILogMessageQueue{TMessageEntry}.EnqueueMessage(TMessageEntry)"/> is called and the queue has already reached the <see cref="QueueLoggerOptions.MaxQueueSize" />
/// </summary>
public enum QueueFullBehavior
{
	/// <summary>
	/// When the queue is full additional items will be ignored.
	/// <para>
	/// Equivalent to the <see cref="BoundedChannelFullMode.DropWrite"/> value
	/// </para>
	/// </summary>
	DontQueue = 0,

	/// <summary>
	/// Summary when the queue is full remove and ignore the newest item to make room for the item being written.
	/// <para>
	/// Equivalent to the <see cref="BoundedChannelFullMode.DropNewest"/> value
	/// </para>
	/// </summary>
	DropNewest = 1,

	/// <summary>
	/// Summary when the queue is full remove and ignore the newest item to make room for the item being written.
	/// <para>
	/// Equivalent to the <see cref="BoundedChannelFullMode.DropOldest"/> value
	/// </para>
	/// </summary>
	DropOldest = 2,

	/// <summary>
	/// When the queue is full the call to <see cref="ILogMessageQueue{TMessageEntry}.EnqueueMessage(TMessageEntry)"/> will try to block until space
	/// is available or the <see cref="QueueLoggerOptions.QueueFullWaitTimeoutThreshold"/> is reached.
	/// <para>
	/// Equivalent to the <see cref="BoundedChannelFullMode.Wait"/> value
	/// </para>
	/// </summary>
	Wait = 3
}
