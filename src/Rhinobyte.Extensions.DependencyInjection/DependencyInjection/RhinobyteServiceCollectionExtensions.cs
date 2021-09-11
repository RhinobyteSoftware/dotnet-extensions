using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public static class RhinobyteServiceCollectionExtensions
	{
		public static IServiceCollection AddTransientWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.AttributeThenMostParameters)
			where TImplementationType : class
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			// TODO: Logic to find the constructor to use
			//serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));

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

		public static IServiceCollection AddScopedWithConstructorSelection<TServiceType, TImplementationType>(
			this IServiceCollection serviceCollection,
			ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.AttributeThenMostParameters)
			where TImplementationType : class
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			// TODO: Logic to find the constructor to use
			//serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));

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
			ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.AttributeThenMostParameters)
			where TImplementationType : class
		{
			_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

			// TODO: Logic to find the constructor to use
			//serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));

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
	}
}
