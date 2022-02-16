using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class ExplicitConstructorFactoryTests
{
	[TestMethod]
	public void Constructor_throws_ArgumentNullException_for_missing_constructor_to_use_argument()
	{
		Invoking(() => new ExplicitConstructorFactory<SomethingOptions>(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*explicitConstructorToUse*");
	}

	[TestMethod]
	public void CallConstructor_successfully_constructs_the_implementation_when_there_is_an_option_parameter_that_cannot_be_resolved()
	{
		using var serviceProvider = new ServiceCollection()
			.AddSingleton<IManuallyConfiguredType>(new ManuallyConfiguredType(new Uri("https://fake.api.com")))
			.AddScoped<ISomethingOptions, SomethingOptions>()
			.AddScoped<ISomethingService, SomethingService>()
			.BuildServiceProvider();

		var constructorToUse = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ExplicitConstructorType), ConstructorSelectionType.MostParametersOnly);
		var explicitConstructorFactory2 = new ExplicitConstructorFactory<ExplicitConstructorType>(constructorToUse!);
		var resolvedInstance2 = explicitConstructorFactory2.CallConstructor(serviceProvider);
		resolvedInstance2.Should().NotBeNull().And.BeOfType<ExplicitConstructorType>();
		resolvedInstance2.ConstructorUsedIndex.Should().Be(4);
		resolvedInstance2.SomethingOptions.Should().NotBeNull();
		resolvedInstance2.TypeWithRegisterAttribute.Should().BeNull();
	}

	[TestMethod]
	public void CallConstructor_successfully_constructs_the_implementation1()
	{
		using var serviceProvider = new ServiceCollection().BuildServiceProvider();

		var somethingOptionsConstructor = typeof(SomethingOptions).GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
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
		var explicitConstructorFactory2 = new ExplicitConstructorFactory<ExplicitConstructorType>(constructorToUse!);
		var resolvedInstance2 = explicitConstructorFactory2.CallConstructor(serviceProvider);
		resolvedInstance2.Should().NotBeNull().And.BeOfType<ExplicitConstructorType>();
		resolvedInstance2.ConstructorUsedIndex.Should().Be(3);
		resolvedInstance2.SomethingOptions.Should().NotBeNull();
	}
}
