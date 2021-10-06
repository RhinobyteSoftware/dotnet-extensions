using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.TestTools.Assertions;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.TestTools.Tests.Assertions
{
	[TestClass]
	public class StringAssertionExtensionsTests
	{
		[TestMethod]
		public void ShouldBeSameAs_passes_when_the_values_match()
		{
			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(null, null)).Should().NotThrow();
			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(string.Empty, string.Empty)).Should().NotThrow();
			Invoking(() => StringAssertionExtensions.ShouldBeSameAs("    ", "    ")).Should().NotThrow();
			Invoking(() => StringAssertionExtensions.ShouldBeSameAs("Test", "Test")).Should().NotThrow();

			Invoking(() => StringAssertionExtensions
					.ShouldBeSameAs("Test ", "Test  \t", whitespaceNormalizationType: WhitespaceNormalizationType.TrimTrailingWhitespace)
				)
				.Should().NotThrow();
		}

		[TestMethod]
		public void ShouldBeSameAs_throws_when_the_values_do_not_match()
		{
			var testString1 = "Test1";
			var testString2 = "Test2";

			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(null, testString1))
				.Should()
				.Throw<AssertFailedException>();

			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(testString2, null))
				.Should()
				.Throw<AssertFailedException>();

			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(string.Empty, testString1))
				.Should()
				.Throw<AssertFailedException>();

			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(testString2, string.Empty))
				.Should()
				.Throw<AssertFailedException>();

			Invoking(() => StringAssertionExtensions.ShouldBeSameAs(testString1, testString2))
				.Should()
				.Throw<AssertFailedException>();
		}
	}
}
