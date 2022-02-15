using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Linq;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class RhinobyteServiceCollectionExtensionsTests
{
	[TestMethod]
	public void AddScopedWithConstructorSelection_behaves_as_expected()
	{
		Invoking(() => RhinobyteServiceCollectionExtensions.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		var serviceCollection = new ServiceCollection();
		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);

		Invoking(() => RhinobyteServiceCollectionExtensions.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!, constructorToUse!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(serviceCollection, explicitConstructorToUse: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*explicitConstructorToUse*");

		serviceCollection.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		var serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

		serviceCollection.Clear();

		serviceCollection.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

		serviceCollection.Clear();
		serviceCollection.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().NotBeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Should().BeOfType<ServiceDescriptor>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	[TestMethod]
	public void AddSingletonWithConstructorSelection_behaves_as_expected()
	{
		Invoking(() => RhinobyteServiceCollectionExtensions.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		var serviceCollection = new ServiceCollection();
		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);

		Invoking(() => RhinobyteServiceCollectionExtensions.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!, constructorToUse!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(serviceCollection, explicitConstructorToUse: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*explicitConstructorToUse*");

		serviceCollection.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		var serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

		serviceCollection.Clear();
		serviceCollection.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

		serviceCollection.Clear();
		serviceCollection.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().NotBeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Should().BeOfType<ServiceDescriptor>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
	}

	[TestMethod]
	public void AddTransientWithConstructorSelection_behaves_as_expected()
	{
		Invoking(() => RhinobyteServiceCollectionExtensions.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		var serviceCollection = new ServiceCollection();
		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);

		Invoking(() => RhinobyteServiceCollectionExtensions.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!, constructorToUse!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(serviceCollection, explicitConstructorToUse: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*explicitConstructorToUse*");

		serviceCollection.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		var serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

		serviceCollection.Clear();
		serviceCollection.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

		serviceCollection.Clear();
		serviceCollection.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().NotBeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Should().BeOfType<ServiceDescriptor>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
	}

	[TestMethod]
	public void RegisterInterfaceImplementations_throws_ArgumentNullException_for_a_null_scanner_argument()
	{
		var serviceCollection = new ServiceCollection();
		Invoking(() => serviceCollection.RegisterInterfaceImplementations(scanner: null!, InterfaceImplementationResolutionStrategy.DefaultConventionOnly))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanner*");
	}

	[TestMethod]
	public void RegisterTypes_overloads_for_a_single_registration_convention_throws_ArgumentNullException_for_null_arguments()
	{
		var scanner = AssemblyScanner.CreateDefault().AddExampleLibrary1();
		var serviceRegistrationConvention = new AttributeDecoratedConvention();

		IServiceCollection serviceCollection = new ServiceCollection();
		serviceCollection = serviceCollection.RegisterTypes(scanner, serviceRegistrationConvention);
		serviceCollection.Should().BeOfType<ServiceRegistrationCache>();

		var serviceRegistrationCache = (ServiceRegistrationCache)serviceCollection;
		serviceRegistrationCache.HasExistingMatch(typeof(ClassWithRegisterAttribute), typeof(ClassWithRegisterAttribute)).Should().BeTrue();
		serviceRegistrationCache.HasExistingMatch(typeof(ITypeWithRegisterAttribute), typeof(TypeWithRegisterAttribute)).Should().BeTrue();
		serviceRegistrationCache.HasAnyByServiceType(typeof(ISomethingOptions)).Should().BeFalse();
	}

	[TestMethod]
	public void RegisterTypes_overloads_for_a_single_registration_convention_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();
		var scanner = AssemblyScanner.CreateDefault();
		var serviceRegistrationConvention = new AttributeDecoratedConvention();

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: null!, scanner: scanner, serviceRegistrationConvention: serviceRegistrationConvention))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanner: null!, serviceRegistrationConvention: serviceRegistrationConvention))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanner*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanner: scanner, serviceRegistrationConvention: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationConvention*");

		var scanResult = new AssemblyScanResult();

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: null!, scanResult: scanResult, serviceRegistrationConvention: serviceRegistrationConvention))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanResult: null!, serviceRegistrationConvention: serviceRegistrationConvention))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanResult*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanResult: scanResult, serviceRegistrationConvention: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationConvention*");
	}

	[TestMethod]
	public void RegisterTypes_overloads_for_multiple_registration_conventions_behaves_as_expected()
	{
		var scanner = AssemblyScanner.CreateDefault().AddExampleLibrary1();
		var serviceRegistrationConventions = new IServiceRegistrationConvention[]
		{
				new AttributeDecoratedConvention(),
				new InterfaceImplementationsConvention()
		};

		IServiceCollection serviceCollection = new ServiceCollection();
		serviceCollection = serviceCollection.RegisterTypes(scanner, serviceRegistrationConventions);
		serviceCollection.Should().BeOfType<ServiceRegistrationCache>();

		var serviceRegistrationCache = (ServiceRegistrationCache)serviceCollection;
		serviceRegistrationCache.HasExistingMatch(typeof(ClassWithRegisterAttribute), typeof(ClassWithRegisterAttribute)).Should().BeTrue();
		serviceRegistrationCache.HasExistingMatch(typeof(ISomethingOptions), typeof(SomethingOptions)).Should().BeTrue();
	}

	[TestMethod]
	public void RegisterTypes_overloads_for_multiple_registration_conventions_throws_ArgumentNullException_for_null_arguments()
	{
		var serviceCollection = new ServiceCollection();
		var scanner = AssemblyScanner.CreateDefault();
		var serviceRegistrationConventions = new IServiceRegistrationConvention[]
		{
				new AttributeDecoratedConvention(),
				new InterfaceImplementationsConvention()
		};

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: null!, scanner: scanner, serviceRegistrationConventions: serviceRegistrationConventions))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanner: null!, serviceRegistrationConventions: serviceRegistrationConventions))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanner*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanner: scanner, serviceRegistrationConventions: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationConventions*");

		var scanResult = new AssemblyScanResult();

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: null!, scanResult: scanResult, serviceRegistrationConventions: serviceRegistrationConventions))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanResult: null!, serviceRegistrationConventions: serviceRegistrationConventions))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanResult*");

		Invoking(() => RhinobyteServiceCollectionExtensions.RegisterTypes(serviceCollection: serviceCollection, scanResult: scanResult, serviceRegistrationConventions: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationConventions*");
	}

	[TestMethod]
	public void TryAddScopedWithConstructorSelection_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();
		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);

		Invoking(() => RhinobyteServiceCollectionExtensions.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(serviceCollection: null!, explicitConstructorToUse: constructorToUse!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		var serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

		serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		serviceCollection.Count.Should().Be(1);

		serviceCollection.Clear();

		serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

		serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceCollection.Count.Should().Be(1);

		serviceCollection.Clear();
		serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().NotBeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Should().BeOfType<ServiceDescriptor>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

		serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceCollection.Count.Should().Be(1);
	}

	[TestMethod]
	public void TryAddSingletonWithConstructorSelection_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();
		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);

		Invoking(() => RhinobyteServiceCollectionExtensions.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(serviceCollection: null!, explicitConstructorToUse: constructorToUse!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		var serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

		serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		serviceCollection.Count.Should().Be(1);

		serviceCollection.Clear();
		serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

		serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceCollection.Count.Should().Be(1);

		serviceCollection.Clear();
		serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().NotBeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Should().BeOfType<ServiceDescriptor>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

		serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceCollection.Count.Should().Be(1);
	}

	[TestMethod]
	public void TryAddTransientWithConstructorSelection_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();
		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);

		Invoking(() => RhinobyteServiceCollectionExtensions.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		Invoking(() => RhinobyteServiceCollectionExtensions.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(serviceCollection: null!, explicitConstructorToUse: constructorToUse!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");

		serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		var serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

		serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
		serviceCollection.Count.Should().Be(1);

		serviceCollection.Clear();
		serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

		serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constructorToUse!);
		serviceCollection.Count.Should().Be(1);

		serviceCollection.Clear();
		serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceDescriptor = serviceCollection.Single();
		serviceDescriptor.Should().NotBeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
		serviceDescriptor.Should().BeOfType<ServiceDescriptor>();
		serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

		serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.DefaultBehaviorOnly);
		serviceCollection.Count.Should().Be(1);
	}
}
