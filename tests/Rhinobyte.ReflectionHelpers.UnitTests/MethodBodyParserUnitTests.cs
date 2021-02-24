using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.ReflectionHelpers.UnitTests
{
	[TestClass]
	public class MethodBodyParserUnitTests
	{
		[TestMethod]
		public void ParseInstructions_returns_the_expected_result()
		{
			var methodInfo = typeof(MethodBodyParserUnitTests).GetMethod(nameof(MethodToParse), BindingFlags.Public | BindingFlags.Static);
			methodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(methodInfo!).ParseInstructions();
			instructions.Count.Should().Be(12);

			var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			results.Should().NotBeNullOrEmpty();
		}

		public static int MethodToParse()
		{
			var value1 = 5;
			var value2 = 10;
			return value1 + value2;
		}
	}
}
