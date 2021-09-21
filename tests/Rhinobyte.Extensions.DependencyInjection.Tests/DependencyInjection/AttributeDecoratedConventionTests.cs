using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests.DependencyInjection
{
	[TestClass]
	public class AttributeDecoratedConventionTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void HandleType_ignores_already_registered_types()
		{
			var scanResult = new AssemblyScanResult();
			var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
			var systemUnderTest = new AttributeDecoratedConvention();

			systemUnderTest.HandleType(typeof(ClassWithRegisterAttribute), scanResult, serviceRegistrationCache)
				.Should().BeTrue();
			serviceRegistrationCache.Count.Should().Be(1);

			systemUnderTest.HandleType(typeof(ClassWithRegisterAttribute), scanResult, serviceRegistrationCache)
				.Should().BeFalse();
			serviceRegistrationCache.Count.Should().Be(1);
		}

		[TestMethod]
		public void HandleType_registers_types_decorated_with_the_attribute()
		{
			var scanResult = new AssemblyScanResult();
			var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
			var systemUnderTest = new AttributeDecoratedConvention();

			systemUnderTest.HandleType(typeof(ClassWithRegisterAttribute), scanResult, serviceRegistrationCache)
				.Should().BeTrue();
			serviceRegistrationCache.Count.Should().Be(1);

			systemUnderTest.HandleType(typeof(ITypeWithRegisterAttribute), scanResult, serviceRegistrationCache)
				.Should().BeTrue();
			serviceRegistrationCache.Count.Should().Be(2);
		}

		[TestMethod]
		public void HandleType_throws_if_the_type_is_decorated_with_a_null_implementation_type()
		{
			var scanResult = new AssemblyScanResult();
			var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
			var systemUnderTest = new AttributeDecoratedConvention();

			Invoking(() => systemUnderTest.HandleType(typeof(Class_WithRegisterAttribute_NullImplementationType), scanResult, serviceRegistrationCache))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"* is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} that has a null value for {nameof(RegisterForDependencyInjectionAttribute.ImplementationType)}");
			serviceRegistrationCache.Count.Should().Be(0);

			Invoking(() => systemUnderTest.HandleType(typeof(IInterface_WithRegisterAttribute_NullImplemenationType), scanResult, serviceRegistrationCache))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"* is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} that has a null value for {nameof(RegisterForDependencyInjectionAttribute.ImplementationType)}");

			serviceRegistrationCache.Count.Should().Be(0);
		}

		[TestMethod]
		public void HandleType_throws_if_the_type_is_decorated_with_an_invalid_implementation_type()
		{
			var scanResult = new AssemblyScanResult();
			var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
			var systemUnderTest = new AttributeDecoratedConvention();

			Invoking(() => systemUnderTest.HandleType(typeof(Class_WithRegisterAttribute_InvalidImplementationType), scanResult, serviceRegistrationCache))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"* is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} with an invalid implementationType of *");
			serviceRegistrationCache.Count.Should().Be(0);

			Invoking(() => systemUnderTest.HandleType(typeof(IInterface_WithRegisterAttribute_InvalidImplementationType), scanResult, serviceRegistrationCache))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"* is decorated with a {nameof(RegisterForDependencyInjectionAttribute)} with an invalid implementationType of *");
			serviceRegistrationCache.Count.Should().Be(0);
		}



		/******     TEST SETUP     *****************************
		 *******************************************************/
		[RegisterForDependencyInjection(typeof(SomethingOptions))]
		public class Class_WithRegisterAttribute_InvalidImplementationType { }

		[RegisterForDependencyInjection(typeof(SomethingOptions))]
		public interface IInterface_WithRegisterAttribute_InvalidImplementationType { }

		[RegisterForDependencyInjection(null!)]
		public class Class_WithRegisterAttribute_NullImplementationType { }

		[RegisterForDependencyInjection(null!)]
		public interface IInterface_WithRegisterAttribute_NullImplemenationType { }
	}
}
