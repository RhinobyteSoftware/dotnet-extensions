using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Base implementation of the <see cref="IServiceRegistrationConvention"/> contract.
/// <para>
/// Subclasses should implement the abstract <see cref="GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> method to handle
/// selection of the implementation type for a discovered type
/// </para>
/// </summary>
public abstract class ServiceRegistrationConventionBase : IServiceRegistrationConvention
{
	/// <summary>
	/// Construct an instance of the registration convention with the specified configuration values.
	/// </summary>
	protected ServiceRegistrationConventionBase(
		ConstructorSelectionType defaultConstructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly,
		ServiceLifetime defaultLifetime = ServiceLifetime.Scoped,
		ServiceRegistrationOverwriteBehavior defaultOverwriteBehavior = ServiceRegistrationOverwriteBehavior.TryAdd,
		bool skipDuplicates = true,
		bool skipImplementationTypesAlreadyInUse = true)
	{
		DefaultConstructorSelectionType = defaultConstructorSelectionType;
		DefaultLifetime = defaultLifetime;
		DefaultOverwriteBehavior = defaultOverwriteBehavior;
		SkipDuplicates = skipDuplicates;
		SkipImplementationTypesAlreadyInUse = skipImplementationTypesAlreadyInUse;
	}

	/// <summary>
	/// The default <see cref="ConstructorSelectionType"/> to use when calling the
	/// <see cref="BuildServiceDescriptor(Type, Type, ServiceRegistrationCache, ConstructorSelectionType?, ServiceLifetime?, bool?)"/> method.
	/// </summary>
	public ConstructorSelectionType DefaultConstructorSelectionType { get; protected set; } = ConstructorSelectionType.DefaultBehaviorOnly;

	/// <summary>
	/// The default <see cref="ServiceLifetime"/> to use when calling the
	/// <see cref="BuildServiceDescriptor(Type, Type, ServiceRegistrationCache, ConstructorSelectionType?, ServiceLifetime?, bool?)"/> method.
	/// </summary>
	public ServiceLifetime DefaultLifetime { get; protected set; } = ServiceLifetime.Scoped;

	/// <summary>
	/// The default <see cref="ServiceRegistrationOverwriteBehavior"/> for the <see cref="HandleType(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> implementation to use.
	/// <para>
	/// Subclasses can supercede the default for an individual registration by returning a non-null <see cref="ServiceRegistrationParameters.OverwriteBehavior"/>
	/// value in their implementation of the <see cref="GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> method.
	/// </para>
	/// </summary>
	public ServiceRegistrationOverwriteBehavior DefaultOverwriteBehavior { get; protected set; } = ServiceRegistrationOverwriteBehavior.TryAdd;

	/// <summary>
	/// When true, the <see cref="ServiceDescriptor"/>s will not be added if the service collection already has one or more desriptors with the same value for
	/// both <see cref="ServiceDescriptor.ServiceType"/> and <see cref="ServiceDescriptorExtensions.TryGetImplementationType(ServiceDescriptor)"/>
	/// <para>
	/// Subclasses can supersede this value for an individual item by using a non-null <see cref="ServiceRegistrationParameters.SkipDuplicates"/>
	/// property
	/// </para>
	/// </summary>
	public bool SkipDuplicates { get; protected set; }

	/// <summary>
	/// When true, the <see cref="ServiceDescriptor"/>s will not be added if the service collection already has one or more desriptors with the same value for
	/// <see cref="ServiceDescriptorExtensions.TryGetImplementationType(ServiceDescriptor)"/>
	/// <para>
	/// Subclasses can supersede this value for an individual item by using a non-null <see cref="ServiceRegistrationParameters.SkipImplementationTypesAlreadyInUse"/>
	/// property and a non null parameter for the <see cref="BuildServiceDescriptor(Type, Type, ServiceRegistrationCache, ConstructorSelectionType?, ServiceLifetime?, bool?)"/>
	/// method
	/// </para>
	/// </summary>
	public bool SkipImplementationTypesAlreadyInUse { get; protected set; } = true;


#pragma warning disable CA1062 // Validate arguments of public methods
	/// <summary>
	/// Helper that subclasses can use to build a service descriptor in their implementation of the
	/// <see cref="GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> method.
	/// <para>
	/// Provides logic to skip implementation types already in use and to handle explicit constructor selection
	/// based on the property/parameter values provided.
	/// </para>
	/// </summary>
	protected ServiceDescriptor? BuildServiceDescriptor(
		Type discoveredServiceType,
		Type implementationType,
		ServiceRegistrationCache serviceRegistrationCache,
		ConstructorSelectionType? constructorSelectionType = null,
		ServiceLifetime? lifetime = null,
		bool? skipImplementationTypesAlreadyInUse = null)
	{
		var skipByImplementationType = skipImplementationTypesAlreadyInUse ?? SkipImplementationTypesAlreadyInUse;
		if (skipByImplementationType && serviceRegistrationCache.HasAnyByImplemenationType(implementationType))
			return null;

		var constructorSelectionTypeToUse = constructorSelectionType ?? DefaultConstructorSelectionType;
		var serviceLifetimeToUse = lifetime ?? DefaultLifetime;
		if (constructorSelectionTypeToUse != ConstructorSelectionType.DefaultBehaviorOnly)
		{
			var explicitConstructor = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(implementationType, constructorSelectionTypeToUse);
			if (explicitConstructor != null)
			{
				return ExplicitConstructorServiceDescriptor.Create(discoveredServiceType, implementationType, explicitConstructor, serviceLifetimeToUse);
			}
		}

		return new ServiceDescriptor(discoveredServiceType, implementationType, serviceLifetimeToUse);
	}
#pragma warning restore CA1062 // Validate arguments of public methods

	/// <summary>
	/// Abstract method that subclasses need to implement to select one or more implementation types to use for the <paramref name="discoveredType"/>
	/// </summary>
	public abstract ServiceRegistrationParameters? GetServiceRegistrationParameters(
		Type discoveredType,
		IAssemblyScanResult scanResult,
		ServiceRegistrationCache serviceRegistrationCache
	);

	/// <summary>
	/// Implementation of the <see cref="IServiceRegistrationConvention.HandleType(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> method.
	/// <para>This implementation provides logic for deciding on when/how to register a service decriptor based on the specified property values.</para>
	/// <para>
	/// Subclasses should override <see cref="GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> to provide the logic for selecting
	/// a specific implementation type to use for the <paramref name="discoveredType"/>
	/// </para>
	/// </summary>
	public virtual bool HandleType(
		Type discoveredType,
		IAssemblyScanResult scanResult,
		ServiceRegistrationCache serviceRegistrationCache)
	{
		_ = discoveredType ?? throw new ArgumentNullException(nameof(discoveredType));
		_ = scanResult ?? throw new ArgumentNullException(nameof(scanResult));
		_ = serviceRegistrationCache ?? throw new ArgumentNullException(nameof(serviceRegistrationCache));

		var serviceRegistrationParameters = GetServiceRegistrationParameters(discoveredType, scanResult, serviceRegistrationCache);
		if (serviceRegistrationParameters is null)
			return false;

		var overwriteBehaviorToUse = serviceRegistrationParameters.OverwriteBehavior ?? DefaultOverwriteBehavior;
		var skipDuplicates = serviceRegistrationParameters.SkipDuplicates ?? SkipDuplicates;
		var skipImplementationTypesAlreadyInUse = serviceRegistrationParameters.SkipImplementationTypesAlreadyInUse ?? SkipImplementationTypesAlreadyInUse;

		if (serviceRegistrationParameters.ServiceDescriptor != null)
			return TryRegister(overwriteBehaviorToUse, serviceRegistrationParameters.ServiceDescriptor, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);

		if (serviceRegistrationParameters.ServiceDescriptors is null)
			return false;

		return TryRegisterMultiple(
			discoveredType,
			overwriteBehaviorToUse,
			serviceRegistrationParameters.ServiceDescriptors,
			serviceRegistrationCache,
			skipDuplicates,
			skipImplementationTypesAlreadyInUse
		);
	}

	/// <summary>
	/// Attempt to register the <paramref name="serviceDescriptor"/> against the <see cref="ServiceRegistrationCache"/>
	/// </summary>
	public static bool TryRegister(
		ServiceRegistrationOverwriteBehavior overwriteBehavior,
		ServiceDescriptor serviceDescriptor,
		ServiceRegistrationCache serviceRegistrationCache,
		bool skipDuplicates,
		bool skipImplementationTypesAlreadyInUse)
	{
		_ = serviceDescriptor ?? throw new ArgumentNullException(nameof(serviceDescriptor));
		_ = serviceRegistrationCache ?? throw new ArgumentNullException(nameof(serviceRegistrationCache));

		var implementationType = serviceDescriptor.TryGetImplementationType()
			?? throw new ArgumentException($"{nameof(ServiceDescriptorExtensions.TryGetImplementationType)} returned null");

		var isTryAdd = overwriteBehavior == ServiceRegistrationOverwriteBehavior.TryAdd;
		var byServiceType = isTryAdd || skipDuplicates
			? serviceRegistrationCache.GetByServiceType(serviceDescriptor.ServiceType)
			: null;

		if (isTryAdd && byServiceType?.Count > 0)
			return true; // Treat it has handled, no need to call the TryAdd extension method which would iterate over all the service descriptors

		if (skipImplementationTypesAlreadyInUse && serviceRegistrationCache.HasAnyByImplemenationType(implementationType))
			return false;

		if (skipDuplicates && byServiceType != null)
		{
			foreach (var existingDescriptor in byServiceType)
			{
				if (existingDescriptor.TryGetImplementationType() == implementationType)
					return false;
			}
		}

		switch (overwriteBehavior)
		{
			case ServiceRegistrationOverwriteBehavior.Add:
			case ServiceRegistrationOverwriteBehavior.TryAdd:
				// We already check for duplicates using our fast lookup above, so calling Add for the TryAdd case here should be fine
				serviceRegistrationCache.Add(serviceDescriptor);
				break;

			case ServiceRegistrationOverwriteBehavior.ReplaceAll:
				serviceRegistrationCache
					.RemoveAll(serviceDescriptor.ServiceType)
					.Add(serviceDescriptor);
				break;

			case ServiceRegistrationOverwriteBehavior.ReplaceFirst:
				_ = serviceRegistrationCache.Replace(serviceDescriptor);
				break;

			default:
				throw new NotImplementedException($"{nameof(TryRegister)} is not implemented for the {nameof(ServiceRegistrationOverwriteBehavior)} value of {overwriteBehavior}");
		}

		return true;
	}

	/// <summary>
	/// Attempt to register the collection of <paramref name="serviceDescriptors"/> against the <see cref="ServiceRegistrationCache"/>
	/// </summary>
	public static bool TryRegisterMultiple(
		Type discoveredType,
		ServiceRegistrationOverwriteBehavior overwriteBehavior,
		IEnumerable<ServiceDescriptor> serviceDescriptors,
		ServiceRegistrationCache serviceRegistrationCache,
		bool skipDuplicates,
		bool skipImplementationTypesAlreadyInUse)
	{
		_ = discoveredType ?? throw new ArgumentNullException(nameof(discoveredType));
		_ = serviceDescriptors ?? throw new ArgumentNullException(nameof(serviceDescriptors));
		_ = serviceRegistrationCache ?? throw new ArgumentNullException(nameof(serviceRegistrationCache));

		var serviceDescriptorsToAdd = new List<ServiceDescriptor>();
		var skipTryAddCount = 0;
		var totalCount = 0;

		var isTryAdd = overwriteBehavior == ServiceRegistrationOverwriteBehavior.TryAdd;
		var isReplaceAll = overwriteBehavior == ServiceRegistrationOverwriteBehavior.ReplaceAll;

		foreach (var serviceDescriptor in serviceDescriptors)
		{
			var implementationType = serviceDescriptor.TryGetImplementationType();
			if (implementationType is null)
				continue;

			++totalCount;

			var byServiceType = isTryAdd || skipDuplicates
				? serviceRegistrationCache.GetByServiceType(serviceDescriptor.ServiceType)
				: null;

			if (isTryAdd && byServiceType?.Count > 0)
			{
				++skipTryAddCount;
				continue; // Treat it has handled, no need to call the TryAdd extension method which would iterate over all the service descriptors
			}

			if (skipImplementationTypesAlreadyInUse && serviceRegistrationCache.HasAnyByImplemenationType(implementationType))
				continue;

			if (skipDuplicates && byServiceType != null)
			{
				var isDuplicate = false;
				foreach (var existingDescriptor in byServiceType)
				{
					if (existingDescriptor.TryGetImplementationType() == implementationType)
					{
						isDuplicate = true;
						break;
					}
				}

				if (isDuplicate)
					continue;
			}

			serviceDescriptorsToAdd.Add(serviceDescriptor);

			if (isReplaceAll && byServiceType?.Count > 0)
				_ = serviceRegistrationCache.RemoveAll(serviceDescriptor.ServiceType);
		}

		if (totalCount == skipTryAddCount)
			return true;

		if (serviceDescriptorsToAdd.Count < 1)
			return false;

		switch (overwriteBehavior)
		{
			case ServiceRegistrationOverwriteBehavior.Add:
			case ServiceRegistrationOverwriteBehavior.TryAdd:
			case ServiceRegistrationOverwriteBehavior.ReplaceAll:
				// We already check for duplicates and handle RemoveAll calls above using our fast lookup
				// so calling Add for the TryAdd and ReplaceAll cases here should be fine
				_ = serviceRegistrationCache.Add(serviceDescriptorsToAdd);
				break;

			case ServiceRegistrationOverwriteBehavior.ReplaceFirst:
				if (serviceDescriptorsToAdd.Count > 1)
					throw new InvalidOperationException($"{nameof(ServiceRegistrationOverwriteBehavior)}.{nameof(ServiceRegistrationOverwriteBehavior.ReplaceFirst)} is only valid when registering a single {nameof(ServiceDescriptor)}. {serviceDescriptorsToAdd.Count} descriptors were returned for {discoveredType.FullName}");

				_ = serviceRegistrationCache.Replace(serviceDescriptorsToAdd[0]);
				break;

			default:
				throw new NotImplementedException($"{nameof(TryRegisterMultiple)} is not implemented for the {nameof(ServiceRegistrationOverwriteBehavior)} value of {overwriteBehavior}");
		}

		return true;
	}

}
