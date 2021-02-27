using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.ReflectionHelpers.UnitTests.Setup;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.ReflectionHelpers.UnitTests
{
	[TestClass]
	public class MethodBaseExtensionsUnitTests
	{
		[TestMethod]
		public void HasMatchingParameters_returns_the_expected_result()
		{
			var methodToTest1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToTest1.Should().NotBeNull();

			methodToTest1!.HasMatchingParameterTypes(new[] { typeof(string) }).Should().BeFalse();

			var methodToTest2 = typeof(ExampleMethods)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Single(methodInfo => methodInfo.Name == nameof(ExampleMethods.OverloadedMethod) && methodInfo.GetParameters().Length == 3);

			methodToTest2.Should().NotBeNull();


			methodToTest2.HasMatchingParameterTypes(new[] { typeof(int), typeof(int) }).Should().BeFalse();
			methodToTest2.HasMatchingParameterTypes(new[] { typeof(int), typeof(int), typeof(int) }).Should().BeFalse();
			methodToTest2.HasMatchingParameterTypes(new[] { typeof(int), typeof(float), typeof(int) }).Should().BeFalse();

			methodToTest2.HasMatchingParameterTypes(new[] { typeof(int), typeof(int), typeof(float) }).Should().BeTrue();
		}

		[TestMethod]
		public void HasNoParameters_returns_the_expected_result()
		{
			var methodToTest1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToTest1.Should().NotBeNull();

			methodToTest1!.HasNoParameters().Should().BeTrue();


			var methodToTest2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			methodToTest2.Should().NotBeNull();

			methodToTest2!.HasNoParameters().Should().BeFalse();
		}
	}
}
