using System.Collections.Generic;
using System.Linq;

namespace Rhinobyte.Extensions.Logging.Common;

internal static class EnumerableExtensions
{
	/// <summary>
	/// Convenience extension method for when we want to use an ICollection return type to ensure the enumerable is materialized. Use this when
	/// you know that if the enumerable is already materialized it's ok to use the current instance without copying it.
	/// </summary>
	public static ICollection<TItem> AsCollectionOrToArray<TItem>(this IEnumerable<TItem> items)
	{
		if (items is ICollection<TItem> collection)
		{
			return collection;
		}

		return items.ToArray();
	}

	/// <summary>
	/// Convenience extension method for when we want to use an ICollection return type to ensure the enumerable is materialized. Use this when
	/// you know that if the enumerable is already materialized it's ok to use the current instance without copying it.
	/// </summary>
	public static ICollection<TItem> AsCollectionOrToList<TItem>(this IEnumerable<TItem> items)
	{
		if (items is ICollection<TItem> collection)
		{
			return collection;
		}

		return items.ToList();
	}


	/// <summary>
	/// Convenience extension method for when we want to use an IReadOnlyCollection return type to ensure the enumerable is materialized. Use this when
	/// you know that if the enumerable is already materialized it's ok to use the current instance without copying it.
	/// </summary>
	public static IReadOnlyCollection<TItem> AsReadOnlyCollectionOrToArray<TItem>(this IEnumerable<TItem> items)
	{
		if (items is IReadOnlyCollection<TItem> collection)
		{
			return collection;
		}

		return items.ToArray();
	}

	/// <summary>
	/// Convenience extension method for when we want to use an IReadOnlyCollection return type to ensure the enumerable is materialized. Use this when
	/// you know that if the enumerable is already materialized it's ok to use the current instance without copying it.
	/// </summary>
	public static IReadOnlyCollection<TItem> AsReadOnlyCollectionOrToList<TItem>(this IEnumerable<TItem> items)
	{
		if (items is IReadOnlyCollection<TItem> collection)
		{
			return collection;
		}

		return items.ToList();
	}
}
