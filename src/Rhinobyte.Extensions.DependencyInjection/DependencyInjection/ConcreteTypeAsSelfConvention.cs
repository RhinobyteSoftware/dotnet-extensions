using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Service registration convention that will register discovered concrete types as their own implementation type
/// </summary>
public class ConcreteTypeAsSelfConvention : ServiceRegistrationConventionBase
{
	/// <summary>
	/// Construct an instance of the registration convention with the specified configuration values.
	/// </summary>
	public ConcreteTypeAsSelfConvention(
		ConstructorSelectionType defaultConstructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly,
		ServiceLifetime defaultLifetime = ServiceLifetime.Scoped,
		ServiceRegistrationOverwriteBehavior defaultOverwriteBehavior = ServiceRegistrationOverwriteBehavior.TryAdd,
		bool skipDuplicates = true,
		bool skipImplementationTypesAlreadyInUse = true)
		: base(
			defaultConstructorSelectionType,
			defaultLifetime,
			defaultOverwriteBehavior,
			skipDuplicates,
			skipImplementationTypesAlreadyInUse
		)
	{
	}

#pragma warning disable CA1062 // Validate arguments of public methods
	/// <summary>
	/// Implementation of the <see cref="ServiceRegistrationConventionBase.GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> base method.
	/// <para>This implementation checks the discovered type to ensure it is a concrete (non-abstract) class type.</para>
	/// <para>
	/// If so the convention attempts to register a <see cref="ServiceDescriptor"/> that uses <paramref name="discoveredType"/> for both the serviceType and implementationType of the descriptor.
	/// </para>
	/// </summary>
	public override ServiceRegistrationParameters? GetServiceRegistrationParameters(
		Type discoveredType,
		IAssemblyScanResult scanResult,
		ServiceRegistrationCache serviceRegistrationCache)
	{
		if (!discoveredType.IsClass || discoveredType.IsAbstract)
			return null;

		var serviceDescriptor = BuildServiceDescriptor(discoveredType, discoveredType, serviceRegistrationCache);
		return serviceDescriptor is null
			? null
			: new ServiceRegistrationParameters(serviceDescriptor);
	}
#pragma warning restore CA1062 // Validate arguments of public methods
}
