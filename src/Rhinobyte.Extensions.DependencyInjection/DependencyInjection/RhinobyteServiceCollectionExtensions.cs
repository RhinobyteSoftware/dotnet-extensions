using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public static class RhinobyteServiceCollectionExtensions
	{
		public static IServiceCollection AddScopedWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.AttributeThenDefaultBehavior)
			where TServiceType : class
			where TImplementationType : class, TServiceType
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
			if (explicitConstructorToUse == null)
				return serviceCollection.AddScoped<TServiceType, TImplementationType>();

			serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));
			return serviceCollection;
		}

		public static IServiceCollection AddScopedWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorInfo explicitConstructorToUse)
			where TImplementationType : class
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));

			return serviceCollection;
		}

		public static IServiceCollection AddSingletonWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.AttributeThenDefaultBehavior)
			where TServiceType : class
			where TImplementationType : class, TServiceType
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
			if (explicitConstructorToUse == null)
				return serviceCollection.AddSingleton<TServiceType, TImplementationType>();

			serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));
			return serviceCollection;
		}

		public static IServiceCollection AddSingletonWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorInfo explicitConstructorToUse)
			where TImplementationType : class
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));

			return serviceCollection;
		}

		public static IServiceCollection AddTransientWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.AttributeThenDefaultBehavior)
			where TServiceType : class
			where TImplementationType : class, TServiceType
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
			if (explicitConstructorToUse == null)
				return serviceCollection.AddTransient<TServiceType, TImplementationType>();

			serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));
			return serviceCollection;
		}

		public static IServiceCollection AddTransientWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorInfo explicitConstructorToUse)
			where TImplementationType : class
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));

			return serviceCollection;
		}

		public static IServiceCollection RegisterTypes(
			this IServiceCollection serviceCollection,
			IAssemblyScanner scanner,
			IServiceRegistrationConvention serviceRegistrationConvention)
		{
			_ = scanner ?? throw new ArgumentNullException(nameof(scanner));
			return RegisterTypes(serviceCollection, scanner.ScanAssemblies(), serviceRegistrationConvention);
		}

		public static IServiceCollection RegisterTypes(
			this IServiceCollection serviceCollection,
			IAssemblyScanResult scanResult,
			IServiceRegistrationConvention serviceRegistrationConvention)
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
			_ = scanResult ?? throw new ArgumentNullException(nameof(scanResult));
			_ = serviceRegistrationConvention ?? throw new ArgumentNullException(nameof(serviceRegistrationConvention));

			foreach (var discoveredType in scanResult.AllDiscoveredTypes)
				serviceRegistrationConvention.HandleType(discoveredType, scanResult, serviceCollection);

			return serviceCollection;
		}
	}
}
