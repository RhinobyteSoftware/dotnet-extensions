namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// The background processor type to use for the queue logger message entry processor.
/// </summary>
public enum QueueLoggerProcessorType
{
	/// <summary>
	/// The message entry processor type that uses a background task.
	/// </summary>
	BackgroundTask = 0,

	/// <summary>
	/// The message entry processor type that uses a background thread.
	/// </summary>
	BackgroundThread = 1
}
