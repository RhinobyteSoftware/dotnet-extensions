using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests
{
	[TestClass]
	public class ExplicitConstructorFactoryTests
	{
		[TestMethod]
		public void Constructor_throws_ArgumentNullException_for_missing_constructor_to_use_argument()
		{
			Invoking(() => new ExplicitConstructorFactory(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*explicitConstructorToUse*");

			Invoking(() => new ExplicitConstructorFactory<SomethingOptions>(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*explicitConstructorToUse*");
		}

		[TestMethod]
		public void CallConstructor_successfully_constructs_the_implementation1()
		{
			using var serviceProvider = new ServiceCollection().BuildServiceProvider();

			var somethingOptionsConstructor = typeof(SomethingOptions).GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
			var explicitConstructorFactory = new ExplicitConstructorFactory(somethingOptionsConstructor);

			var somethingOptionsInstance = explicitConstructorFactory.CallConstructor(serviceProvider);
			somethingOptionsInstance.Should().NotBeNull().And.BeOfType<SomethingOptions>();

			var explicitConstructorFactory2 = new ExplicitConstructorFactory<SomethingOptions>(somethingOptionsConstructor);
			var somethingOptionsInstance2 = explicitConstructorFactory2.CallConstructor(serviceProvider);
			somethingOptionsInstance2.Should().NotBeNull().And.BeOfType<SomethingOptions>();
		}

		[TestMethod]
		public void CallConstructor_successfully_constructs_the_implementation2()
		{
			using var serviceProvider = new ServiceCollection()
				.AddExampleLibrary1()
				.BuildServiceProvider();

			var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.AttributeThenDefaultBehavior);
			var explicitConstructorFactory = new ExplicitConstructorFactory(constructorToUse!);

			var somethingOptionsInstance = explicitConstructorFactory.CallConstructor(serviceProvider);
			somethingOptionsInstance.Should().NotBeNull().And.BeOfType<ExplicitConstructorType>();
			((ExplicitConstructorType)somethingOptionsInstance).SomethingOptions.Should().NotBeNull();

			var explicitConstructorFactory2 = new ExplicitConstructorFactory<ExplicitConstructorType>(constructorToUse!);
			var somethingOptionsInstance2 = explicitConstructorFactory2.CallConstructor(serviceProvider);
			somethingOptionsInstance2.Should().NotBeNull().And.BeOfType<ExplicitConstructorType>();
			somethingOptionsInstance2.SomethingOptions.Should().NotBeNull();
		}
	}
}
