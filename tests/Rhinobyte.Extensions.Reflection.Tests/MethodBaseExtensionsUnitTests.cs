using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests
{
	[TestClass]
	public class MethodBaseExtensionsUnitTests
	{

		[TestMethod]
		public void ContainsReferenceTo_correctly_differentiates_method_overloads()
		{
			var memberToLookFor = typeof(ExampleMethods)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Single(methodInfo => methodInfo.Name == nameof(ExampleMethods.OverloadedMethod) && methodInfo.GetParameters().Length == 3);

			memberToLookFor.Should().NotBeNull();


			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesOverloadedMethod1), BindingFlags.Public | BindingFlags.Static);
			methodToSearch1.Should().NotBeNull();
			methodToSearch1!.ContainsReferenceTo(memberToLookFor!).Should().BeFalse();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesOverloadedMethod2), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();
			methodToSearch2!.ContainsReferenceTo(memberToLookFor!).Should().BeFalse();


			var methodToSearch3 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesOverloadedMethod3), BindingFlags.Public | BindingFlags.Static);
			methodToSearch3.Should().NotBeNull();
			methodToSearch3!.ContainsReferenceTo(memberToLookFor!).Should().BeTrue();
		}

		[TestMethod]
		public void ContainsReferenceTo_returns_the_expected_result()
		{
			var memberToLookFor = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			memberToLookFor.Should().NotBeNull();


			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14), BindingFlags.Public | BindingFlags.Static);
			methodToSearch1.Should().NotBeNull();
			methodToSearch1!.ContainsReferenceTo(memberToLookFor!).Should().BeTrue();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14_Using_Delegate_Function), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();
			methodToSearch2!.ContainsReferenceTo(memberToLookFor!).Should().BeTrue();


			var methodToSearch3 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToSearch3.Should().NotBeNull();
			methodToSearch3!.ContainsReferenceTo(memberToLookFor!).Should().BeFalse();
		}

		[TestMethod]
		public void ContainsReferenceTo_throw_ArgmentNullException_for_the_required_parameters()
		{
			var consoleWriteMethods = typeof(System.Console)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(methodInfo => methodInfo.Name == nameof(System.Console.Write))
				.ToList();

			Invoking(() => MethodBaseExtensions.ContainsReferenceTo(null!, consoleWriteMethods.First()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.ContainsReferenceTo(methodToSearch1!, null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferenceToLookFor*");
		}

		[TestMethod]
		public void ContainsReferencesToAll_returns_the_expected_result()
		{
			var consoleWriteMethods = typeof(System.Console)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(methodInfo => methodInfo.Name == nameof(System.Console.Write))
				.ToList();

			var membersToLookFor1 = new List<MethodInfo>();
			var membersToLookFor2 = new List<MethodInfo>();
			var membersToLookFor3 = new List<MethodInfo>();
			foreach (var methodInfo in consoleWriteMethods)
			{
				var methodParameters = methodInfo.GetParameters();
				if (methodParameters.Length != 1)
				{
					continue;
				}

				var parameterType = methodParameters[0].ParameterType;
				if (parameterType == typeof(bool) || parameterType == typeof(char))
				{
					membersToLookFor1.Add(methodInfo);
					membersToLookFor2.Add(methodInfo);
					membersToLookFor3.Add(methodInfo);
					continue;
				}

				if (parameterType == typeof(int))
				{
					membersToLookFor2.Add(methodInfo);
					continue;
				}

				if (parameterType == typeof(string))
				{
					membersToLookFor3.Add(methodInfo);
				}
			}

			membersToLookFor1.Should().NotBeNull().And.HaveCount(2);
			membersToLookFor2.Should().NotBeNull().And.HaveCount(3);
			membersToLookFor3.Should().NotBeNull().And.HaveCount(3);


			// ExampleMethods.AddTwoValues should not contain references to any of the Console.Write methods
			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			methodToSearch1.Should().NotBeNull();
			methodToSearch1!.ContainsReferencesToAll(membersToLookFor1).Should().BeFalse();
			methodToSearch1!.ContainsReferencesToAll(membersToLookFor2).Should().BeFalse();
			methodToSearch1!.ContainsReferencesToAll(membersToLookFor3).Should().BeFalse();

			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesConsoleWriteMethods_Bool_Char_And_Int), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();

			methodToSearch2!.ContainsReferencesToAll(membersToLookFor1).Should().BeTrue(); // bool and char, true
			methodToSearch2!.ContainsReferencesToAll(membersToLookFor2).Should().BeTrue(); // bool and char and int, true
			methodToSearch2!.ContainsReferencesToAll(membersToLookFor3).Should().BeFalse(); // bool and char but not string, false
		}

		[TestMethod]
		public void ContainsMethodToAll_throw_ArgmentNullException_for_the_required_parameters()
		{
			Invoking(() => MethodBaseExtensions.ContainsReferencesToAll(null!, Array.Empty<MemberInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.ContainsReferencesToAll(methodToSearch1!, null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferencesToLookFor*");
		}

		[TestMethod]
		public void ContainsReferenceToAny_returns_the_expected_result()
		{
			var consoleWriteMethods = typeof(System.Console)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(methodInfo => methodInfo.Name == nameof(System.Console.Write))
				.ToList();

			var membersToLookFor1 = new List<MethodInfo>();
			var membersToLookFor2 = new List<MethodInfo>();
			foreach (var methodInfo in consoleWriteMethods)
			{
				var methodParameters = methodInfo.GetParameters();
				if (methodParameters.Length != 1)
				{
					continue;
				}

				var parameterType = methodParameters[0].ParameterType;
				if (parameterType == typeof(bool) || parameterType == typeof(float))
				{
					membersToLookFor1.Add(methodInfo);
					continue;
				}

				if (parameterType == typeof(string) || parameterType == typeof(decimal))
				{
					membersToLookFor2.Add(methodInfo);
				}
			}

			membersToLookFor1.Should().NotBeNull().And.HaveCount(2);
			membersToLookFor2.Should().NotBeNull().And.HaveCount(2);


			// ExampleMethods.AddTwoValues should not contain references to any of the Console.Write methods
			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			methodToSearch1.Should().NotBeNull();
			methodToSearch1!.ContainsReferenceToAny(membersToLookFor1).Should().BeFalse();
			methodToSearch1!.ContainsReferenceToAny(membersToLookFor2).Should().BeFalse();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesConsoleWriteMethods_Bool_Char_And_Int), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();

			methodToSearch2!.ContainsReferenceToAny(membersToLookFor1).Should().BeTrue();
			methodToSearch2!.ContainsReferenceToAny(membersToLookFor2).Should().BeFalse();
		}

		[TestMethod]
		public void ContainsMethodToAny_throw_ArgmentNullException_for_the_required_parameters()
		{
			Invoking(() => MethodBaseExtensions.ContainsReferenceToAny(null!, Array.Empty<MemberInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.ContainsReferenceToAny(methodToSearch1!, null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferencesToLookFor*");
		}

		[TestMethod]
		public void DescribeInstructions_returns_the_expected_result()
		{
			// Release/optimized build will have different IL causing our test expectations will fail
			var debuggableAttribute = typeof(ExampleMethods).Assembly.GetCustomAttribute<System.Diagnostics.DebuggableAttribute>();
			var assemblyIsDebugBuildWithoutOptimizations = debuggableAttribute?.IsJITOptimizerDisabled == true;
			assemblyIsDebugBuildWithoutOptimizations.Should().BeTrue();

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
		public void GetAccessLevel_returns_the_expected_result()
		{
			var methodToTest = typeof(ExampleAccessLevelMethods).GetMethod("DefaultAccessInstanceMethod", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();
			methodToTest!.GetAccessLevel().Should().Be("private");

			methodToTest = typeof(ExampleAccessLevelMethods).GetMethod("PrivateInstanceMethod", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();
			methodToTest!.GetAccessLevel().Should().Be("private");

			methodToTest = typeof(ExampleAccessLevelMethods).GetMethod("ProtectedInstanceMethod", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();
			methodToTest!.GetAccessLevel().Should().Be("protected");

			methodToTest = typeof(ExampleAccessLevelMethods).GetMethod(nameof(ExampleAccessLevelMethods.ProtectedInternalInstanceMethod), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();
			methodToTest!.GetAccessLevel().Should().Be("protected internal");

			methodToTest = typeof(ExampleAccessLevelMethods).GetMethod(nameof(ExampleAccessLevelMethods.InternalInstanceMethod), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();
			methodToTest!.GetAccessLevel().Should().Be("internal");

			methodToTest = typeof(ExampleAccessLevelMethods).GetMethod(nameof(ExampleAccessLevelMethods.PublicInstanceMethod), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();
			methodToTest!.GetAccessLevel().Should().Be("public");
		}

		[TestMethod]
		public void GetAccessLevel_throws_ArgumentNullException_for_a_null_method_base_argument()
		{
			Invoking(() => MethodBaseExtensions.GetAccessLevel(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_basic()
		{
			var methodToTest = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToTest.Should().NotBeNull();

			methodToTest!.GetSignature().Should().Be("public static int AddLocalVariables_For_5_And_15()");
			methodToTest!.GetSignature(true).Should().Be("public static int Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleMethods.AddLocalVariables_For_5_And_15()");

			methodToTest = typeof(AbstractSomething).GetMethod(nameof(AbstractSomething.GetSomething), BindingFlags.Public | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();

			methodToTest!.GetSignature().Should().Be("public abstract string GetSomething()");
			methodToTest!.GetSignature(true).Should().Be("public abstract string Rhinobyte.Extensions.Reflection.Tests.Setup.AbstractSomething.GetSomething()");

			methodToTest = typeof(BaseClass).GetMethod(nameof(BaseClass.GetSomething), BindingFlags.Public | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();

			methodToTest!.GetSignature().Should().Be("public virtual string GetSomething()");
			methodToTest!.GetSignature(true).Should().Be("public virtual string Rhinobyte.Extensions.Reflection.Tests.Setup.BaseClass.GetSomething()");

			methodToTest = typeof(SomethingSubclass).GetMethod(nameof(SomethingSubclass.GetSomething), BindingFlags.Public | BindingFlags.Instance);
			methodToTest.Should().NotBeNull();

			methodToTest!.GetSignature().Should().Be("public override string GetSomething()");
			methodToTest!.GetSignature(true).Should().Be("public override string Rhinobyte.Extensions.Reflection.Tests.Setup.SomethingSubclass.GetSomething()");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics1()
		{
			// GenericStruct<>.TryGetSomething()
			var genericMethodToTest1 = typeof(GenericStruct<>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryGetSomething");
			genericMethodToTest1.Should().NotBeNull();

			genericMethodToTest1!.GetSignature().Should().Be("public T? TryGetSomething()");
			genericMethodToTest1!.GetSignature(true).Should().Be("public T? Rhinobyte.Extensions.Reflection.Tests.Setup.GenericStruct<T>.TryGetSomething() where T : System.IConvertible, struct");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics2()
		{
			// ExampleGenericType<OperandType, string, OpCodeType>.TryCreate(..)
			var genericMethodToTest = typeof(ExampleGenericType<OperandType, string, OpCodeType>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryCreate");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public static ExampleGenericType<OperandType, string, OpCodeType>? TryCreate(OperandType property1, string? property2, OpCodeType property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public static Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>? Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>.TryCreate(System.Reflection.Emit.OperandType property1, string? property2, System.Reflection.Emit.OpCodeType property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics3()
		{
			// ExampleGenericType<OperandType, string, OpCodeType>.SomeInstanceMethodAsync(..)
			var genericMethodToTest = typeof(ExampleGenericType<OperandType, string, OpCodeType>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "SomeInstanceMethodAsync");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public virtual async Task<ExampleGenericType<OperandType, string, OpCodeType>?> SomeInstanceMethodAsync(OperandType property1, string? property2, OpCodeType property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public virtual async System.Threading.Tasks.Task<Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>?> Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleGenericType<System.Reflection.Emit.OperandType, string, System.Reflection.Emit.OpCodeType>.SomeInstanceMethodAsync(System.Reflection.Emit.OperandType property1, string? property2, System.Reflection.Emit.OpCodeType property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics4()
		{
			// ClosedGenericType.TryCreate(..)
			var genericMethodToTest = typeof(ClosedGenericType).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryCreate");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public static ClosedGenericType? TryCreate(DateTimeKind property1, string property2, DateTime property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public static Rhinobyte.Extensions.Reflection.Tests.Setup.ClosedGenericType? Rhinobyte.Extensions.Reflection.Tests.Setup.ClosedGenericType.TryCreate(System.DateTimeKind property1, string property2, System.DateTime property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics5()
		{
			// ClosedGenericType.SomeInstanceMethodAsync(..)
			var genericMethodToTest = typeof(ClosedGenericType).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "SomeInstanceMethodAsync");
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public virtual async Task<ExampleGenericType<DateTimeKind, string, DateTime>?> SomeInstanceMethodAsync(DateTimeKind property1, string? property2, DateTime property3)");
			genericMethodToTest!.GetSignature(true).Should().Be("public virtual async System.Threading.Tasks.Task<Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleGenericType<System.DateTimeKind, string, System.DateTime>?> Rhinobyte.Extensions.Reflection.Tests.Setup.ExampleGenericType<System.DateTimeKind, string, System.DateTime>.SomeInstanceMethodAsync(System.DateTimeKind property1, string? property2, System.DateTime property3)");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics6()
		{
			// ClosedGenericType.SomeInstanceMethodAsync(..)
			var genericMethodToTest = typeof(ExampleLibrary1.ClassWithGenericMethod).GetMethod(nameof(ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult3), BindingFlags.Public | BindingFlags.Instance);
			genericMethodToTest.Should().NotBeNull();

			genericMethodToTest!.GetSignature().Should().Be("public ISomeNullableOpenGenericTypeTwo<TOne, TTwo>? MethodWithGenericResult3<TOne?, TTwo?>(TOne? paramOne, TTwo? paramTwo)");
			genericMethodToTest!.GetSignature(true).Should().Be("public ISomeNullableOpenGenericTypeTwo<TOne, TTwo>? ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult3<TOne?, TTwo?>(TOne? paramOne, TTwo? paramTwo)");

			var method2 = genericMethodToTest!.MakeGenericMethod(typeof(object), typeof(object));
			method2!.GetSignature().Should().Be("public ISomeNullableOpenGenericTypeTwo<object, object>? MethodWithGenericResult3<object, object>(object? paramOne, object? paramTwo)");
			method2!.GetSignature(true).Should().Be("public ExampleLibrary1.ISomeNullableOpenGenericTypeTwo<object, object>? ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult3<object, object>(object? paramOne, object? paramTwo)");


			var genericMethodToTest2 = typeof(ExampleLibrary1.ClassWithGenericMethod).GetMethod(nameof(ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult4), BindingFlags.Public | BindingFlags.Instance);
			genericMethodToTest2!.GetSignature().Should().Be("public ValueTuple<TOne, TTwo> MethodWithGenericResult4<TOne, TTwo>(TOne paramOne, TTwo paramTwo)");
			genericMethodToTest2!.GetSignature(true).Should().Be("public ValueTuple<TOne, TTwo> ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult4<TOne, TTwo>(TOne paramOne, TTwo paramTwo)");

			var method3 = genericMethodToTest2!.MakeGenericMethod(typeof(ExampleLibrary1.ISomeOpenGenericType<,>), typeof(object));
			method3!.GetSignature().Should().Be("public ValueTuple<ISomeOpenGenericType<TOne, TTwo>, object> MethodWithGenericResult4<ISomeOpenGenericType<TOne, TTwo>, object>(ISomeOpenGenericType<TOne, TTwo> paramOne, object paramTwo)");
			method3!.GetSignature(true).Should().Be("public ValueTuple<ExampleLibrary1.ISomeOpenGenericType<TOne, TTwo>, object> ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult4<ExampleLibrary1.ISomeOpenGenericType<TOne, TTwo>, object>(ExampleLibrary1.ISomeOpenGenericType<TOne, TTwo> paramOne, object paramTwo)");

			var method4 = genericMethodToTest2!.MakeGenericMethod(typeof(ExampleLibrary1.ISomeOpenGenericType<string,string>), typeof(object));
			method4!.GetSignature().Should().Be("public ValueTuple<ISomeOpenGenericType<string, string>, object> MethodWithGenericResult4<ISomeOpenGenericType<string, string>, object>(ISomeOpenGenericType<string, string> paramOne, object paramTwo)");
			method4!.GetSignature(true).Should().Be("public System.ValueTuple<ExampleLibrary1.ISomeOpenGenericType<string, string>, object> ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult4<ExampleLibrary1.ISomeOpenGenericType<string, string>, object>(ExampleLibrary1.ISomeOpenGenericType<string, string> paramOne, object paramTwo)");


			var genericMethodToTest3 = typeof(ExampleLibrary1.ClassWithGenericMethod).GetMethod(nameof(ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericConstraints), BindingFlags.Public | BindingFlags.Instance);
			genericMethodToTest3!.GetSignature().Should().Be(
@"public TSomething MethodWithGenericConstraints<TSomething, TSomethingElse>(TSomething something, TSomethingElse somethingElse)
where TSomething : Enum
where TSomethingElse : Enum");

			genericMethodToTest3!.GetSignature(true).Should().Be(
@"public TSomething ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericConstraints<TSomething, TSomethingElse>(TSomething something, TSomethingElse somethingElse)
where TSomething : System.Enum
where TSomethingElse : System.Enum");
		}

		[TestMethod]
		public void GetSignature_throws_ArgumentNullException_for_a_null_method_base_argument()
		{
			Invoking(() => MethodBaseExtensions.GetSignature(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");
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
		public void HasNoParameterTypes_throws_ArgumentNullException_for_null_arguments()
		{
			Invoking(() => MethodBaseExtensions.HasMatchingParameterTypes(null!, Array.Empty<Type>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodInfo = typeof(MethodBaseExtensionsUnitTests).GetMethod(nameof(HasNoParameterTypes_throws_ArgumentNullException_for_null_arguments), BindingFlags.Public | BindingFlags.Instance);
			Invoking(() => MethodBaseExtensions.HasMatchingParameterTypes(methodInfo!, null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*parameterTypes*");
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
		public void HasNoParameters_throws_ArgumentNullException_for_a_null_method_base_argument()
		{
			Invoking(() => MethodBaseExtensions.HasNoParameters(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");
		}

		[TestMethod]
		public void IsAsync_throws_ArgumentNullException_for_a_null_method_base_argument()
		{
			Invoking(() => MethodBaseExtensions.IsAsync(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");
		}

		[TestMethod]
		public void IsOverride_returns_false_for_non_overriden_methods()
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

		[TestMethod]
		public void IsOverride_throws_ArgumentNullException_for_a_null_method_info_argument()
		{
			Invoking(() => MethodBaseExtensions.IsOverride(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodInfo*");
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result()
		{
			var methodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodInfo.Should().NotBeNull();

			var instructions = methodInfo!.ParseInstructions();
			instructions.Count.Should().Be(12);
		}

		[TestMethod]
		public void ParseInstructions_throws_ArgumentNullException_for_a_null_method_base_parameter()
		{
			Invoking(() => MethodBaseExtensions.ParseInstructions(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");
		}
	}
}
