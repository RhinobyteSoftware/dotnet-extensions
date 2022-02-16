using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.TestTools;

/// <summary>
/// A collection of <see cref="WaitConfigurationItem"/> instances for use by the <see cref="WaitUtility"/>
/// </summary>
public class WaitConfiguration
{
	private readonly ConcurrentDictionary<string, WaitConfigurationCompositeItem> _waitConfigurationCache;
	private readonly ConcurrentDictionary<string, WaitConfigurationItem> _waitConfigurations;

	/// <summary>
	/// Construct a new WaitConfiguration instance
	/// </summary>
	public WaitConfiguration()
	{
		_waitConfigurations = new();
		_waitConfigurationCache = new();
	}

	/// <summary>
	/// Construct an instance of the WaitConfiguration that will use the specified dictionary of wait configurations
	/// </summary>
	/// <param name="waitConfigurationCache">An optional explicit dictionary to use for the configuration cache. If null a new dictionary will be instantiated</param>
	/// <param name="waitConfigurations">The dictionary of wait configuration category/item pairs to use for this configuration</param>
	public WaitConfiguration(
		ConcurrentDictionary<string, WaitConfigurationItem> waitConfigurations,
		ConcurrentDictionary<string, WaitConfigurationCompositeItem>? waitConfigurationCache = null)
	{
		_waitConfigurations = waitConfigurations ?? throw new ArgumentNullException(nameof(waitConfigurations));
		_waitConfigurationCache = waitConfigurationCache ?? new();
	}

	/// <summary>
	/// Construct a new WaitConfiguration instance and then copies the provided <paramref name="waitConfigurationItems"/> into the configurations dictionary
	/// </summary>
	/// <param name="waitConfigurationItems">The wait configuration category/item pairs to use for this configuration</param>
	public static WaitConfiguration FromDictionary(IDictionary<string, WaitConfigurationItem> waitConfigurationItems)
	{
		if (waitConfigurationItems is null)
			throw new ArgumentNullException(nameof(waitConfigurationItems));

		if (waitConfigurationItems.Count < 1)
			throw new ArgumentException($"{nameof(waitConfigurationItems)} cannot be an empty dictionary");

		return new WaitConfiguration(new ConcurrentDictionary<string, WaitConfigurationItem>(waitConfigurationItems));
	}

	/// <summary>
	/// Clear all entries from the configurations and the configurations cache dictionaries
	/// </summary>
	public void Clear()
	{
		_waitConfigurationCache.Clear();
		_waitConfigurations.Clear();
	}

	/// <summary>
	/// Clear all composite entries from the configuration cache
	/// </summary>
	public void ClearCache()
	{
		_waitConfigurationCache.Clear();
	}

	/// <summary>
	/// Find and return a wait configuration values to use for the specified <paramref name="waitId"/>
	/// <para>
	/// This method will search for configuration items moving from the most specific key category to the least specific key category.
	/// A composite item is constructed using the non-null value for the most specific category key
	/// </para>
	/// <para>Categories keys use a dot (.) character as a delimiter</para>
	/// <para>Example:</para>
	/// <para>For a waitId of <c>'Some.Specific.Wait.Id</c> this method would first look the exact key</para>
	/// <para>If no item was found it would then look for a key of <c>'Some.Specific.Wait'</c></para>
	/// <para>
	/// If a match is found but one of the properties, such as <see cref="WaitConfigurationItem.TimeoutInterval"/> is null, then this method
	/// will continue looking through the less specific keys for the remaining values that are still null
	/// </para>
	/// <para>
	/// If necessary, the method will check for a <c>'Default'</c> key last
	/// </para>
	/// </summary>
	/// <param name="waitId">The waitId to find the configuration item for</param>
	/// <returns>The wait configuration values for the most specific category matches found or null if no matches and no default were found</returns>
	public WaitConfigurationCompositeItem? FindWaitConfigurationValues(
		string? waitId)
	{
		if (_waitConfigurations.IsEmpty || string.IsNullOrWhiteSpace(waitId))
			return null;

		if (_waitConfigurationCache.TryGetValue(waitId!, out var waitConfigurationCompositeItem))
			return waitConfigurationCompositeItem;

		foreach (var keyPrefix in GetKeyPrefixes(waitId!))
		{
			if (!_waitConfigurations.TryGetValue(keyPrefix, out var waitConfigurationItem))
				continue;

			if (waitConfigurationCompositeItem is null)
				waitConfigurationCompositeItem = new WaitConfigurationCompositeItem();

			if (waitConfigurationCompositeItem.AssignMissingValues(waitConfigurationItem, keyPrefix))
				break; // Break if all values are set on the composite
		}

		if (waitConfigurationCompositeItem is not null)
			_ = _waitConfigurationCache.TryAdd(waitId!, waitConfigurationCompositeItem);

		return waitConfigurationCompositeItem;
	}

	/// <summary>
	/// Get the enumerable of wait configuration categories keys to check for a given name
	/// </summary>
	public static IEnumerable<string> GetKeyPrefixes(string name)
	{
		while (!string.IsNullOrEmpty(name))
		{
			yield return name;
			var lastIndexOfDot = name.LastIndexOf('.');
			if (lastIndexOfDot == -1)
			{
				yield return "Default";
				break;
			}
			name = name.Substring(0, lastIndexOfDot);
		}
	}

	/// <summary>
	/// Clear the configurations dictionary and then add the new configuration items to it
	/// <para>Clears the configuration cache dictionary as a final step</para>
	/// </summary>
	/// <param name="newConfigurationItems">The new configuration items to use</param>
	public void Reload(IEnumerable<KeyValuePair<string, WaitConfigurationItem>> newConfigurationItems)
	{
		_ = newConfigurationItems ?? throw new ArgumentNullException(nameof(newConfigurationItems));

		_waitConfigurations.Clear();
		foreach (var keyValuePair in newConfigurationItems)
		{
			_ = _waitConfigurations.TryAdd(keyValuePair.Key, keyValuePair.Value);
		}

		_waitConfigurationCache.Clear();
	}

}
