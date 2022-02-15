namespace Rhinobyte.Extensions.TestTools;

/// <summary>
/// A composite <see cref="WaitConfigurationItem"/> representing the values to use for a specific key taken from the most
/// specific category matches available
/// </summary>
public class WaitConfigurationCompositeItem : WaitConfigurationItem
{
	/// <summary>
	/// The key of the configuration item that the <see cref="WaitConfigurationItem.Delay"/> value was captured from
	/// </summary>
	public string? DelayItemKey { get; set; }

	/// <summary>
	/// The key of the configuration item that the <see cref="WaitConfigurationItem.PollingInterval"/> value was captured from
	/// </summary>
	public string? PollingIntervalKey { get; set; }

	/// <summary>
	/// The key of the configuration item that the <see cref="WaitConfigurationItem.TimeoutInterval"/> value was captured from
	/// </summary>
	public string? TimeoutIntervalKey { get; set; }


	/// <summary>
	/// Sets any properties on this instance that are still null to the property value from the <paramref name="itemToCopyFrom"/>
	/// </summary>
	/// <returns>true if all values are assigned, false otherwise</returns>
	public bool AssignMissingValues(WaitConfigurationItem itemToCopyFrom, string key)
	{
		if (itemToCopyFrom is null)
			return false;

		if (Delay is null && itemToCopyFrom.Delay is not null)
		{
			Delay = itemToCopyFrom.Delay;
			DelayItemKey = key;
		}

		if (PollingInterval is null && itemToCopyFrom.PollingInterval is not null)
		{
			PollingInterval = itemToCopyFrom.PollingInterval;
			PollingIntervalKey = key;
		}

		if (TimeoutInterval is null && itemToCopyFrom.TimeoutInterval is not null)
		{
			TimeoutInterval = itemToCopyFrom.TimeoutInterval;
			TimeoutIntervalKey = key;
		}

		return Delay is not null
			&& PollingInterval is not null
			&& TimeoutInterval is not null;
	}
}
