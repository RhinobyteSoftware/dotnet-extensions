using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.UnitTests.Setup;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.UnitTests
{
	[TestClass]
	public class MethodBodyParserUnitTests
	{

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
			var methodBodyParser1 = new MethodBodyParser(methodToSearch1!);
			methodBodyParser1.ContainsReferencesToAll(membersToLookFor1).Should().BeFalse();
			methodBodyParser1.ContainsReferencesToAll(membersToLookFor2).Should().BeFalse();
			methodBodyParser1.ContainsReferencesToAll(membersToLookFor3).Should().BeFalse();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesConsoleWriteMethods_Bool_Char_And_Int), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();

			var methodBodyParser2 = new MethodBodyParser(methodToSearch2!);
			methodBodyParser2.ContainsReferencesToAll(membersToLookFor1).Should().BeTrue(); // bool and char, true
			methodBodyParser2.ContainsReferencesToAll(membersToLookFor2).Should().BeTrue(); // bool and char and int, true
			methodBodyParser2.ContainsReferencesToAll(membersToLookFor3).Should().BeFalse(); // bool and char but not string, false
		}

		[TestMethod]
		public void ContainsReferenceTo_returns_the_expected_result()
		{
			var memberToLookFor = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues), BindingFlags.Public | BindingFlags.Static);
			memberToLookFor.Should().NotBeNull();


			var methodToSearch1 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14), BindingFlags.Public | BindingFlags.Static);
			methodToSearch1.Should().NotBeNull();
			new MethodBodyParser(methodToSearch1!).ContainsReferenceTo(memberToLookFor!).Should().BeTrue();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14_Using_Delegate_Function), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();
			new MethodBodyParser(methodToSearch2!).ContainsReferenceTo(memberToLookFor!).Should().BeTrue();


			var methodToSearch3 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodToSearch3.Should().NotBeNull();
			new MethodBodyParser(methodToSearch3!).ContainsReferenceTo(memberToLookFor!).Should().BeFalse();
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
			new MethodBodyParser(methodToSearch1!).ContainsReferenceTo(memberToLookFor!).Should().BeFalse();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesOverloadedMethod2), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();
			new MethodBodyParser(methodToSearch2!).ContainsReferenceTo(memberToLookFor!).Should().BeFalse();


			var methodToSearch3 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesOverloadedMethod3), BindingFlags.Public | BindingFlags.Static);
			methodToSearch3.Should().NotBeNull();
			new MethodBodyParser(methodToSearch3!).ContainsReferenceTo(memberToLookFor!).Should().BeTrue();
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
			var methodBodyParser1 = new MethodBodyParser(methodToSearch1!);
			methodBodyParser1.ContainsReferenceToAny(membersToLookFor1).Should().BeFalse();
			methodBodyParser1.ContainsReferenceToAny(membersToLookFor2).Should().BeFalse();


			var methodToSearch2 = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.UsesConsoleWriteMethods_Bool_Char_And_Int), BindingFlags.Public | BindingFlags.Static);
			methodToSearch2.Should().NotBeNull();

			var methodBodyParser2 = new MethodBodyParser(methodToSearch2!);
			methodBodyParser2.ContainsReferenceToAny(membersToLookFor1).Should().BeTrue();
			methodBodyParser2.ContainsReferenceToAny(membersToLookFor2).Should().BeFalse();
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result1()
		{
			var methodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(methodInfo!).ParseInstructions();
			instructions.Count.Should().Be(12);

			//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			//results.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result2()
		{
			var nullCheckMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.NullParameterCheck_Type1), BindingFlags.Public | BindingFlags.Static);
			nullCheckMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(nullCheckMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(15);

			//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			//results.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result3()
		{
			var nullCheckMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.NullParameterCheck_Type2), BindingFlags.Public | BindingFlags.Static);
			nullCheckMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(nullCheckMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(11);

			//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			//results.Should().NotBeNullOrEmpty();
		}
	}
}
