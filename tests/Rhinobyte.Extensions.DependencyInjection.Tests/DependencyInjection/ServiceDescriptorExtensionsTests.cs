using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection.Tests
{
	[TestClass]
	public class ServiceDescriptorExtensionsTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void TryGetImplementationType_gracefully_handles_nulls()
		{
			ServiceDescriptorExtensions.TryGetImplementationType(null!).Should().Be(null);

			var serviceDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
			var implementationTypeField = typeof(ServiceDescriptor).GetField("<ImplementationType>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			implementationTypeField!.SetValue(serviceDescriptor, null);

			serviceDescriptor.TryGetImplementationType().Should().BeNull();
		}

		[TestMethod]
		public void TryGetImplementationType_returns_the_expected_result_for_custom_descriptors()
		{
			var constructorToUse = typeof(SomethingOptions).GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
			ServiceDescriptor serviceDescriptor = new ExplicitConstructorServiceDescriptor(typeof(ISomethingOptions), typeof(SomethingOptions), constructorToUse, ServiceLifetime.Scoped);
			serviceDescriptor.TryGetImplementationType().Should().Be<SomethingOptions>();

			serviceDescriptor = new ExplicitConstructorServiceDescriptor<SomethingOptions>(typeof(ISomethingOptions), constructorToUse, ServiceLifetime.Scoped);
			serviceDescriptor.TryGetImplementationType().Should().Be<SomethingOptions>();
		}

		[TestMethod]
		public void TryGetImplementationType_returns_the_expected_result_for_normal_descriptors()
		{
			var serviceDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
			serviceDescriptor.TryGetImplementationType().Should().Be<SomethingOptions>();

			serviceDescriptor = ServiceDescriptor.Singleton<ISomethingOptions>(new SomethingOptions());
			serviceDescriptor.TryGetImplementationType().Should().Be<SomethingOptions>();

			serviceDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>(serviceProvider => new SomethingOptions());
			serviceDescriptor.TryGetImplementationType().Should().Be<SomethingOptions>();

			serviceDescriptor = ServiceDescriptor.Scoped<ISomethingOptions>(serviceProvider => new SomethingOptions());
			serviceDescriptor.TryGetImplementationType().Should().Be<ISomethingOptions>();
		}
	}
}
