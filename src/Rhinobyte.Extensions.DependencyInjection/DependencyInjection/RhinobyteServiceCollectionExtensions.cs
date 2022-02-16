using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// See extension methods for <see cref="IServiceCollection"/> providing support for convention based registration of types discovered via reflection
/// </summary>
public static class RhinobyteServiceCollectionExtensions
{
	/// <summary>
	/// Add a <see cref="ServiceLifetime.Scoped"/> service registration to the collection for the specified types.
	/// <para>
	/// If an explicit constructor is matched for the provided <paramref name="constructorSelectionType"/> then the registration will use
	/// an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>.
	/// </para>
	/// </summary>
	public static IServiceCollection AddScopedWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		if (explicitConstructorToUse is null)
			return serviceCollection.AddScoped<TServiceType, TImplementationType>();

		serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));
		return serviceCollection;
	}

	/// <summary>
	/// Add a <see cref="ServiceLifetime.Scoped"/>  service registration to the collection for the specified types.
	/// <para>
	/// The registration will use an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>
	/// to ensure the provided <paramref name="explicitConstructorToUse"/> is called to construct the instance.
	/// </para>
	/// </summary>
	public static IServiceCollection AddScopedWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorInfo explicitConstructorToUse)
		where TImplementationType : class
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));

		return serviceCollection;
	}

	/// <summary>
	/// Add a <see cref="ServiceLifetime.Singleton"/> service registration to the collection for the specified types.
	/// <para>
	/// If an explicit constructor is matched for the provided <paramref name="constructorSelectionType"/> then the registration will use
	/// an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>.
	/// </para>
	/// </summary>
	public static IServiceCollection AddSingletonWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		if (explicitConstructorToUse is null)
			return serviceCollection.AddSingleton<TServiceType, TImplementationType>();

		serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));
		return serviceCollection;
	}

	/// <summary>
	/// Add a <see cref="ServiceLifetime.Singleton"/>  service registration to the collection for the specified types.
	/// <para>
	/// The registration will use an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>
	/// to ensure the provided <paramref name="explicitConstructorToUse"/> is called to construct the instance.
	/// </para>
	/// </summary>
	public static IServiceCollection AddSingletonWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorInfo explicitConstructorToUse)
		where TImplementationType : class
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));

		return serviceCollection;
	}

	/// <summary>
	/// Add a <see cref="ServiceLifetime.Transient"/> service registration to the collection for the specified types.
	/// <para>
	/// If an explicit constructor is matched for the provided <paramref name="constructorSelectionType"/> then the registration will use
	/// an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>.
	/// </para>
	/// </summary>
	public static IServiceCollection AddTransientWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		if (explicitConstructorToUse is null)
			return serviceCollection.AddTransient<TServiceType, TImplementationType>();

		serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));
		return serviceCollection;
	}

	/// <summary>
	/// Add a <see cref="ServiceLifetime.Transient"/>  service registration to the collection for the specified types.
	/// <para>
	/// The registration will use an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>
	/// to ensure the provided <paramref name="explicitConstructorToUse"/> is called to construct the instance.
	/// </para>
	/// </summary>
	public static IServiceCollection AddTransientWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorInfo explicitConstructorToUse)
		where TImplementationType : class
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		serviceCollection.Add(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));

		return serviceCollection;
	}

	/// <summary>
	/// Apply a <see cref="AttributeDecoratedConvention"/> to register types discovered by the <paramref name="scanner"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterAttributeDecoratedTypes(
		this IServiceCollection serviceCollection,
		IAssemblyScanner scanner)
	{
		_ = scanner ?? throw new ArgumentNullException(nameof(scanner));
		return RegisterAttributeDecoratedTypes(serviceCollection, scanner.ScanAssemblies());
	}

	/// <summary>
	/// Apply a <see cref="AttributeDecoratedConvention"/> to register the discovered types in the <paramref name="scanResult"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterAttributeDecoratedTypes(
		this IServiceCollection serviceCollection,
		IAssemblyScanResult scanResult)
		=> RegisterTypes(serviceCollection, scanResult, new AttributeDecoratedConvention());

	/// <summary>
	/// Apply a <see cref="ConcreteTypeAsSelfConvention"/> to register types discovered by the <paramref name="scanner"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterConcreteTypesAsSelf(
		this IServiceCollection serviceCollection,
		IAssemblyScanner scanner,
		bool skipImplementationTypesAlreadyInUse = true)
	{
		_ = scanner ?? throw new ArgumentNullException(nameof(scanner));
		return RegisterConcreteTypesAsSelf(serviceCollection, scanner.ScanAssemblies(), skipImplementationTypesAlreadyInUse);
	}

	/// <summary>
	/// Apply a <see cref="ConcreteTypeAsSelfConvention"/> to register the discovered types in the <paramref name="scanResult"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterConcreteTypesAsSelf(
		this IServiceCollection serviceCollection,
		IAssemblyScanResult scanResult,
		bool skipImplementationTypesAlreadyInUse = true)
		=> RegisterTypes(serviceCollection, scanResult, new ConcreteTypeAsSelfConvention(skipImplementationTypesAlreadyInUse: skipImplementationTypesAlreadyInUse));

	/// <summary>
	/// Apply an <see cref="InterfaceImplementationsConvention"/> to register types discovered by the <paramref name="scanner"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterInterfaceImplementations(
		this IServiceCollection serviceCollection,
		IAssemblyScanner scanner,
		InterfaceImplementationResolutionStrategy resolutionStrategy)
	{
		_ = scanner ?? throw new ArgumentNullException(nameof(scanner));
		return RegisterInterfaceImplementations(serviceCollection, scanner.ScanAssemblies(), resolutionStrategy);
	}

	/// <summary>
	/// Apply an <see cref="InterfaceImplementationsConvention"/> to register the discovered types in the <paramref name="scanResult"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterInterfaceImplementations(
		this IServiceCollection serviceCollection,
		IAssemblyScanResult scanResult,
		InterfaceImplementationResolutionStrategy resolutionStrategy)
		=> RegisterTypes(serviceCollection, scanResult, new InterfaceImplementationsConvention(resolutionStrategy: resolutionStrategy));

	/// <summary>
	/// Executed the provided <paramref name="serviceRegistrationConvention"/> to register types discovered by the <paramref name="scanner"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterTypes(
		this IServiceCollection serviceCollection,
		IAssemblyScanner scanner,
		IServiceRegistrationConvention serviceRegistrationConvention)
	{
		_ = scanner ?? throw new ArgumentNullException(nameof(scanner));
		return RegisterTypes(serviceCollection, scanner.ScanAssemblies(), serviceRegistrationConvention);
	}

	/// <summary>
	/// Executed the provided <paramref name="serviceRegistrationConvention"/> to register the <paramref name="discoveredTypes"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterTypes(
		this IServiceCollection serviceCollection,
		IEnumerable<Type> discoveredTypes,
		IServiceRegistrationConvention serviceRegistrationConvention)
	{
		_ = discoveredTypes ?? throw new ArgumentNullException(nameof(discoveredTypes));
		var assemblyScanner = AssemblyScanner.CreateDefault().IncludeTypes(discoveredTypes);

		return RegisterTypes(serviceCollection, assemblyScanner.ScanAssemblies(), serviceRegistrationConvention);
	}

	/// <summary>
	/// Executed the provided <paramref name="serviceRegistrationConvention"/> to register the discovered types in the <paramref name="scanResult"/>
	/// against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterTypes(
		this IServiceCollection serviceCollection,
		IAssemblyScanResult scanResult,
		IServiceRegistrationConvention serviceRegistrationConvention)
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
		_ = scanResult ?? throw new ArgumentNullException(nameof(scanResult));
		_ = serviceRegistrationConvention ?? throw new ArgumentNullException(nameof(serviceRegistrationConvention));

		if (serviceCollection is not ServiceRegistrationCache serviceRegistrationCache)
		{
			// Wrap serviceCollection as a ServiceRegistrationCache, if not already wrapped
			serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);
		}

		foreach (var discoveredType in scanResult.AllDiscoveredTypes)
			_ = serviceRegistrationConvention.HandleType(discoveredType, scanResult, serviceRegistrationCache);

		return serviceRegistrationCache;
	}

	/// <summary>
	/// Executed the provided collection of <paramref name="serviceRegistrationConventions"/> to register the types discovered by
	/// the <paramref name="scanner"/> against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterTypes(
		this IServiceCollection serviceCollection,
		IAssemblyScanner scanner,
		IEnumerable<IServiceRegistrationConvention> serviceRegistrationConventions,
		bool tryAllConventions = false)
	{
		_ = scanner ?? throw new ArgumentNullException(nameof(scanner));
		return RegisterTypes(serviceCollection, scanner.ScanAssemblies(), serviceRegistrationConventions, tryAllConventions);
	}

	/// <summary>
	/// Executed the provided collection of <paramref name="serviceRegistrationConventions"/> to register the
	/// <paramref name="discoveredTypes"/> against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterTypes(
		this IServiceCollection serviceCollection,
		IEnumerable<Type> discoveredTypes,
		IEnumerable<IServiceRegistrationConvention> serviceRegistrationConventions,
		bool tryAllConventions = false)
	{
		_ = discoveredTypes ?? throw new ArgumentNullException(nameof(discoveredTypes));
		var assemblyScanner = AssemblyScanner.CreateDefault().IncludeTypes(discoveredTypes);

		return RegisterTypes(serviceCollection, assemblyScanner.ScanAssemblies(), serviceRegistrationConventions, tryAllConventions);
	}

	/// <summary>
	/// Executed the provided collection of <paramref name="serviceRegistrationConventions"/> to register the discovered types in
	/// the <paramref name="scanResult"/> against the <paramref name="serviceCollection" />.
	/// </summary>
	public static IServiceCollection RegisterTypes(
		this IServiceCollection serviceCollection,
		IAssemblyScanResult scanResult,
		IEnumerable<IServiceRegistrationConvention> serviceRegistrationConventions,
		bool tryAllConventions = false)
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
		_ = scanResult ?? throw new ArgumentNullException(nameof(scanResult));
		_ = serviceRegistrationConventions ?? throw new ArgumentNullException(nameof(serviceRegistrationConventions));

		if (serviceCollection is not ServiceRegistrationCache serviceRegistrationCache)
		{
			// Wrap serviceCollection as a ServiceRegistrationCache, if not already wrapped
			serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);
		}

		foreach (var discoveredType in scanResult.AllDiscoveredTypes)
		{
			foreach (var convention in serviceRegistrationConventions)
			{
				if (convention.HandleType(discoveredType, scanResult, serviceRegistrationCache) && !tryAllConventions)
					break;
			}
		}

		return serviceRegistrationCache;
	}

	/// <summary>
	/// Try adding a <see cref="ServiceLifetime.Scoped"/> service registration to the collection for the specified types, only if
	/// an existing descriptor for <typeparamref name="TServiceType"/> does not already exist.
	/// <para>
	/// If an explicit constructor is matched for the provided <paramref name="constructorSelectionType"/> then the registration will use
	/// an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>.
	/// </para>
	/// </summary>
	public static IServiceCollection TryAddScopedWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		if (explicitConstructorToUse is null)
		{
			serviceCollection.TryAddScoped<TServiceType, TImplementationType>();
			return serviceCollection;
		}

		serviceCollection.TryAdd(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));
		return serviceCollection;
	}

	/// <summary>
	/// Try adding a <see cref="ServiceLifetime.Scoped"/>  service registration to the collection for the specified types, only if
	/// an existing descriptor for <typeparamref name="TServiceType"/> does not already exist.
	/// <para>
	/// The registration will use an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>
	/// to ensure the provided <paramref name="explicitConstructorToUse"/> is called to construct the instance.
	/// </para>
	/// </summary>
	public static IServiceCollection TryAddScopedWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorInfo explicitConstructorToUse)
		where TImplementationType : class
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		serviceCollection.TryAdd(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Scoped));

		return serviceCollection;
	}

	/// <summary>
	/// Try adding a <see cref="ServiceLifetime.Singleton"/> service registration to the collection for the specified types, only if
	/// an existing descriptor for <typeparamref name="TServiceType"/> does not already exist.
	/// <para>
	/// If an explicit constructor is matched for the provided <paramref name="constructorSelectionType"/> then the registration will use
	/// an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>.
	/// </para>
	/// </summary>
	public static IServiceCollection TryAddSingletonWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		if (explicitConstructorToUse is null)
		{
			serviceCollection.TryAddSingleton<TServiceType, TImplementationType>();
			return serviceCollection;
		}

		serviceCollection.TryAdd(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));
		return serviceCollection;
	}

	/// <summary>
	/// Try adding a <see cref="ServiceLifetime.Singleton"/>  service registration to the collection for the specified types, only if
	/// an existing descriptor for <typeparamref name="TServiceType"/> does not already exist.
	/// <para>
	/// The registration will use an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>
	/// to ensure the provided <paramref name="explicitConstructorToUse"/> is called to construct the instance.
	/// </para>
	/// </summary>
	public static IServiceCollection TryAddSingletonWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorInfo explicitConstructorToUse)
		where TImplementationType : class
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		serviceCollection.TryAdd(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Singleton));

		return serviceCollection;
	}

	/// <summary>
	/// Try adding a <see cref="ServiceLifetime.Transient"/> service registration to the collection for the specified types, only if
	/// an existing descriptor for <typeparamref name="TServiceType"/> does not already exist.
	/// <para>
	/// If an explicit constructor is matched for the provided <paramref name="constructorSelectionType"/> then the registration will use
	/// an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>.
	/// </para>
	/// </summary>
	public static IServiceCollection TryAddTransientWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorSelectionType constructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		var explicitConstructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		if (explicitConstructorToUse is null)
		{
			serviceCollection.TryAddTransient<TServiceType, TImplementationType>();
			return serviceCollection;
		}

		serviceCollection.TryAdd(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));
		return serviceCollection;
	}

	/// <summary>
	/// Try adding a <see cref="ServiceLifetime.Transient"/>  service registration to the collection for the specified types, only if
	/// an existing descriptor for <typeparamref name="TServiceType"/> does not already exist.
	/// <para>
	/// The registration will use an <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/>
	/// to ensure the provided <paramref name="explicitConstructorToUse"/> is called to construct the instance.
	/// </para>
	/// </summary>
	public static IServiceCollection TryAddTransientWithConstructorSelection<TServiceType, TImplementationType>(
		this IServiceCollection serviceCollection,
		ConstructorInfo explicitConstructorToUse)
		where TImplementationType : class
	{
		_ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

		serviceCollection.TryAdd(new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), explicitConstructorToUse, ServiceLifetime.Transient));

		return serviceCollection;
	}
}
