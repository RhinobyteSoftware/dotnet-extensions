using ExampleLibrary1;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests
{
	[TestClass]
	public class ParameterInfoExtensionsTests
	{

		[TestMethod]
		public void IsNullableReferenceType_returns_the_expected_result()
		{
			var methodInfo = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult3), BindingFlags.Public | BindingFlags.Instance);
			var parameters = methodInfo!.GetParameters();
			parameters[0].IsNullableReferenceType().Should().BeTrue();
			parameters[1].IsNullableReferenceType().Should().BeTrue();

			var methodInfo2 = methodInfo.MakeGenericMethod(typeof(object), typeof(object));
			parameters = methodInfo2!.GetParameters();
			parameters[0].IsNullableReferenceType().Should().BeTrue();
			parameters[1].IsNullableReferenceType().Should().BeTrue();

			var methodInfo3 = methodInfo.MakeGenericMethod(typeof(bool?), typeof(int?));
			parameters = methodInfo3!.GetParameters();
			parameters[0].IsNullableReferenceType().Should().BeTrue();
			parameters[1].IsNullableReferenceType().Should().BeTrue();

			var methodInfo4 = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult4), BindingFlags.Public | BindingFlags.Instance);
			parameters = methodInfo4!.GetParameters();
			parameters[0].IsNullableReferenceType().Should().BeFalse();
			parameters[1].IsNullableReferenceType().Should().BeFalse();
		}

		[TestMethod]
		public void IsNullableReferenceType_should_throw_ArgumentNullException_for_null_parameter_info()
		{
			Invoking(() => ParameterInfoExtensions.IsNullableReferenceType(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*parameterInfo*");
		}

		[TestMethod]
		public void IsNullableType_returns_the_expected_result()
		{
			var methodInfo = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult3), BindingFlags.Public | BindingFlags.Instance);
			var parameters = methodInfo!.GetParameters();
			parameters[0].IsNullableType().Should().BeTrue();
			parameters[1].IsNullableType().Should().BeTrue();

			var methodInfo2 = methodInfo.MakeGenericMethod(typeof(bool?), typeof(int?));
			parameters = methodInfo2!.GetParameters();
			parameters[0].IsNullableType().Should().BeTrue();
			parameters[1].IsNullableType().Should().BeTrue();

			var methodInfo3 = methodInfo.MakeGenericMethod(typeof(bool), typeof(int));
			parameters = methodInfo3!.GetParameters();
			parameters[0].IsNullableType().Should().BeFalse();
			parameters[1].IsNullableType().Should().BeFalse();
		}

		[TestMethod]
		public void IsNullableType_should_throw_ArgumentNullException_for_null_parameter_info()
		{
			Invoking(() => ParameterInfoExtensions.IsNullableType(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*parameterInfo*");
		}
	}
}
