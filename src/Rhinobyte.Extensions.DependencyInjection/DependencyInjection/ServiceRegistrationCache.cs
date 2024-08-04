using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// IServiceCollection implementation that wraps a default IServiceCollection implementation and provides dictionaries for fast lookup of existing service registration types
/// </summary>
public class ServiceRegistrationCache : IServiceCollection
{
	private readonly Dictionary<Type, List<ServiceDescriptor>> _lookupByImplementationType = [];
	private readonly Dictionary<Type, List<ServiceDescriptor>> _lookupByServiceType = [];
	private readonly IServiceCollection _serviceCollection;

	/// <summary>
	/// Construct a <see cref="ServiceRegistrationCache"/> that wraps the provided <paramref name="serviceCollection"/>
	/// </summary>
	public ServiceRegistrationCache(IServiceCollection serviceCollection)
	{
		_serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		foreach (var serviceDescriptor in serviceCollection)
			AddLookupItems(serviceDescriptor);
	}


	/// <inheritdoc/>
	public ServiceDescriptor this[int index] { get => _serviceCollection[index]; set => _serviceCollection[index] = value; }

	/// <inheritdoc/>
	public int Count => _serviceCollection.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => _serviceCollection.IsReadOnly;


	/// <inheritdoc/>
	public void Add(ServiceDescriptor item)
	{
		AddLookupItems(item);
		_serviceCollection.Add(item);
	}

	private void AddLookupItems(ServiceDescriptor serviceDescriptor)
	{
		if (serviceDescriptor is null)
			return;

		var implementationType = serviceDescriptor.TryGetImplementationType();
		if (implementationType is null)
			return;

		if (!_lookupByServiceType.TryGetValue(serviceDescriptor.ServiceType, out var byServiceTypeList))
		{
			byServiceTypeList = [];
			_lookupByServiceType.Add(serviceDescriptor.ServiceType, byServiceTypeList);
		}

		byServiceTypeList.Add(serviceDescriptor);


		if (!_lookupByImplementationType.TryGetValue(implementationType, out var byImplementationTypeList))
		{
			byImplementationTypeList = [];
			_lookupByImplementationType.Add(implementationType, byImplementationTypeList);
		}

		byImplementationTypeList.Add(serviceDescriptor);
	}

	/// <inheritdoc/>
	public void Clear()
	{
		_serviceCollection.Clear();

		_lookupByImplementationType.Clear();
		_lookupByServiceType.Clear();
	}

	/// <inheritdoc/>
	public bool Contains(ServiceDescriptor item) => _serviceCollection.Contains(item);

#pragma warning disable CA1725 // Parameter names should match base declaration
	/// <inheritdoc/>
	public void CopyTo(ServiceDescriptor[] array, int arrayStartIndex) => _serviceCollection.CopyTo(array, arrayStartIndex);
#pragma warning restore CA1725 // Parameter names should match base declaration

	/// <inheritdoc/>
	public IEnumerator<ServiceDescriptor> GetEnumerator() => _serviceCollection.GetEnumerator();

	/// <summary>
	/// Convenience method to return a collection of any <see cref="ServiceDescriptor"/> instances in the collection with the specified <paramref name="implementationType"/>
	/// </summary>
	/// <remarks>Uses an internally maintained <see cref="IDictionary{TKey, TValue}"/> to avoid the need to enumerate all of the contained descriptors.</remarks>
	public IReadOnlyCollection<ServiceDescriptor>? GetByImplementationType(Type implementationType)
	{
		if (_lookupByImplementationType.TryGetValue(implementationType, out var byImplemenationTypeList))
			return byImplemenationTypeList;

		return null;
	}

	/// <summary>
	/// Convenience method to return a collection of any <see cref="ServiceDescriptor"/> instances in the collection with the specified <paramref name="serviceType"/>
	/// </summary>
	/// <remarks>Uses an internally maintained <see cref="IDictionary{TKey, TValue}"/> to avoid the need to enumerate all of the contained descriptors.</remarks>
	public IReadOnlyCollection<ServiceDescriptor>? GetByServiceType(Type serviceType)
	{
		if (_lookupByServiceType.TryGetValue(serviceType, out var byServiceTypeList))
			return byServiceTypeList;

		return null;
	}

	/// <summary>
	/// Return true if the <see cref="IServiceCollection"/> contains one or more <see cref="ServiceDescriptor"/> with the specified <paramref name="implementationType"/>
	/// </summary>
	/// <remarks>Uses an internally maintained <see cref="IDictionary{TKey, TValue}"/> to avoid the need to enumerate all of the contained descriptors.</remarks>
	public bool HasAnyByImplemenationType(Type implementationType)
		=> GetByImplementationType(implementationType)?.Count > 0;

	/// <summary>
	/// Return true if the <see cref="IServiceCollection"/> contains one or more <see cref="ServiceDescriptor"/> with the specified <paramref name="serviceType"/>
	/// </summary>
	/// <remarks>Uses an internally maintained <see cref="IDictionary{TKey, TValue}"/> to avoid the need to enumerate all of the contained descriptors.</remarks>
	public bool HasAnyByServiceType(Type serviceType)
		=> GetByServiceType(serviceType)?.Count > 0;

	/// <summary>
	/// Return true if the <see cref="IServiceCollection"/> contains one or more <see cref="ServiceDescriptor"/> the match both the <paramref name="serviceType"/>
	/// and the <paramref name="implementationType"/>.
	/// </summary>
	/// <remarks>Uses an internally maintained <see cref="IDictionary{TKey, TValue}"/> to avoid the need to enumerate all of the contained descriptors.</remarks>
	public bool HasExistingMatch(Type serviceType, Type implementationType)
	{
		if (!_lookupByServiceType.TryGetValue(serviceType, out var byServiceType))
			return false;

		foreach (var existingDescriptor in byServiceType)
		{
			if (existingDescriptor.TryGetImplementationType() == implementationType)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Return true if the <see cref="IServiceCollection"/> contains one or more <see cref="ServiceDescriptor"/> the match both the <see cref="ServiceDescriptor.ServiceType"/>
	/// and <see cref="ServiceDescriptor.ImplementationType"/>.
	/// </summary>
	/// <remarks>Uses an internally maintained <see cref="IDictionary{TKey, TValue}"/> to avoid the need to enumerate all of the contained descriptors.</remarks>
	public bool HasExistingMatch(ServiceDescriptor serviceDescriptor)
	{
		if (serviceDescriptor is null)
			return false;

		var implementationType = serviceDescriptor.TryGetImplementationType();
		if (implementationType is null)
			return false;

		return HasExistingMatch(serviceDescriptor.ServiceType, implementationType);
	}

	/// <inheritdoc/>
	public int IndexOf(ServiceDescriptor item) => _serviceCollection.IndexOf(item);

	/// <inheritdoc/>
	public void Insert(int index, ServiceDescriptor item)
	{
		AddLookupItems(item);
		_serviceCollection.Insert(index, item);
	}

	/// <inheritdoc/>
	public bool Remove(ServiceDescriptor item)
	{
		RemoveLookupItems(item);
		return _serviceCollection.Remove(item);
	}

	/// <inheritdoc/>
	public void RemoveAt(int index)
	{
		var serviceDescriptorToRemove = _serviceCollection[index];
		RemoveLookupItems(serviceDescriptorToRemove);

		_serviceCollection.RemoveAt(index);
	}

	private void RemoveLookupItems(ServiceDescriptor serviceDescriptor)
	{
		if (serviceDescriptor is null)
			return;

		var implementationType = serviceDescriptor.TryGetImplementationType();
		if (implementationType is null)
			return;

		if (_lookupByServiceType.TryGetValue(serviceDescriptor.ServiceType, out var byServiceTypeList))
			_ = byServiceTypeList.Remove(serviceDescriptor);

		if (_lookupByImplementationType.TryGetValue(implementationType, out var byImplementationTypeList))
			_ = byImplementationTypeList.Remove(serviceDescriptor);
	}

	IEnumerator IEnumerable.GetEnumerator() => _serviceCollection.GetEnumerator();
}
