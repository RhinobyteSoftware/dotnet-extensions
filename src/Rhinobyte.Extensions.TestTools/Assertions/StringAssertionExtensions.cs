using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Rhinobyte.Extensions.TestTools.Assertions;

/// <summary>
/// <see cref="string"/> extension methods used to perform test assertions with whitespace normalization support.
/// <para>
/// Many of these extensions leverage the <see cref="StringComparisonHelper"/> method to provide a breakdown of the specific line
/// differences in the generated assertion failure exception.
/// </para>
/// </summary>
public static class StringAssertionExtensions
{
	/// <summary>
	/// Extension to assert that a source string matches an expected string. The extension supports normalizing whitespace in both strings
	/// and leverages the <see cref="StringComparisonHelper.GetComparisonDifferencesString(string, string, int, int, WhitespaceNormalizationType)"/>
	/// to output an assertion failure message that shows the specific lines that differ between the source and expected values.
	/// </summary>
	public static void ShouldBeSameAs(
		this string? sourceString,
		string? expected,
		int maxComparisonOffset = 20,
		int maxLinesFromRangeDifferenceToOuput = 3,
		WhitespaceNormalizationType whitespaceNormalizationType = WhitespaceNormalizationType.TrimTrailingWhitespace)
	{
		if (sourceString is null && expected is null)
			return;

		if (sourceString is null)
			throw new AssertFailedException($"The source string (null) does not match the expected value of:{Environment.NewLine}{Environment.NewLine}{expected}{Environment.NewLine}");

		if (expected is null)
			throw new AssertFailedException($"Expected source string to be null but instead the source string has a value of:{Environment.NewLine}{Environment.NewLine}{sourceString}{Environment.NewLine}");

		if (sourceString.Length == 0 && expected.Length == 0)
			return;

		if (sourceString.Length == 0)
			throw new AssertFailedException($"The source string (empty) does not match the expected value of:{Environment.NewLine}{Environment.NewLine}{expected}{Environment.NewLine}");

		if (expected.Length == 0)
			throw new AssertFailedException($"Expected source string to be empty but instead the source string has a value of:{Environment.NewLine}{Environment.NewLine}{sourceString}{Environment.NewLine}");

		var differencesString = StringComparisonHelper.GetComparisonDifferencesString(sourceString, expected, maxComparisonOffset, maxLinesFromRangeDifferenceToOuput, whitespaceNormalizationType);
		if (string.IsNullOrWhiteSpace(differencesString))
			return;

		throw new AssertFailedException($"Source string does not match the expected value. The following line differences we're found:{Environment.NewLine}{differencesString}");
	}
}
