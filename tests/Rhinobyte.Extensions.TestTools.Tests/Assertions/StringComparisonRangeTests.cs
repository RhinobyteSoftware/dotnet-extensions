using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.TestTools.Assertions;

namespace Rhinobyte.Extensions.TestTools.Tests.Assertions
{
	[TestClass]
	public class StringComparisonRangeTests
	{
		[TestMethod]
		public void Equals_returns_the_expected_result1()
		{
			var stringComparisonRange1 = new StringComparisonRange(true, 0, 2, 0, 2);
			stringComparisonRange1.Equals(obj: "Test").Should().BeFalse();

#pragma warning disable CA1508 // Avoid dead conditional code
			stringComparisonRange1.Equals(obj: null).Should().BeFalse();
			stringComparisonRange1!.Equals(other: null).Should().BeFalse();
#pragma warning restore CA1508 // Avoid dead conditional code
		}

		[TestMethod]
		public void Equals_returns_the_expected_result2()
		{
			var stringComparisonRange1 = new StringComparisonRange(true, 0, 2, 0, 2);
			stringComparisonRange1.Equals(stringComparisonRange1).Should().BeTrue();

			var stringComparisonRange2 = new StringComparisonRange(true, 0, 2, 0, 2);
			stringComparisonRange1.Equals(stringComparisonRange2).Should().BeTrue();

			var stringComparisonRange3 = new StringComparisonRange(false, 0, 2, 0, 2);
			stringComparisonRange1.Equals(stringComparisonRange3).Should().BeFalse();

			var stringComparisonRange4 = new StringComparisonRange(true, 0, 2, 2, 0);
			stringComparisonRange1.Equals(stringComparisonRange4).Should().BeFalse();

			var stringComparisonRange5 = new StringComparisonRange(false, 2, 2, 0, 2);
			stringComparisonRange1.Equals(stringComparisonRange5).Should().BeFalse();
		}

		[TestMethod]
		public void GetHashCode_does_not_match_when_values_are_different()
		{
			var hash1 = new StringComparisonRange(true, 10, 12, 5, 7).GetHashCode();

			hash1.Should().NotBe(new StringComparisonRange(false, 10, 12, 5, 7).GetHashCode());
			hash1.Should().NotBe(new StringComparisonRange(true, 9, 12, 5, 7).GetHashCode());
			hash1.Should().NotBe(new StringComparisonRange(true, 10, 13, 5, 7).GetHashCode());
			hash1.Should().NotBe(new StringComparisonRange(true, 10, 12, 6, 7).GetHashCode());
			hash1.Should().NotBe(new StringComparisonRange(true, 10, 12, 5, -7).GetHashCode());
		}

		[TestMethod]
		public void GetHashCode_matches_when_values_are_the_same()
		{
			var stringComparisonRange1 = new StringComparisonRange(true, 0, 2, 0, 2);
			stringComparisonRange1.GetHashCode().Should().Be(stringComparisonRange1.GetHashCode());

			var stringComparisonRange2 = new StringComparisonRange(true, 0, 2, 0, 2);
			stringComparisonRange1.GetHashCode().Should().Be(stringComparisonRange2.GetHashCode());
		}

		[TestMethod]
		public void ToString_returns_the_expected_result()
		{
			new StringComparisonRange(true, 0, 2, 0, 2).ToString()
				.Should().Be("IsMatch: True   Source: [0, 2)   Target: [0, 2)");
		}
	}
}
