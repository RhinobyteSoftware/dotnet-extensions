using System;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection.DependencyInjection
{
	public class AttributeDecoratedConvention : ServiceRegistrationConventionBase
	{
#pragma warning disable CA1062 // Validate arguments of public methods
		public override ServiceRegistrationParameters? GetServiceRegistrationParameters(
			Type discoveredType,
			IAssemblyScanResult scanResult,
			ServiceRegistrationCache serviceRegistrationCache)
		{
			if (serviceRegistrationCache.HasAnyByServiceType(discoveredType))
				return null; // Already registered

			var registrationAttribute = discoveredType.GetCustomAttribute<RegisterForDependencyInjectionAttribute>(false);
			if (registrationAttribute == null)
				return null;

			var implementationType = registrationAttribute.ImplementationType;
			if (implementationType == null)
				throw new InvalidOperationException($"{discoveredType.FullName} is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} that has a null value for {nameof(RegisterForDependencyInjectionAttribute.ImplementationType)}");

			if (!discoveredType.IsAssignableFrom(implementationType) || !implementationType.IsClass || implementationType.IsAbstract)
				throw new InvalidOperationException($"{discoveredType.FullName} is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} with an invalid implementationType of {implementationType.FullName}");

			var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationType, serviceRegistrationCache, registrationAttribute.ConstructorSelectionType, registrationAttribute.ServiceLifetime);
			if (serviceDescriptor == null)
				return null;

			return new ServiceRegistrationParameters(serviceDescriptor, registrationAttribute.ServiceRegistrationOverwriteBehavior);
		}
#pragma warning restore CA1062 // Validate arguments of public methods
	}
}
