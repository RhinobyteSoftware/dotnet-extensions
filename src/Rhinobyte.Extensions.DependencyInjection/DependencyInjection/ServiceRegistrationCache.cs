using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// IServiceCollection implementation that wraps a default IServiceCollection implementation and provides dictionaries for fast lookup of existing service registration types
	/// </summary>{
	public class ServiceRegistrationCache : IServiceCollection
	{
		private readonly Dictionary<Type, List<ServiceDescriptor>> _lookupByImplementationType = new Dictionary<Type, List<ServiceDescriptor>>();
		private readonly Dictionary<Type, List<ServiceDescriptor>> _lookupByServiceType = new Dictionary<Type, List<ServiceDescriptor>>();
		private readonly IServiceCollection _serviceCollection;

		public ServiceRegistrationCache(IServiceCollection serviceCollection)
		{
			_serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			foreach (var serviceDescriptor in serviceCollection)
				AddLookupItems(serviceDescriptor);
		}


		public ServiceDescriptor this[int index] { get => _serviceCollection[index]; set => _serviceCollection[index] = value; }

		public int Count => _serviceCollection.Count;

		public bool IsReadOnly => _serviceCollection.IsReadOnly;



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
				byServiceTypeList = new List<ServiceDescriptor>();
				_lookupByServiceType.Add(serviceDescriptor.ServiceType, byServiceTypeList);
			}

			byServiceTypeList.Add(serviceDescriptor);


			if (!_lookupByImplementationType.TryGetValue(implementationType, out var byImplementationTypeList))
			{
				byImplementationTypeList = new List<ServiceDescriptor>();
				_lookupByImplementationType.Add(implementationType, byImplementationTypeList);
			}

			byImplementationTypeList.Add(serviceDescriptor);
		}

		public void Clear()
		{
			_serviceCollection.Clear();

			_lookupByImplementationType.Clear();
			_lookupByServiceType.Clear();
		}

		public bool Contains(ServiceDescriptor item) => _serviceCollection.Contains(item);

#pragma warning disable CA1725 // Parameter names should match base declaration
		public void CopyTo(ServiceDescriptor[] array, int arrayStartIndex) => _serviceCollection.CopyTo(array, arrayStartIndex);
#pragma warning restore CA1725 // Parameter names should match base declaration

		public IEnumerator<ServiceDescriptor> GetEnumerator() => _serviceCollection.GetEnumerator();

		public IReadOnlyCollection<ServiceDescriptor>? GetByImplementationType(Type implementationType)
		{
			if (_lookupByImplementationType.TryGetValue(implementationType, out var byImplemenationTypeList))
				return byImplemenationTypeList;

			return null;
		}

		public IReadOnlyCollection<ServiceDescriptor>? GetByServiceType(Type serviceType)
		{
			if (_lookupByServiceType.TryGetValue(serviceType, out var byServiceTypeList))
				return byServiceTypeList;

			return null;
		}

		public bool HasAnyByImplemenationType(Type implementationType)
			=> GetByImplementationType(implementationType)?.Count > 0;

		public bool HasAnyByServiceType(Type serviceType)
			=> GetByServiceType(serviceType)?.Count > 0;

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

		public bool HasExistingMatch(ServiceDescriptor serviceDescriptor)
		{
			if (serviceDescriptor is null)
				return false;

			var implementationType = serviceDescriptor.TryGetImplementationType();
			if (implementationType is null)
				return false;

			return HasExistingMatch(serviceDescriptor.ServiceType, implementationType);
		}

		public int IndexOf(ServiceDescriptor item) => _serviceCollection.IndexOf(item);

		public void Insert(int index, ServiceDescriptor item)
		{
			AddLookupItems(item);
			_serviceCollection.Insert(index, item);
		}

		public bool Remove(ServiceDescriptor item)
		{
			RemoveLookupItems(item);
			return _serviceCollection.Remove(item);
		}

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
			{
				byServiceTypeList.Remove(serviceDescriptor);
			}

			if (_lookupByImplementationType.TryGetValue(implementationType, out var byImplementationTypeList))
			{
				byImplementationTypeList.Remove(serviceDescriptor);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => _serviceCollection.GetEnumerator();
	}
}
