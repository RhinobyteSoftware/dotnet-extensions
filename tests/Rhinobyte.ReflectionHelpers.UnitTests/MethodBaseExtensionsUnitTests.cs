using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.ReflectionHelpers.UnitTests.Setup;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.UnitTests
{
	[TestClass]
	public class MethodBaseExtensionsUnitTests
	{
		[TestMethod]
		public void DescribeInstructions_returns_the_expected_result()
		{
			var methodToTest = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToTest.Should().NotBeNull();

			var methodBodyDescription = methodToTest!.DescribeInstructions();
			methodBodyDescription.Should().Be(
@"(0) NO-OP
(1) LOAD INT LITERAL (5)
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) LOAD INT VALUE (Int8)  [SByte Value: 10]
(4) SET LOCAL VARIABLE (Index 1)  [Of type Int32]
(5) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(6) LOAD LOCAL VARIABLE (Index 1)  [Of type Int32]
(7) ADD
(8) SET LOCAL VARIABLE (Index 2)  [Of type Int32]
(9) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 10]
(10) LOAD LOCAL VARIABLE (Index 2)  [Of type Int32]
(11) RETURN");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_basic()
		{
			var methodToTest = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToTest.Should().NotBeNull();

			methodToTest!.GetSignature().Should().Be("public static int AddLocalVariables_For_5_And_15()");
			methodToTest.GetSignature(true).Should().Be("public static int Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleMethods.AddLocalVariables_For_5_And_15()");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics1()
		{
			// GenericStruct<>.TryGetSomething()
			var genericMethodToTest1 = typeof(GenericStruct<>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryGetSomething");
			genericMethodToTest1.Should().NotBeNull();

			genericMethodToTest1!.GetSignature().Should().Be("public T? TryGetSomething()");
			genericMethodToTest1!.GetSignature(true).Should().Be("public T? Rhinobyte.ReflectionHelpers.UnitTests.Setup.GenericStruct<T>.TryGetSomething() where T : System.IConvertible, struct");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics2()
		{
			// ExampleGenericType<OperandType, string, OpCodeType>.TryCreate(..)
			var genericMethodToTest = typeof(ExampleGenericType<OperandType, string, OpCodeType>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryCreate");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public static ExampleGenericType<OperandType, string, OpCodeType>? TryCreate(OperandType property1, string? property2, OpCodeType property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public static Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>? Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>.TryCreate(System.Reflection.Emit.OperandType property1, string? property2, System.Reflection.Emit.OpCodeType property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics3()
		{
			// ExampleGenericType<OperandType, string, OpCodeType>.SomeInstanceMethodAsync(..)
			var genericMethodToTest = typeof(ExampleGenericType<OperandType, string, OpCodeType>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "SomeInstanceMethodAsync");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public virtual async Task<ExampleGenericType<OperandType, string, OpCodeType>?> SomeInstanceMethodAsync(OperandType property1, string? property2, OpCodeType property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public virtual async System.Threading.Tasks.Task<Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>?> Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>.SomeInstanceMethodAsync(System.Reflection.Emit.OperandType property1, string? property2, System.Reflection.Emit.OpCodeType property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics4()
		{
			// ClosedGenericType.TryCreate(..)
			var genericMethodToTest = typeof(ClosedGenericType).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryCreate");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public static ClosedGenericType? TryCreate(DateTimeKind property1, string property2, DateTime property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public static Rhinobyte.ReflectionHelpers.UnitTests.Setup.ClosedGenericType? Rhinobyte.ReflectionHelpers.UnitTests.Setup.ClosedGenericType.TryCreate(System.DateTimeKind property1, string property2, System.DateTime property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics5()
		{
			// ClosedGenericType.SomeInstanceMethodAsync(..)
			var genericMethodToTest = typeof(ClosedGenericType).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "SomeInstanceMethodAsync");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public virtual async Task<ExampleGenericType<DateTimeKind, string, DateTime>?> SomeInstanceMethodAsync(DateTimeKind property1, string? property2, DateTime property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public virtual async System.Threading.Tasks.Task<Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleGenericType<System.DateTimeKind, string, System.DateTime>?> Rhinobyte.ReflectionHelpers.UnitTests.Setup.ExampleGenericType<System.DateTimeKind, string, System.DateTime>.SomeInstanceMethodAsync(System.DateTimeKind property1, string? property2, System.DateTime property3)");
		}

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



		[TestMethod]
		public void IsOverride_returns_false_for_overriden_methods()
		{
			var methodToTest = typeof(ISomething).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeFalse();

			methodToTest = typeof(SomethingImplementation).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeFalse();

			methodToTest = typeof(AbstractSomething).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeFalse();

			methodToTest = typeof(BaseClass).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeFalse();

			methodToTest = typeof(ChildClass).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeFalse();
		}

		[TestMethod]
		public void IsOverride_returns_true_for_overriden_methods()
		{
			var methodToTest = typeof(SomethingSubclass).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeTrue();

			methodToTest = typeof(GrandChildClass).GetMethod("GetSomething");
			methodToTest.Should().NotBeNull();
			methodToTest!.IsOverride().Should().BeTrue();
		}
	}
}
