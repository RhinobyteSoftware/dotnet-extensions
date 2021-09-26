using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;

namespace Rhinobyte.Extensions.DependencyInjection.Tests.DependencyInjection
{
	[TestClass]
	public class ConcreteTypesAsSelfConventionTests
	{
		[TestMethod]
		public void GetServiceRegistrationParameters_ignores_non_concrete_types()
		{
			var scanResult = new AssemblyScanResult();
			var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

			var systemUnderTest = new ConcreteTypeAsSelfConvention();

			systemUnderTest.GetServiceRegistrationParameters(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
				.Should().BeNull();

			systemUnderTest.GetServiceRegistrationParameters(typeof(AbstractSomething), scanResult, serviceRegistrationCache)
				.Should().BeNull();
		}

		[TestMethod]
		public void GetServiceRegistrationParameters_returns_the_expected_result_for_a_concrete_type()
		{
			var scanResult = new AssemblyScanResult();
			var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

			var systemUnderTest = new ConcreteTypeAsSelfConvention();

			var discoveredType = typeof(SomethingService);
			var serviceRegistrationParameters = systemUnderTest.GetServiceRegistrationParameters(discoveredType, scanResult, serviceRegistrationCache);
			serviceRegistrationParameters.Should().NotBeNull();
			serviceRegistrationParameters!.ServiceDescriptor.Should().NotBeNull();
			serviceRegistrationParameters.ServiceDescriptor!.ServiceType.Should().Be(discoveredType);
			serviceRegistrationParameters.ServiceDescriptor.ImplementationType.Should().Be(discoveredType);
			serviceRegistrationParameters.ServiceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
		}
	}
}
