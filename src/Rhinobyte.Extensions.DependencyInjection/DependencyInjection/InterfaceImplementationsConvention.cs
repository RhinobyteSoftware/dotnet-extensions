using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public class InterfaceImplementationsConvention : ServiceRegistrationConventionBase
	{
		public InterfaceImplementationsConvention(
			ConstructorSelectionType defaultConstructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly,
			ServiceLifetime defaultLifetime = ServiceLifetime.Scoped,
			ServiceRegistrationOverwriteBehavior defaultOverwriteBehavior = ServiceRegistrationOverwriteBehavior.TryAdd,
			InterfaceImplementationResolutionStrategy resolutionStrategy = InterfaceImplementationResolutionStrategy.DefaultConventionOrSingleImplementationOnly,
			bool skipAlreadyRegistered = true,
			bool skipDuplicates = true,
			bool skipImplementationTypesAlreadyInUse = true)
			: base(defaultConstructorSelectionType, defaultLifetime, defaultOverwriteBehavior, skipDuplicates, skipImplementationTypesAlreadyInUse)
		{
			ResolutionStrategy = resolutionStrategy;
			SkipAlreadyRegistered = skipAlreadyRegistered;
		}

		public InterfaceImplementationResolutionStrategy ResolutionStrategy { get; protected set; }

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
			if (!discoveredType.IsInterface)
				return null;

			if (SkipAlreadyRegistered && serviceRegistrationCache.HasAnyByServiceType(discoveredType))
				return null;

			var implementationTypes = new List<Type>();
			var defaultConventionImplementationTypes = new List<Type>();
			var defaultConventionClassName = discoveredType.Name?.StartsWith("I") == true
				? discoveredType.Name.Substring(1)
				: null;

			foreach (var concreteType in scanResult.ConcreteTypes)
			{
				if (!discoveredType.IsAssignableFrom(concreteType))
					continue;

				implementationTypes.Add(concreteType);

				if (ResolutionStrategy != InterfaceImplementationResolutionStrategy.SingleImplementationOnly
					&& concreteType.Name == defaultConventionClassName)
				{
					defaultConventionImplementationTypes.Add(concreteType);
				}
			}

			if (implementationTypes.Count < 1)
				return null;

			switch (ResolutionStrategy)
			{
				case InterfaceImplementationResolutionStrategy.AllImplementations:
				{
					var serviceDescriptors = new List<ServiceDescriptor>();
					foreach (var implementationType in implementationTypes)
					{
						var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationType, serviceRegistrationCache);
						if (serviceDescriptor != null)
							serviceDescriptors.Add(serviceDescriptor);
					}

					if (serviceDescriptors.Count < 1)
						return null;

					return new ServiceRegistrationParameters(serviceDescriptors);
				}

				case InterfaceImplementationResolutionStrategy.DefaultConventionOnly:
				{
					if (defaultConventionImplementationTypes.Count != 1)
						return null;

					var serviceDescriptor = BuildServiceDescriptor(discoveredType, defaultConventionImplementationTypes[0], serviceRegistrationCache);
					return serviceDescriptor == null
						? null
						: new ServiceRegistrationParameters(serviceDescriptor);
				}

				case InterfaceImplementationResolutionStrategy.DefaultConventionOrAll:
				{
					if (defaultConventionImplementationTypes.Count == 1)
					{
						var serviceDescriptor = BuildServiceDescriptor(discoveredType, defaultConventionImplementationTypes[0], serviceRegistrationCache);
						return serviceDescriptor == null
							? null
							: new ServiceRegistrationParameters(serviceDescriptor);
					}

					var serviceDescriptors = new List<ServiceDescriptor>();
					foreach (var implementationType in implementationTypes)
					{
						var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationType, serviceRegistrationCache);
						if (serviceDescriptor != null)
							serviceDescriptors.Add(serviceDescriptor);
					}

					if (serviceDescriptors.Count < 1)
						return null;

					return new ServiceRegistrationParameters(serviceDescriptors);
				}

				case InterfaceImplementationResolutionStrategy.DefaultConventionOrSingleImplementationOnly:
				{
					if (implementationTypes.Count == 1)
					{
						var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationTypes[0], serviceRegistrationCache);
						return serviceDescriptor == null
							? null
							: new ServiceRegistrationParameters(serviceDescriptor);
					}

					if (defaultConventionImplementationTypes.Count == 1)
					{
						var serviceDescriptor = BuildServiceDescriptor(discoveredType, defaultConventionImplementationTypes[0], serviceRegistrationCache);
						return serviceDescriptor == null
							? null
							: new ServiceRegistrationParameters(serviceDescriptor);
					}

					return null;
				}

				case InterfaceImplementationResolutionStrategy.SingleImplementationOnly:
				{
					if (implementationTypes.Count != 1)
						return null;

					var serviceDescriptor = BuildServiceDescriptor(discoveredType, implementationTypes[0], serviceRegistrationCache);
					return serviceDescriptor == null
						? null
						: new ServiceRegistrationParameters(serviceDescriptor);
				}

				default:
					throw new NotImplementedException($"{nameof(InterfaceImplementationsConvention)}.{nameof(GetServiceRegistrationParameters)}(..) is not implemented for the {nameof(InterfaceImplementationResolutionStrategy)} value of {ResolutionStrategy}");
			}
		}
#pragma warning restore CA1062 // Validate arguments of public methods
	}
}
