using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
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
		public TestContext TestContext { get; set; } = null!;

		[TestMethod]
		public void ContainsReferencesToAll_finds_the_references_in_an_async_method()
		{
			var fieldThatIsReferenced1 = typeof(AsyncContainsReferenceTestClass).GetField("_field1", BindingFlags.NonPublic | BindingFlags.Instance);
			fieldThatIsReferenced1.Should().NotBeNull();
			var fieldThatIsReferenced2 = typeof(AsyncContainsReferenceTestClass).GetField("_field2", BindingFlags.NonPublic | BindingFlags.Instance);
			fieldThatIsReferenced2.Should().NotBeNull();
			var fieldThatIsNotReferenced = typeof(AsyncContainsReferenceTestClass).GetField("_field3", BindingFlags.Public | BindingFlags.Instance);
			fieldThatIsNotReferenced.Should().NotBeNull();

			var methodThatIsReferenced = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsReferenced));
			methodThatIsReferenced.Should().NotBeNull();
			var methodThatIsNotReferenced1 = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsNotReferenced1));
			methodThatIsNotReferenced1.Should().NotBeNull();
			var methodThatIsNotReferenced2 = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsNotReferenced1));
			methodThatIsNotReferenced2.Should().NotBeNull();

			var propertyThatIsReferenced1 = typeof(AsyncContainsReferenceTestClass).GetProperty(nameof(AsyncContainsReferenceTestClass.Property1));
			propertyThatIsReferenced1.Should().NotBeNull();
			var propertyThatIsReferenced2 = typeof(AsyncContainsReferenceTestClass).GetProperty(nameof(AsyncContainsReferenceTestClass.Property2));
			propertyThatIsReferenced2.Should().NotBeNull();
			var propertyThatIsNotReferenced = typeof(AsyncContainsReferenceTestClass).GetProperty(nameof(AsyncContainsReferenceTestClass.Property3));
			propertyThatIsNotReferenced.Should().NotBeNull();

			var asyncMethodToCheck = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.DoSomethingAsync));

			// No references match should return false
			var noMatchingReferences = new MemberInfo[] { fieldThatIsNotReferenced!, methodThatIsNotReferenced1!, methodThatIsNotReferenced2!, propertyThatIsNotReferenced! };
			asyncMethodToCheck!.ContainsReferencesToAll(noMatchingReferences).Should().BeFalse();

			// Mix of matching and non-matching references should return false
			var mixedReferences = new MemberInfo[] { fieldThatIsReferenced1!, fieldThatIsNotReferenced!, methodThatIsReferenced!, propertyThatIsReferenced1! };
			asyncMethodToCheck!.ContainsReferencesToAll(mixedReferences).Should().BeFalse();

#if IS_RELEASE_TESTING_BUILD
			var reflectionExtensionsDebuggableAttribute = typeof(MethodBaseExtensions).Assembly.GetCustomAttribute<System.Diagnostics.DebuggableAttribute>();
			var isOptimized = reflectionExtensionsDebuggableAttribute?.IsJITOptimizerDisabled == false;
			if (isOptimized)
				Console.WriteLine($"Is Optimized: {isOptimized}");

			var thisAssemblyDebuggableAttribute = typeof(AsyncContainsReferenceTestClass).Assembly.GetCustomAttribute<System.Diagnostics.DebuggableAttribute>();
			var isOptimized2 = thisAssemblyDebuggableAttribute?.IsJITOptimizerDisabled == false;
			if (isOptimized2)
				Console.WriteLine($"Is Optimized2: {isOptimized2}");

			// Matching references (fields only, the method liked gets inlined / optimized away in RELEASE_TESTING builds
			var successfulMatchReferences1 = new MemberInfo[] { fieldThatIsReferenced1!, methodThatIsReferenced! };
			asyncMethodToCheck!.ContainsReferencesToAll(successfulMatchReferences1).Should().BeTrue();
#else
			// Matching references
			var successfulMatchReferences1 = new MemberInfo[] { fieldThatIsReferenced1!, methodThatIsReferenced! };
			asyncMethodToCheck!.ContainsReferencesToAll(successfulMatchReferences1).Should().BeTrue();
#endif

#if IS_RELEASE_TESTING_BUILD
			var successfulMatchReferences2 = new MemberInfo[] { fieldThatIsReferenced1!, fieldThatIsReferenced2!, propertyThatIsReferenced1!, propertyThatIsReferenced2! };
			asyncMethodToCheck!.ContainsReferencesToAll(successfulMatchReferences2).Should().BeTrue();
#else
			var successfulMatchReferences2 = new MemberInfo[] { fieldThatIsReferenced1!, fieldThatIsReferenced2!, methodThatIsReferenced!, propertyThatIsReferenced1!, propertyThatIsReferenced2! };
			asyncMethodToCheck!.ContainsReferencesToAll(successfulMatchReferences2).Should().BeTrue();
#endif
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

			// Test the method overload that directly takes the IMemberReferenceMatchInfo objects
			methodToSearch2!.ContainsReferencesToAll(membersToLookFor1.Select(member => new MemberReferenceMatchInfo(member, false, false))).Should().BeTrue(); // bool and char, true
		}

		[TestMethod]
		public void ContainsReferencesToAll_throw_ArgmentNullException_for_the_required_parameters()
		{
			Invoking(() => MethodBaseExtensions.ContainsReferencesToAll(null!, memberReferencesToLookFor: Array.Empty<MemberInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			Invoking(() => MethodBaseExtensions.ContainsReferencesToAll(null!, memberReferencesMatchInfoToLookFor: Array.Empty<IMemberReferenceMatchInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.ContainsReferencesToAll(methodToSearch1!, memberReferencesToLookFor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferencesToLookFor*");

			Invoking(() => MethodBaseExtensions.ContainsReferencesToAll(methodToSearch1!, memberReferencesMatchInfoToLookFor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferencesMatchInfoToLookFor*");
		}

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
		public void ContainsReferenceTo_finds_the_reference_in_an_async_method()
		{
			var methodThatIsReferenced = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsReferenced));
			var methodThatIsNotReferenced = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsNotReferenced1));
			var asyncMethodToCheck = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.DoSomethingAsync));

			asyncMethodToCheck!.ContainsReferenceTo(methodThatIsNotReferenced!).Should().BeFalse();

			asyncMethodToCheck!.ContainsReferenceTo(methodThatIsReferenced!).Should().BeTrue();
		}

		[TestMethod]
		public void ContainsReferenceTo_returns_the_expected_result()
		{
			var memberToLookFor = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			memberToLookFor.Should().NotBeNull();


			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14), BindingFlags.Public | BindingFlags.Static);
			methodToSearch1.Should().NotBeNull();
			methodToSearch1!.ContainsReferenceTo(memberToLookFor!).Should().BeTrue();
			methodToSearch1!.ContainsReferenceTo(new MemberReferenceMatchInfo(memberToLookFor!, false, false)).Should().BeTrue();


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

			Invoking(() => MethodBaseExtensions.ContainsReferenceTo(null!, memberReferenceToLookFor: consoleWriteMethods.First()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			Invoking(() => MethodBaseExtensions.ContainsReferenceTo(null!, memberReferenceMatchInfoToLookFor: new MemberReferenceMatchInfo(consoleWriteMethods.First(), false, false)))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.ContainsReferenceTo(methodToSearch1!, memberReferenceToLookFor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferenceToLookFor*");

			Invoking(() => MethodBaseExtensions.ContainsReferenceTo(methodToSearch1!, memberReferenceMatchInfoToLookFor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferenceMatchInfoToLookFor*");
		}

		[TestMethod]
		public void ContainsReferencesToAny_finds_the_references_in_an_async_method()
		{
			var fieldThatIsReferenced1 = typeof(AsyncContainsReferenceTestClass).GetField("_field1", BindingFlags.NonPublic | BindingFlags.Instance);
			fieldThatIsReferenced1.Should().NotBeNull();
			var fieldThatIsReferenced2 = typeof(AsyncContainsReferenceTestClass).GetField("_field2", BindingFlags.NonPublic | BindingFlags.Instance);
			fieldThatIsReferenced2.Should().NotBeNull();
			var fieldThatIsNotReferenced = typeof(AsyncContainsReferenceTestClass).GetField("_field3", BindingFlags.Public | BindingFlags.Instance);
			fieldThatIsNotReferenced.Should().NotBeNull();

			var methodThatIsReferenced = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsReferenced));
			methodThatIsReferenced.Should().NotBeNull();
			var methodThatIsNotReferenced1 = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsNotReferenced1));
			methodThatIsNotReferenced1.Should().NotBeNull();
			var methodThatIsNotReferenced2 = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.MethodThatIsNotReferenced1));
			methodThatIsNotReferenced2.Should().NotBeNull();

			var propertyThatIsReferenced1 = typeof(AsyncContainsReferenceTestClass).GetProperty(nameof(AsyncContainsReferenceTestClass.Property1));
			propertyThatIsReferenced1.Should().NotBeNull();
			var propertyThatIsReferenced2 = typeof(AsyncContainsReferenceTestClass).GetProperty(nameof(AsyncContainsReferenceTestClass.Property2));
			propertyThatIsReferenced2.Should().NotBeNull();
			var propertyThatIsNotReferenced = typeof(AsyncContainsReferenceTestClass).GetProperty(nameof(AsyncContainsReferenceTestClass.Property3));
			propertyThatIsNotReferenced.Should().NotBeNull();

			var asyncMethodToCheck = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.DoSomethingAsync));

			// No references match should return false
			var noMatchingReferences1 = new MemberInfo[] { fieldThatIsNotReferenced!, methodThatIsNotReferenced1!, methodThatIsNotReferenced2!, propertyThatIsNotReferenced! };
			asyncMethodToCheck!.ContainsReferenceToAny(noMatchingReferences1).Should().BeFalse();

			var noMatchingReferences2 = new MemberInfo[] { fieldThatIsNotReferenced! };
			asyncMethodToCheck!.ContainsReferenceToAny(noMatchingReferences2).Should().BeFalse();

			var noMatchingReferences3 = new MemberInfo[] { methodThatIsNotReferenced2!, propertyThatIsNotReferenced! };
			asyncMethodToCheck!.ContainsReferenceToAny(noMatchingReferences3).Should().BeFalse();


			// Mix of matching and non-matching references should return true
			var mixedReferences = new MemberInfo[] { fieldThatIsReferenced1!, fieldThatIsNotReferenced!, methodThatIsReferenced!, propertyThatIsReferenced1! };
			asyncMethodToCheck!.ContainsReferenceToAny(mixedReferences).Should().BeTrue();

			// Matching references
			var successfulMatchReferences1 = new MemberInfo[] { fieldThatIsReferenced1!, methodThatIsReferenced! };
			asyncMethodToCheck!.ContainsReferenceToAny(successfulMatchReferences1).Should().BeTrue();

			var successfulMatchReferences2 = new MemberInfo[] { fieldThatIsReferenced1!, fieldThatIsReferenced2!, methodThatIsReferenced!, propertyThatIsReferenced1!, propertyThatIsReferenced2! };
			asyncMethodToCheck!.ContainsReferenceToAny(successfulMatchReferences2).Should().BeTrue();
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

			// Test the method overload that directly takes the IMemberReferenceMatchInfo objects
			methodToSearch2!.ContainsReferenceToAny(membersToLookFor1.Select(member => new MemberReferenceMatchInfo(member, false, false))).Should().BeTrue();
		}

		[TestMethod]
		public void ContainsReferenceToAny_throw_ArgmentNullException_for_the_required_parameters()
		{
			Invoking(() => MethodBaseExtensions.ContainsReferenceToAny(null!, memberReferencesToLookFor: Array.Empty<MemberInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			Invoking(() => MethodBaseExtensions.ContainsReferenceToAny(null!, memberReferencesMatchInfoToLookFor: Array.Empty<IMemberReferenceMatchInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*methodBase*");

			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.ContainsReferenceToAny(methodToSearch1!, memberReferencesToLookFor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferencesToLookFor*");

			Invoking(() => MethodBaseExtensions.ContainsReferenceToAny(methodToSearch1!, memberReferencesMatchInfoToLookFor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*memberReferencesMatchInfoToLookFor*");
		}

		[TestMethod]
		public void DescribeInstructions_returns_the_expected_result1()
		{
			var methodToTest = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToTest.Should().NotBeNull();

			var methodBodyDescription = methodToTest!.DescribeInstructions();
#if IS_RELEASE_TESTING_BUILD
			methodBodyDescription.Should().Be(
@"(0) LOAD INT LITERAL (5)
(1) LOAD INT VALUE (Int8)  [SByte Value: 10]
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(4) ADD
(5) RETURN");
#else
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
#endif
		}

		[TestMethod]
		public void DescribeInstructions_returns_the_expected_result2()
		{
			var asyncMethodToCheck = typeof(AsyncContainsReferenceTestClass).GetMethod(nameof(AsyncContainsReferenceTestClass.DoSomethingAsync));
			var description = asyncMethodToCheck!.DescribeInstructions(new RecursiveInstructionFormatter(maxTraversalDepth: 3));

			description.Should().ContainAll(new[] { "START OF METHOD", "MIDDLE OF METHOD", "END OF METHOD" });
		}

		[TestMethod]
		public void DescribeInstructions_throws_ArgumentNullException_for_required_parameters_that_are_null()
		{
			Invoking(() => MethodBaseExtensions.DescribeInstructions(methodBase: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*methodBase*");
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

			var mockMethodBase = new MockMethodBase(null, "MockMethod");
			mockMethodBase.GetAccessLevel().Should().BeEmpty();
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

			var mockMethodBase = new MockMethodBase(null, "MockMethod");
			mockMethodBase.GetSignature().Should().Be("<ReturnType> MockMethod()");
		}

		[TestMethod]
		public void GetSignature_returns_the_expected_result_with_generics1()
		{
			// GenericStruct<>.TryGetSomething()
			var genericMethodToTest1 = typeof(GenericStruct<>).GetMethods()
				.FirstOrDefault(methodInfo => methodInfo.Name == "TryGetSomething");
			genericMethodToTest1.Should().NotBeNull();

			genericMethodToTest1!.ToString().Should().Be("System.Nullable`1[T] TryGetSomething()");

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

			var method4 = genericMethodToTest2!.MakeGenericMethod(typeof(ExampleLibrary1.ISomeOpenGenericType<string, string>), typeof(object));
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
		public void HasMatchingParameterNamesAndTypes_returns_the_expected_result1()
		{
			var methodToTest1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			var methodToTest2 = typeof(ExampleMethods)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Single(methodInfo => methodInfo.Name == nameof(ExampleMethods.OverloadedMethod) && methodInfo.GetParameters().Length == 3);

			methodToTest1!.HasMatchingParameterNamesAndTypes(methodToTest2).Should().BeFalse();

			var methodToTes1Parameters = methodToTest1!.GetParameters();
			methodToTest1!.HasMatchingParameterNamesAndTypes(methodToTes1Parameters!).Should().BeTrue();
		}

		[TestMethod]
		public void HasMatchingParameterNamesAndTypes_returns_the_expected_result2()
		{
			var methodToTest1 = typeof(ExampleMethods)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Single(methodInfo => methodInfo.Name == nameof(ExampleMethods.OverloadedMethod) && methodInfo.GetParameters().Length == 3);

			methodToTest1!.HasMatchingParameterNamesAndTypes(methodToTest1).Should().BeTrue();

			var testParameters1 = methodToTest1!.GetParameters();
			methodToTest1!.HasMatchingParameterNamesAndTypes(testParameters1).Should().BeTrue();

			var testParameters2 = new ParameterInfo[testParameters1.Length];
			testParameters2[0] = testParameters1[0];
			testParameters2[1] = testParameters1[1];

			var mockParameter1 = new Mock<ParameterInfo>();
			mockParameter1.Setup(x => x.ParameterType).Returns(typeof(IServiceProvider));
			mockParameter1.Setup(x => x.Name).Returns(testParameters1[2].Name);
			testParameters2[2] = mockParameter1.Object;
			methodToTest1!.HasMatchingParameterNamesAndTypes(testParameters2).Should().BeFalse();

			var testParameters3 = new ParameterInfo[testParameters1.Length];
			testParameters3[0] = testParameters1[0];

			var mockParameter2 = new Mock<ParameterInfo>();
			mockParameter1.Setup(x => x.ParameterType).Returns(testParameters1[1].ParameterType);
			mockParameter1.Setup(x => x.Name).Returns("mockParameterName");
			testParameters3[1] = mockParameter1.Object;

			testParameters3[2] = testParameters1[2];

			methodToTest1!.HasMatchingParameterNamesAndTypes(testParameters3).Should().BeFalse();
		}

		[TestMethod]
		public void HasMatchingParameterNamesAndTypes_throws_ArgumentNullException_for_the_required_arguments()
		{
			var methodBase = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			Invoking(() => MethodBaseExtensions.HasMatchingParameterNamesAndTypes(null!, methodToCompareTo: methodBase!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*methodBase*");

			Invoking(() => MethodBaseExtensions.HasMatchingParameterNamesAndTypes(null!, methodParametersToCompareTo: Array.Empty<ParameterInfo>()))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*methodBase*");

			Invoking(() => MethodBaseExtensions.HasMatchingParameterNamesAndTypes(methodBase!, methodToCompareTo: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*methodToCompareTo*");

			Invoking(() => MethodBaseExtensions.HasMatchingParameterNamesAndTypes(methodBase!, methodParametersToCompareTo: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*methodParametersToCompareTo*");
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

#if IS_RELEASE_TESTING_BUILD
			instructions.Count.Should().Be(6);
#else
			instructions.Count.Should().Be(12);
#endif
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
