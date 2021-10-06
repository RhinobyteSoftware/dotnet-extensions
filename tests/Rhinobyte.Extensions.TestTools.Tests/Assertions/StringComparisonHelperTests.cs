using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.TestTools.Assertions;
using System;
using System.Text;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.TestTools.Tests.Assertions
{
	[TestClass]
	public class StringComparisonHelperTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void AppendNewlineIfNecessary_behaves_as_expected()
		{
			Invoking(() => StringComparisonHelper.AppendNewlineIfNecessary(null!, false))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*stringBuilder*");

			var stringBuilder = new StringBuilder();
			StringComparisonHelper.AppendNewlineIfNecessary(stringBuilder, false);
			stringBuilder.ToString().Should().Be(Environment.NewLine);

			_ = stringBuilder.Clear();
			StringComparisonHelper.AppendNewlineIfNecessary(stringBuilder, true);
			stringBuilder.ToString().Should().Be(Environment.NewLine);

			_ = stringBuilder.Clear().Append('\r');
			StringComparisonHelper.AppendNewlineIfNecessary(stringBuilder, false);
			stringBuilder.ToString().Should().Be($"\r{Environment.NewLine}");

			_ = stringBuilder.Clear().Append('\r');
			StringComparisonHelper.AppendNewlineIfNecessary(stringBuilder, true);
			stringBuilder.ToString().Should().Be("\r\n");

			_ = stringBuilder.Clear().Append('\n');
			StringComparisonHelper.AppendNewlineIfNecessary(stringBuilder, false);
			stringBuilder.ToString().Should().Be("\n");

			_ = stringBuilder.Clear().Append('\n');
			StringComparisonHelper.AppendNewlineIfNecessary(stringBuilder, true);
			stringBuilder.ToString().Should().Be("\n");
		}

		[TestMethod]
		public void CompareLinesTo_behaves_as_expected_for_different_WhitespaceNormalizationTypes()
		{
			var isMatchRange = new StringComparisonRange(true, 0, 3, 0, 3);
			var isNotAMatchRange = new StringComparisonRange(false, 0, 3, 0, 3);

			var sourceString = "Line1\r\nLine2\nLine3\r";
			var targetString = "Line1\nLine2\r\nLine3\r\r";

			var comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.None);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isNotAMatchRange });

			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.RemoveCarriageReturns);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isMatchRange });

			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.TrimTrailingWhitespace);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isMatchRange });

			sourceString = "Line1\n\rLine2\nLi\rne3";
			targetString = "\rLine1\nLine2\nLine3";

			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.None);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isNotAMatchRange });

			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.RemoveCarriageReturns);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isMatchRange });

			// TrimTrailingWhitespace should not match in this case because there are \r characters not at the end of the line
			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.TrimTrailingWhitespace);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isNotAMatchRange });

			sourceString = "Line1   \r\nLine2\nLine3    ";
			targetString = "Line1\nLine2\t\r\nLine3\t";

			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.None);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isNotAMatchRange });

			// RemoveCarriageReturns should not match in this case because there are trailing space characters
			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.RemoveCarriageReturns);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isNotAMatchRange });

			comparisonResult = StringComparisonHelper.CompareLinesTo(sourceString, targetString, WhitespaceNormalizationType.TrimTrailingWhitespace);
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[] { isMatchRange });
		}

		[DataTestMethod]
		[DataRow(WhitespaceNormalizationType.None)]
		[DataRow(WhitespaceNormalizationType.RemoveCarriageReturns)]
		[DataRow(WhitespaceNormalizationType.TrimTrailingWhitespace)]
		[DataRow(WhitespaceNormalizationType.TrimLeadingAndTrailingWhitespace)]
		public void CompareLinesTo_doesnt_blow_up_on_nulls_or_empty_arrays(WhitespaceNormalizationType whitespaceNormalizationType)
		{
			StringComparisonHelper.CompareLinesTo(source: null!, target: "Target String", whitespaceNormalizationType).Should().BeNull();
			StringComparisonHelper.CompareLinesTo(source: "Source String", target: null!, whitespaceNormalizationType).Should().BeNull();
			StringComparisonHelper.CompareLinesTo(source: null!, target: null!, whitespaceNormalizationType).Should().BeNull();

			var normalLines = new[] { "Some Line" };
			StringComparisonHelper.CompareLinesTo(sourceLines: null!, targetLines: normalLines, whitespaceNormalizationType).Should().BeNull();
			StringComparisonHelper.CompareLinesTo(sourceLines: normalLines, targetLines: null!, whitespaceNormalizationType).Should().BeNull();
			StringComparisonHelper.CompareLinesTo(sourceLines: null!, targetLines: null!, whitespaceNormalizationType).Should().BeNull();

			var emptyLinesArray = Array.Empty<string>();
			StringComparisonHelper.CompareLinesTo(sourceLines: emptyLinesArray, targetLines: normalLines, whitespaceNormalizationType).Should().BeNull();
			StringComparisonHelper.CompareLinesTo(sourceLines: normalLines, targetLines: emptyLinesArray, whitespaceNormalizationType).Should().BeNull();
			StringComparisonHelper.CompareLinesTo(sourceLines: emptyLinesArray, targetLines: emptyLinesArray, whitespaceNormalizationType).Should().BeNull();
		}

		[DataTestMethod]
		[DataRow(WhitespaceNormalizationType.None)]
		[DataRow(WhitespaceNormalizationType.RemoveCarriageReturns)]
		[DataRow(WhitespaceNormalizationType.TrimTrailingWhitespace)]
		[DataRow(WhitespaceNormalizationType.TrimLeadingAndTrailingWhitespace)]
		public void CompareLinesTo_handles_null_lines(WhitespaceNormalizationType whitespaceNormalizationType)
		{
			var sourceLines = new[]
			{
				"Line 1",
				"Line 2",
				"Source Line 3",
				string.Empty,
				null,
				"Line 6",
				"Line 7",
				null
			};

			var targetLines = new[]
			{
				"Line 1",
				"Line 2",
				null,
				string.Empty,
				null,
				"Target Line 6",
				"Line 7",
				"Line 8"
			};

			var comparisonResult = StringComparisonHelper.CompareLinesTo(
				sourceLines!,
				targetLines!,
				WhitespaceNormalizationType.TrimTrailingWhitespace,
				maxComparisonOffset: 0
			);

			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[]
			{
				new StringComparisonRange(true, 0, 2, 0, 2),
				new StringComparisonRange(false, 2, 3, 2, 3),
				new StringComparisonRange(true, 3, 5, 3, 5),
				new StringComparisonRange(false, 5, 6, 5, 6),
				new StringComparisonRange(true, 6, 7, 6, 7),
				new StringComparisonRange(false, 7, 8, 7, 8),
			});
		}

		[TestMethod]
		public void CompareLinesTo_handles_the_maxComparisonOffset_as_expected1()
		{
			var sourceString =
@"Line 1
Line 2
Extra Line In Source String1
Extra Line In Source String2
Line 3
Extra Line In Source String3
Extra Line In Source String4
Extra Line In Source String5
Extra Line In Source String6
Extra Line In Source String7
Line 4";

			var targetString =
@"Line 1
Line 2
Line 3
Line 4";

			var comparisonResult = StringComparisonHelper.CompareLinesTo(
				sourceString,
				targetString,
				WhitespaceNormalizationType.TrimTrailingWhitespace,
				maxComparisonOffset: 10
			);

			comparisonResult.Should().NotBeNull();
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[]
			{
				new StringComparisonRange(true, 0, 2, 0, 2),
				new StringComparisonRange(false, 2, 4, 2, 2),
				new StringComparisonRange(true, 4, 5, 2, 3),
				new StringComparisonRange(false, 5, 10, 3, 3),
				new StringComparisonRange(true, 10, 11, 3, 4),
			});
		}

		[TestMethod]
		public void CompareLinesTo_handles_the_maxComparisonOffset_as_expected2()
		{
			var sourceString =
@"Line 1
Line 2
Extra Line In Source String1
Extra Line In Source String2
Line 3
Extra Line In Source String3
Extra Line In Source String4
Extra Line In Source String5
Extra Line In Source String6
Extra Line In Source String7
Line 4";

			var targetString =
@"Line 1
Line 2
Line 3
Line 4";

			var comparisonResult = StringComparisonHelper.CompareLinesTo(
				sourceString,
				targetString,
				WhitespaceNormalizationType.TrimTrailingWhitespace,
				maxComparisonOffset: 3
			);

			comparisonResult.Should().NotBeNull();
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[]
			{
				new StringComparisonRange(true, 0, 2, 0, 2),
				new StringComparisonRange(false, 2, 4, 2, 2),
				new StringComparisonRange(true, 4, 5, 2, 3),
				new StringComparisonRange(false, 5, 11, 3, 4),
			});
		}

		[TestMethod]
		public void CompareLinesTo_handles_the_maxComparisonOffset_as_expected3()
		{
			var sourceString =
@"Line 1
Line 2
Extra Line In Source String1
Extra Line In Source String2
Line 3
Extra Line In Source String3
Extra Line In Source String4
Extra Line In Source String5
Extra Line In Source String6
Extra Line In Source String7
Line 4";

			var targetString =
@"Line 1
Line 2
Line 3
Line 4";

			var comparisonResult = StringComparisonHelper.CompareLinesTo(
				sourceString,
				targetString,
				WhitespaceNormalizationType.TrimTrailingWhitespace,
				maxComparisonOffset: 0
			);

			comparisonResult.Should().NotBeNull();
			comparisonResult!.ComparisonRanges.Should().BeEquivalentTo(new[]
			{
				new StringComparisonRange(true, 0, 2, 0, 2),
				new StringComparisonRange(false, 2, 11, 2, 4),
			});
		}

		[TestMethod]
		public void GetComparisonDifferencesString_does_not_trim_line_trailing_whitespace_when_the_normalization_type_is_none()
		{
			var sourceString = "Single line string";
			var targetString = "Single line string   "; // Comparison

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, targetString, whitespaceNormalizationTypeForLines: WhitespaceNormalizationType.None);

			var expectedValue =
@"  -- Differing lines (source: 0 to 1)  (target: 0 to 1)
+ Single line string
- Single line string   

";
			// Ignore carriage returns in case the tests run on different platforms
			comparisonDifferenceResult.RemoveAllCarriageReturns().Should().Be(expectedValue.RemoveAllCarriageReturns());
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result1()
		{
			var sourceString = "Single line string";
			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, sourceString);
			comparisonDifferenceResult.Should().BeEmpty();
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result2()
		{
			var sourceString =
@"Line 1
Line 2 
Line 3";

			var targetString =
@"Line 1
Line 2
Line 3";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, targetString);
			comparisonDifferenceResult.Should().BeEmpty();
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result3()
		{
			var sourceString =
@"Line 1
Line 2
Extra Line In Source String
Line 3";

			var targetString =
@"Line 1
Line 2
Line 3";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, targetString);

			var expectedValue =
@"  -- Unmatched source lines (2 to 3)
+ Extra Line In Source String

";

			// Ignore carriage returns in case the tests run on different platforms
			VerifyExpectedDifferencesStringResult(comparisonDifferenceResult, expectedValue);
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result4()
		{
			var sourceString =
@"Line 1
Line 2
Line 3";

			var targetString =
@"Line 1
Extra Line In Target String
Line 2
Line 3";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, targetString);

			var expectedValue =
@"  -- Unmatched target lines (1 to 2)
- Extra Line In Target String

";

			// Ignore carriage returns in case the tests run on different platforms
			VerifyExpectedDifferencesStringResult(comparisonDifferenceResult, expectedValue);
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result5()
		{
			var sourceString =
@"Source Line 1
Line 2
Line 3
Line 4";

			var targetString =
@"Target Line 1
Target Line 2
Line 3
Line 4";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, targetString, maxComparisonOffset: 0);

			var expectedValue =
@"  -- Differing lines (source: 0 to 2)  (target: 0 to 2)
+ Source Line 1
+ Line 2
- Target Line 1
- Target Line 2

";

			// Ignore carriage returns in case the tests run on different platforms
			VerifyExpectedDifferencesStringResult(comparisonDifferenceResult, expectedValue);
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result6()
		{
			var sourceString =
@"Line 1
Line 2
Extra Source Line 1
Extra Source Line 2
Extra Source Line 3
Extra Source Line 4
Extra Source Line 5
Extra Source Line 6
Extra Source Line 7
Extra Source Line 8
Extra Source Line 9
Extra Source Line 10
Extra Source Line 11
Extra Source Line 12
Extra Source Line 13
Extra Source Line 14
Extra Source Line 15
Extra Source Line 16
Extra Source Line 17
Extra Source Line 18
Extra Source Line 19
Extra Source Line 21
Extra Source Line 22
Extra Source Line 23
Extra Source Line 24
Extra Source Line 25
Extra Source Line 26
Extra Source Line 27
Extra Source Line 28
Extra Source Line 29
Extra Source Line 30
Line 3
Line 4";

			var targetString =
@"Line 1
Line 2
Line 3
Line 4";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(sourceString, targetString, maxComparisonOffset: -1);

			var expectedValue =
@"  -- Unmatched source lines (2 to 31)
+ Extra Source Line 1
+ Extra Source Line 2
+ Extra Source Line 3
    ... 26 additional lines ...

";

			// Ignore carriage returns in case the tests run on different platforms
			VerifyExpectedDifferencesStringResult(comparisonDifferenceResult, expectedValue);
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result7()
		{
			var sourceString =
@"Line 1
Line 2
Line 3
Line 4";

			var targetString =
@"Line 1
Line 2
Extra Target Line 1
Extra Target Line 2
Extra Target Line 3
Extra Target Line 4
Extra Target Line 5
Extra Target Line 6
Extra Target Line 7
Extra Target Line 8
Extra Target Line 9
Extra Target Line 10
Extra Target Line 11
Extra Target Line 12
Extra Target Line 13
Extra Target Line 14
Extra Target Line 15
Extra Target Line 16
Extra Target Line 17
Extra Target Line 18
Extra Target Line 19
Extra Target Line 21
Extra Target Line 22
Extra Target Line 23
Extra Target Line 24
Extra Target Line 25
Extra Target Line 26
Extra Target Line 27
Extra Target Line 28
Extra Target Line 29
Extra Target Line 30
Line 3
Line 4";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(
				sourceString,
				targetString,
				maxComparisonOffset: -1,
				maxLinesFromRangeDifferenceToOuput: 4);

			var expectedValue =
@"  -- Unmatched target lines (2 to 31)
- Extra Target Line 1
- Extra Target Line 2
- Extra Target Line 3
- Extra Target Line 4
    ... 25 additional lines ...

";

			// Ignore carriage returns in case the tests run on different platforms
			VerifyExpectedDifferencesStringResult(comparisonDifferenceResult, expectedValue);
		}

		[TestMethod]
		public void GetComparisonDifferencesString_returns_the_expected_result8()
		{
			var sourceString =
@"Line 1
Line 2
Source Line 3
Source Line 4
Source Line 5
Source Line 6
Source Line 7
Source Line 8
Line 9
Line 10";

			var targetString =
@"Line 1
Line 2
Target Line 3
Target Line 4
Target Line 5
Target Line 6
Target Line 7
Target Line 8
Line 9
Line 10";

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(
				sourceString,
				targetString,
				maxLinesFromRangeDifferenceToOuput: 3);

			var expectedValue =
@"  -- Differing lines (source: 2 to 8)  (target: 2 to 8)
+ Source Line 3
+ Source Line 4
+ Source Line 5
    ... 3 additional lines ...
- Target Line 3
- Target Line 4
- Target Line 5
    ... 3 additional lines ...

";

			// Ignore carriage returns in case the tests run on different platforms
			VerifyExpectedDifferencesStringResult(comparisonDifferenceResult, expectedValue);
		}

		[TestMethod]
		public void GetComparisonDifferencesString_trims_leading_and_trailing_whitespace_before_comparing()
		{
			var sourceString = "   Single line string";
			var targetString = "Single line string   "; // Comparison

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(
				sourceString,
				targetString,
				whitespaceNormalizationTypeForLines: WhitespaceNormalizationType.TrimTrailingWhitespace
			);

			comparisonDifferenceResult.Should().NotBeEmpty();

			// Comparing using TrimLeadingAndTrailingWhitespace should return an empty comparison result
			comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(
				sourceString,
				targetString,
				whitespaceNormalizationTypeForLines: WhitespaceNormalizationType.TrimLeadingAndTrailingWhitespace
			);

			comparisonDifferenceResult.Should().BeEmpty();
		}

		[TestMethod]
		public void GetComparisonDifferencesString_trims_line_trailing_whitespace_before_comparing()
		{
			var sourceString = "Single line string";
			var targetString = "Single line string   "; // Comparison

			var comparisonDifferenceResult = StringComparisonHelper.GetComparisonDifferencesString(
				sourceString,
				targetString,
				whitespaceNormalizationTypeForLines: WhitespaceNormalizationType.TrimTrailingWhitespace
			);

			comparisonDifferenceResult.Should().BeEmpty();
		}

		[TestMethod]
		public void PrintVisibleWhitespace_does_not_blow_up_with_a_null_or_empty_argument()
		{
			StringComparisonHelper.PrintVisibleWhitespace(null).Should().BeNull();
			StringComparisonHelper.PrintVisibleWhitespace(string.Empty).Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveAllCarriageReturns_does_not_blow_up_with_null_argument()
		{
			StringComparisonHelper.RemoveAllCarriageReturns(stringToNormalize: null).Should().BeNull();
		}


		/******     TEST SETUP     *****************************
		 *******************************************************/
		public static void VerifyExpectedDifferencesStringResult(string actualDifferencesString, string expectedResult)
		{
			// Our comparison difference helper method escapes the \t \r \n characters in the output source/target line values.
			// Since this behavior can vary depending on the line ending of the environment running these tests, we'll
			// use this verification method in tests that aren't explicitly looking for the visibly escaped characters
			actualDifferencesString = actualDifferencesString.Replace("\r", string.Empty).Replace("\\r", string.Empty);
			expectedResult = expectedResult.Replace("\r", string.Empty);
			actualDifferencesString.Should().Be(expectedResult);
		}
	}
}
