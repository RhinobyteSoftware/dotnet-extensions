using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System.Linq;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class InterfaceImplementationsConventionTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void HandleType_does_not_ignores_already_registered_types_when_the_SkipAlreadyRegistered_property_is_false()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				ServiceDescriptor.Singleton<ISomethingOptions>(new SomethingOptions())
			};

		var systemUnderTest = new InterfaceImplementationsConvention(
			defaultOverwriteBehavior: ServiceRegistrationOverwriteBehavior.Add,
			skipAlreadyRegistered: false,
			skipDuplicates: false,
			skipImplementationTypesAlreadyInUse: false
		);

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(2);
	}

	[TestMethod]
	public void HandleType_ignores_already_registered_types_when_the_SkipAlreadyRegistered_property_is_true()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				ServiceDescriptor.Singleton<ISomethingOptions>(new SomethingOptions())
			};

		var systemUnderTest = new InterfaceImplementationsConvention();

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(1);
	}

	[TestMethod]
	public void HandleType_ignores_in_use_implementation_types_when_the_SkipAlreadyInUseImplementationType_property_is_true()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceCollection = new ServiceCollection()
			.AddScoped<SomethingService>()
			.AddScoped<AlternateSomethingService>()
			.AddScoped<SomethingService3>();

		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

#if NET5_0_OR_GREATER
		var allResolutionStrategies = System.Enum.GetValues<InterfaceImplementationResolutionStrategy>();
#else
		var allResolutionStrategies = System.Enum.GetValues(typeof(InterfaceImplementationResolutionStrategy)).Cast<InterfaceImplementationResolutionStrategy>().ToArray();
#endif
		foreach (var resolutionStrategy in allResolutionStrategies)
		{
			var systemUnderTest = new InterfaceImplementationsConvention(
				resolutionStrategy: resolutionStrategy,
				skipImplementationTypesAlreadyInUse: true
			);

			systemUnderTest.HandleType(typeof(ISomethingService), scanResult, serviceRegistrationCache)
				.Should().BeFalse();
			serviceRegistrationCache.Count.Should().Be(3);
		}
	}

	[TestMethod]
	public void HandleType_ignores_interfaces_with_no_implementation_type()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new InterfaceImplementationsConvention();

		systemUnderTest.HandleType(typeof(IInterfaceWithNoImplementationType), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(0);
	}

	[TestMethod]
	public void HandleType_works_as_expected_for_the_AllImplementations_strategy()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new InterfaceImplementationsConvention(resolutionStrategy: InterfaceImplementationResolutionStrategy.AllImplementations);

		// All implementations should be used, even when there's a default implementation
		systemUnderTest.HandleType(typeof(ISomethingService), scanResult, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(3);
		serviceRegistrationCache.All(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ISomethingService)).Should().BeTrue();
		serviceRegistrationCache.Any(serviceDescriptor => serviceDescriptor.ImplementationType == typeof(SomethingService)).Should().BeTrue();
		serviceRegistrationCache.Any(serviceDescriptor => serviceDescriptor.ImplementationType == typeof(AlternateSomethingService)).Should().BeTrue();
		serviceRegistrationCache.Any(serviceDescriptor => serviceDescriptor.ImplementationType == typeof(SomethingService3)).Should().BeTrue();
	}

	[TestMethod]
	public void HandleType_works_as_expected_for_the_DefaultConventionOnly_strategy()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new InterfaceImplementationsConvention(resolutionStrategy: InterfaceImplementationResolutionStrategy.DefaultConventionOnly);

		systemUnderTest.HandleType(typeof(IInterfaceWithNoDefaultConventionImplementation), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(0);

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.First().ServiceType.Should().Be<ISomethingOptions>();
		serviceRegistrationCache.First().ImplementationType.Should().Be<SomethingOptions>();
	}

	[TestMethod]
	public void HandleType_works_as_expected_for_the_DefaultConventionOrAll_strategy()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new InterfaceImplementationsConvention(resolutionStrategy: InterfaceImplementationResolutionStrategy.DefaultConventionOrAll);

		// No default convention, should register all implementations
		systemUnderTest.HandleType(typeof(IInterfaceWithNoDefaultConventionImplementation), scanResult, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(2);
		serviceRegistrationCache.HasExistingMatch(typeof(IInterfaceWithNoDefaultConventionImplementation), typeof(NoDefaultConventionClass1)).Should().BeTrue();
		serviceRegistrationCache.HasExistingMatch(typeof(IInterfaceWithNoDefaultConventionImplementation), typeof(NoDefaultConventionClass2)).Should().BeTrue();

		// Default convention, should register only 1 implementation
		serviceRegistrationCache.Clear();
		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.First().ServiceType.Should().Be<ISomethingOptions>();
		serviceRegistrationCache.First().ImplementationType.Should().Be<SomethingOptions>();
	}

	[TestMethod]
	public void HandleType_works_as_expected_for_the_DefaultConventionOrSingleImplementationOnly_strategy()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new InterfaceImplementationsConvention(resolutionStrategy: InterfaceImplementationResolutionStrategy.DefaultConventionOrSingleImplementationOnly);

		// Multiple implemenations should use the default convention
		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.First().ServiceType.Should().Be<ISomethingOptions>();
		serviceRegistrationCache.First().ImplementationType.Should().Be<SomethingOptions>();

		// Multiple implementations and no default convention should be skipped
		serviceRegistrationCache.Clear();
		systemUnderTest.HandleType(typeof(IInterfaceWithNoDefaultConventionImplementation), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(0);

		var scanResult2 = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ExcludeType<ExampleLibrary1.NoDefaultConventionClass2>()
			.ScanAssemblies();

		// Single non-default implementation should be used
		systemUnderTest.HandleType(typeof(IInterfaceWithNoDefaultConventionImplementation), scanResult2, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.First().ServiceType.Should().Be<IInterfaceWithNoDefaultConventionImplementation>();
		serviceRegistrationCache.First().ImplementationType.Should().Be<NoDefaultConventionClass1>();
	}

	[TestMethod]
	public void HandleType_works_as_expected_for_the_SingleImplementationOnly_strategy()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new InterfaceImplementationsConvention(resolutionStrategy: InterfaceImplementationResolutionStrategy.SingleImplementationOnly);

		// Multiple implemenations default convention should be skipped
		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(0);

		// Multiple implementations no default convention should be skipped
		serviceRegistrationCache.Clear();
		systemUnderTest.HandleType(typeof(IInterfaceWithNoDefaultConventionImplementation), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(0);

		// Single implementation should be used
		var scanResult2 = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ExcludeType<ExampleLibrary1.NoDefaultConventionClass2>()
			.ScanAssemblies();

		// Single non-default implementation should be used
		systemUnderTest.HandleType(typeof(IInterfaceWithNoDefaultConventionImplementation), scanResult2, serviceRegistrationCache)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.First().ServiceType.Should().Be<IInterfaceWithNoDefaultConventionImplementation>();
		serviceRegistrationCache.First().ImplementationType.Should().Be<NoDefaultConventionClass1>();
	}



	/******     TEST SETUP     *****************************
	 *******************************************************/

}
