using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Service registration convention that will register discovered concrete types as their own implementation type
	/// </summary>
	public class ConcreteTypeAsSelfConvention : ServiceRegistrationConventionBase
	{
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
}
