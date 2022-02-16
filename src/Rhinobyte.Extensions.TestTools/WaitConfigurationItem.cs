namespace Rhinobyte.Extensions.TestTools;

/// <summary>
/// Wait/delay configuration item values for a given category key
/// </summary>
public class WaitConfigurationItem
{
	/// <summary>
	/// The value to use for the delay or the initial delay value to use before first checking the wait condition
	/// </summary>
	public int? Delay { get; set; }

	/// <summary>
	/// The polling interval to wait for before subsequent checks of the wait condition
	/// </summary>
	public int? PollingInterval { get; set; }

	/// <summary>
	/// The timeout threshold of the wait condition
	/// </summary>
	public int? TimeoutInterval { get; set; }
}
