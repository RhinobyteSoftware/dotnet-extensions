using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Service registration convention that will handle registration of discovered types that are decorated with the <see cref="RegisterForDependencyInjectionAttribute"/>
	/// </summary>
	public class AttributeDecoratedConvention : ServiceRegistrationConventionBase
	{
		/// <summary>
		/// Construct an instance of the registration convention with the specified configuration values.
		/// </summary>
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
		/// <summary>
		/// Implementation of the <see cref="ServiceRegistrationConventionBase.GetServiceRegistrationParameters(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> base method.
		/// <para>This implementation checks the discovered type for an <see cref="RegisterForDependencyInjectionAttribute"/> decorator.</para>
		/// <para>If the attribute is present the <see cref="RegisterForDependencyInjectionAttribute.ImplementationType"/> value is used to construct the registration parameters.</para>
		/// </summary>
		public override ServiceRegistrationParameters? GetServiceRegistrationParameters(
			Type discoveredType,
			IAssemblyScanResult scanResult,
			ServiceRegistrationCache serviceRegistrationCache)
		{
			if (SkipAlreadyRegistered && serviceRegistrationCache.HasAnyByServiceType(discoveredType))
				return null;

			var registrationAttribute = discoveredType.GetCustomAttribute<RegisterForDependencyInjectionAttribute>(false);
			if (registrationAttribute is null)
				return null;

			var implementationType = registrationAttribute.ImplementationType;
			if (implementationType is null)
				throw new InvalidOperationException($"{discoveredType.FullName} is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} that has a null value for {nameof(RegisterForDependencyInjectionAttribute.ImplementationType)}");

			if (!discoveredType.IsAssignableFrom(implementationType) || !implementationType.IsClass || implementationType.IsAbstract)
				throw new InvalidOperationException($"{discoveredType.FullName} is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} with an invalid implementationType of {implementationType.FullName}");

			var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationType, serviceRegistrationCache, constructorSelectionType: registrationAttribute.ConstructorSelectionType, lifetime: registrationAttribute.ServiceLifetime);
			if (serviceDescriptor is null)
				return null;

			return new ServiceRegistrationParameters(serviceDescriptor, registrationAttribute.ServiceRegistrationOverwriteBehavior);
		}
#pragma warning restore CA1062 // Validate arguments of public methods
	}
}
