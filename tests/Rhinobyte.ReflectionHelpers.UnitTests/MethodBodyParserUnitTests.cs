using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.ReflectionHelpers.UnitTests.Setup;
using System.Reflection;

namespace Rhinobyte.ReflectionHelpers.UnitTests
{
	[TestClass]
	public class MethodBodyParserUnitTests
	{
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
