using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public class AttributeDecoratedConvention : ServiceRegistrationConventionBase
	{
		public AttributeDecoratedConvention(
			ConstructorSelectionType defaultConstructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly,
			ServiceLifetime defaultLifetime = ServiceLifetime.Scoped,
			ServiceRegistrationOverwriteBehavior defaultOverwriteBehavior = ServiceRegistrationOverwriteBehavior.TryAdd,
			bool skipAlreadyRegistered = true,
			bool skipDuplicates = true,
			bool skipImplementationTypesAlreadyInUse = true)
			: base(defaultConstructorSelectionType, defaultLifetime, defaultOverwriteBehavior, skipDuplicates, skipImplementationTypesAlreadyInUse)
		{
			SkipAlreadyRegistered = skipAlreadyRegistered;
		}

		/// <summary>
		/// When true the <see cref="GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> method will short circuit and return null
		/// if the service collection contains any items where the <see cref="ServiceDescriptor.ServiceType"/> matches the discovered type
		/// <para>
		/// This happens before any other checks regardless of other values such as the <see cref="ServiceRegistrationOverwriteBehavior"/>
		/// </para>
		/// </summary>
		public bool SkipAlreadyRegistered { get; protected set; }


#pragma warning disable CA1062 // Validate arguments of public methods
		public override ServiceRegistrationParameters? GetServiceRegistrationParameters(
			Type discoveredType,
			IAssemblyScanResult scanResult,
			ServiceRegistrationCache serviceRegistrationCache)
		{
			if (SkipAlreadyRegistered && serviceRegistrationCache.HasAnyByServiceType(discoveredType))
				return null;

			var registrationAttribute = discoveredType.GetCustomAttribute<RegisterForDependencyInjectionAttribute>(false);
			if (registrationAttribute == null)
				return null;

			var implementationType = registrationAttribute.ImplementationType;
			if (implementationType == null)
				throw new InvalidOperationException($"{discoveredType.FullName} is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} that has a null value for {nameof(RegisterForDependencyInjectionAttribute.ImplementationType)}");

			if (!discoveredType.IsAssignableFrom(implementationType) || !implementationType.IsClass || implementationType.IsAbstract)
				throw new InvalidOperationException($"{discoveredType.FullName} is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} with an invalid implementationType of {implementationType.FullName}");

			var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationType, serviceRegistrationCache, constructorSelectionType: registrationAttribute.ConstructorSelectionType, lifetime: registrationAttribute.ServiceLifetime);
			if (serviceDescriptor == null)
				return null;

			return new ServiceRegistrationParameters(serviceDescriptor, registrationAttribute.ServiceRegistrationOverwriteBehavior);
		}
#pragma warning restore CA1062 // Validate arguments of public methods
	}
}
