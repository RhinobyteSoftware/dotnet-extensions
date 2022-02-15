using ExampleLibrary1;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.Tests;

[TestClass]
public class TypeExtensionTests
{
	[TestMethod]
	public void GetCommonTypeName_returns_the_expected_result()
	{
		TypeExtensions.GetCommonTypeName(null).Should().BeEmpty();
		typeof(void).GetCommonTypeName().Should().Be("void");

		typeof(string).GetCommonTypeName().Should().Be("string");
		typeof(string).GetCommonTypeName(true).Should().Be("string?");

		typeof(int).GetCommonTypeName().Should().Be("int");
		typeof(int?).GetCommonTypeName().Should().Be("int?");

		typeof(bool).GetCommonTypeName().Should().Be("bool");
		typeof(bool?).GetCommonTypeName().Should().Be("bool?");

		typeof(byte).GetCommonTypeName().Should().Be("byte");
		typeof(byte?).GetCommonTypeName().Should().Be("byte?");

		typeof(sbyte).GetCommonTypeName().Should().Be("sbyte");
		typeof(sbyte?).GetCommonTypeName().Should().Be("sbyte?");

		typeof(short).GetCommonTypeName().Should().Be("short");
		typeof(ushort).GetCommonTypeName().Should().Be("ushort");
		typeof(int).GetCommonTypeName().Should().Be("int");
		typeof(uint).GetCommonTypeName().Should().Be("uint");
		typeof(long).GetCommonTypeName().Should().Be("long");
		typeof(ulong).GetCommonTypeName().Should().Be("ulong");
		typeof(float).GetCommonTypeName().Should().Be("float");
		typeof(double).GetCommonTypeName().Should().Be("double");
		typeof(decimal).GetCommonTypeName().Should().Be("decimal");

#pragma warning disable IDE0049 // Simplify Names
		typeof(Int32).GetCommonTypeName().Should().Be("int");
		typeof(Double).GetCommonTypeName().Should().Be("double");
#pragma warning restore IDE0049 // Simplify Names
	}

	[TestMethod]
	public void GetDeclaringMember_should_return_the_expected_result()
	{
		TypeExtensions.GetDeclaringMember(null).Should().BeNull();
		typeof(ISomeOpenGenericType<,>).GetDeclaringMember().Should().BeNull();

		var methodInfo = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult1), BindingFlags.Public | BindingFlags.Instance);
		methodInfo!.ReturnType.GetDeclaringMember().Should().Be(null);
		var genericTypeArguments = methodInfo!.ReturnType.GetGenericArguments();
		genericTypeArguments.Should().NotBeNull();
		genericTypeArguments.Length.Should().Be(2);
		genericTypeArguments[0].GetDeclaringMember().Should().BeNull();
		genericTypeArguments[1].GetDeclaringMember().Should().BeNull();

		methodInfo = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult2), BindingFlags.Public | BindingFlags.Instance);
		methodInfo!.MakeGenericMethod(new Type[] { typeof(object), typeof(object) });
		methodInfo!.ReturnType.GetDeclaringMember().Should().Be(null);
		genericTypeArguments = methodInfo!.ReturnType.GetGenericArguments();
		genericTypeArguments.Should().NotBeNull();
		genericTypeArguments.Length.Should().Be(2);
		genericTypeArguments[0].GetDeclaringMember().Should().NotBeNull();
		genericTypeArguments[1].GetDeclaringMember().Should().NotBeNull();
	}

	[TestMethod]
	public void GetDisplayName_returns_the_expected_result()
	{
		TypeExtensions.GetDisplayName(null).Should().BeEmpty();

		typeof(ISomeOpenGenericType<,>).GetDisplayName().Should().Be("ISomeOpenGenericType<TOne, TTwo>");
		typeof(ISomeOpenGenericType<object, object>).GetDisplayName().Should().Be("ISomeOpenGenericType<object, object>");

		typeof(ISomeNullableOpenGenericType<,>).GetDisplayName().Should().Be("ISomeNullableOpenGenericType<TOne?, TTwo?>");
		typeof(ISomeNullableOpenGenericType<object, object>).GetDisplayName().Should().Be("ISomeNullableOpenGenericType<object, object>");

		typeof(ISomeNullableOpenGenericTypeTwo<,>).GetDisplayName().Should().Be("ISomeNullableOpenGenericTypeTwo<TOne?, TTwo?>");
		typeof(ISomeNullableOpenGenericTypeTwo<object?, object?>).GetDisplayName().Should().Be("ISomeNullableOpenGenericTypeTwo<object, object>");

		var methodInfo = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult1), BindingFlags.Public | BindingFlags.Instance);
		var returnTypeDisplayName = methodInfo!.ReturnType.GetDisplayName();
		returnTypeDisplayName.Should().Be("ISomeNullableOpenGenericType<object, object>");

		methodInfo = typeof(ClassWithGenericMethod).GetMethod(nameof(ClassWithGenericMethod.MethodWithGenericResult2), BindingFlags.Public | BindingFlags.Instance);
		returnTypeDisplayName = methodInfo!.ReturnType.GetDisplayName();
		returnTypeDisplayName.Should().Be("ISomeNullableOpenGenericTypeTwo<TOne?, TTwo?>");
		methodInfo = methodInfo.MakeGenericMethod(typeof(object), typeof(object));
		returnTypeDisplayName = methodInfo!.ReturnType.GetDisplayName();
		returnTypeDisplayName.Should().Be("ISomeNullableOpenGenericTypeTwo<object, object>");
	}

	[TestMethod]
	public void IsCompilerGenerated_returns_the_expected_result()
	{
		TypeExtensions.IsCompilerGenerated(null!).Should().BeFalse();
		typeof(AssemblyInclude).IsCompilerGenerated().Should().BeFalse();
		typeof(ISomethingOptions).IsCompilerGenerated().Should().BeFalse();
		typeof(SomethingOptions).IsCompilerGenerated().Should().BeFalse();

		var reflectionLibraryTypes = typeof(Rhinobyte.Extensions.Reflection.MethodBaseExtensions).Assembly.GetTypes();
		var compilerGeneratedType = reflectionLibraryTypes.FirstOrDefault(type => type.Name.StartsWith("<"));
		compilerGeneratedType.Should().NotBeNull();
		compilerGeneratedType!.IsCompilerGenerated().Should().BeTrue();
	}

	[TestMethod]
	public void IsOpenGeneric_returns_the_expected_result()
	{
		TypeExtensions.IsOpenGeneric(null!).Should().BeFalse();
		typeof(ISomeOpenGenericType<,>).IsOpenGeneric().Should().BeTrue();
		typeof(PartiallyOpenGeneric<>).IsOpenGeneric().Should().BeTrue();

		typeof(ClassThatClosedOpenGenericOne).IsOpenGeneric().Should().BeFalse();
		typeof(ClassThatClosedOpenGenericTwo).IsOpenGeneric().Should().BeFalse();
		typeof(ISomethingOptions).IsOpenGeneric().Should().BeFalse();
		typeof(SomethingOptions).IsOpenGeneric().Should().BeFalse();
	}

	[TestMethod]
	public void IsValueTypeOrString_returns_the_expected_result()
	{
		TypeExtensions.IsValueTypeOrString(null!).Should().BeFalse();
		typeof(DateTime).IsValueTypeOrString().Should().BeTrue();
		typeof(string).IsValueTypeOrString().Should().BeTrue();
		typeof(AssemblyInclude).IsValueTypeOrString().Should().BeTrue();

		typeof(object).IsValueTypeOrString().Should().BeFalse();
		typeof(ISomethingOptions).IsValueTypeOrString().Should().BeFalse();
		typeof(SomethingOptions).IsValueTypeOrString().Should().BeFalse();
	}
}
