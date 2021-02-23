using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		}

		public static int MethodToParse()
		{
			var value1 = 5;
			var value2 = 10;
			return value1 + value2;
		}
	}
}
