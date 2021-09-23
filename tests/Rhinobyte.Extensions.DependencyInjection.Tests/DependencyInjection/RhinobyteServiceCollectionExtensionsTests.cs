using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests
{
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

			serviceCollection.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			var serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

			serviceCollection.Clear();
			var constuctorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.AddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
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

			serviceCollection.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			var serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

			serviceCollection.Clear();
			var constuctorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.AddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
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

			serviceCollection.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			var serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

			serviceCollection.Clear();
			var constuctorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.AddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
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
		public void TryAddScopedWithConstructorSelection_behaves_as_expected()
		{
			Invoking(() => RhinobyteServiceCollectionExtensions.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*serviceCollection*");

			var serviceCollection = new ServiceCollection();

			serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			var serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

			serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.Count.Should().Be(1);

			serviceCollection.Clear();
			var constuctorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
			serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

			serviceCollection.TryAddScopedWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
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
			Invoking(() => RhinobyteServiceCollectionExtensions.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*serviceCollection*");

			var serviceCollection = new ServiceCollection();

			serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			var serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

			serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.Count.Should().Be(1);

			serviceCollection.Clear();
			var constuctorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
			serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

			serviceCollection.TryAddSingletonWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
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
			Invoking(() => RhinobyteServiceCollectionExtensions.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*serviceCollection*");

			var serviceCollection = new ServiceCollection();

			serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			var serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

			serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.Count.Should().Be(1);

			serviceCollection.Clear();
			var constuctorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
			serviceDescriptor = serviceCollection.Single();
			serviceDescriptor.Should().BeOfType<ExplicitConstructorServiceDescriptor<ExplicitConstructorType>>();
			serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);

			serviceCollection.TryAddTransientWithConstructorSelection<IExplicitConstructorType, ExplicitConstructorType>(constuctorToUse!);
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
}
