using System;
using System.Collections.Generic;
using System.Text;

namespace Rhinobyte.Extensions.TestTools.Assertions;

/// <summary>
/// Helper class for performing comparison between strings
/// </summary>
public static class StringComparisonHelper
{
	/// <summary>
	/// Appends a '\n' character or <see cref="Environment.NewLine" /> if the string builder does not already end with the new line characters.
	/// </summary>
	/// <param name="stringBuilder">The string builder to append to</param>
	/// <param name="newlineIncludesCarriageReturn">
	/// Whether or not the method should check for a terminating '\r' character and append only a '\n' character if the carraige return is present
	/// </param>
	public static StringBuilder AppendNewlineIfNecessary(this StringBuilder stringBuilder, bool newlineIncludesCarriageReturn)
	{
		_ = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));

		if (stringBuilder.Length == 0)
			return stringBuilder.Append(Environment.NewLine);

		var finalCharacter = stringBuilder[stringBuilder.Length - 1];
		if (finalCharacter.Equals('\n'))
			return stringBuilder;

		if (newlineIncludesCarriageReturn && finalCharacter.Equals('\r'))
			return stringBuilder.Append('\n');

		return stringBuilder.Append(Environment.NewLine);
	}

	/// <summary>
	/// Splits the source string and the expected strings in an array of lines using the '\n' character.
	/// <para>Compares the lines ingoring line ending differences and throws an AssertionFailedException if the lines don't match</para>
	/// </summary>
	/// <remarks>
	/// <para>A <paramref name="maxComparisonOffset"/> value of -1 or lower is treated as unlimited</para>
	/// <para>A <paramref name="maxComparisonOffset"/> value of 0 will skip comparing local ranges and instead compare exact line numbers only</para>
	/// </remarks>
	public static StringComparisonResult? CompareLinesTo(
		this string source,
		string target,
		WhitespaceNormalizationType whitespaceNormalizationTypeForLines,
		int maxComparisonOffset = 5)
	{
		if (source is null || target is null)
			return null;

		return CompareLinesTo(source.Split('\n'), target.Split('\n'), whitespaceNormalizationTypeForLines, maxComparisonOffset: maxComparisonOffset);
	}

	/// <summary>
	/// Compares the source and target lines and returns a collection of comparison range results indicating which ranges of lines matched and
	/// which ranges of lines did not match.
	/// </summary>
	/// <remarks>
	/// <para>A <paramref name="maxComparisonOffset"/> value of -1 or lower is treated as unlimited</para>
	/// <para>A <paramref name="maxComparisonOffset"/> value of 0 will skip comparing local ranges and instead compare exact line numbers only</para>
	/// </remarks>
	public static StringComparisonResult? CompareLinesTo(
		string[] sourceLines,
		string[] targetLines,
		WhitespaceNormalizationType whitespaceNormalizationTypeForLines,
		int maxComparisonOffset = 5)
	{
		if (sourceLines is null || targetLines is null || sourceLines.Length < 1 || targetLines.Length < 1)
			return null;

		var isUnlimitedComparisonOffset = maxComparisonOffset < 0;
		var skipLocalRangeComparisons = maxComparisonOffset == 0;
		var isCurrentRangeMatched = DoLinesMatch(sourceLines[0], targetLines[0], whitespaceNormalizationTypeForLines);

		var sourceBeginningLineNumber = 0;
		var targetBeginningLineNumber = 0;
		var sourceIndex = 1;
		var targetIndex = 1;
		var sourceUpperBound = sourceLines.Length;
		var targetUpperBound = targetLines.Length;

		var comparisonRanges = new List<StringComparisonRange>();

		while (sourceIndex < sourceUpperBound && targetIndex < targetUpperBound)
		{
			var doLinesMatch = DoLinesMatch(sourceLines[sourceIndex], targetLines[targetIndex], whitespaceNormalizationTypeForLines);
			if (isCurrentRangeMatched)
			{
				if (doLinesMatch)
				{
					++sourceIndex;
					++targetIndex;
					continue;
				}

				comparisonRanges.Add(new StringComparisonRange(true, sourceBeginningLineNumber, sourceIndex, targetBeginningLineNumber, targetIndex));
				isCurrentRangeMatched = false;
				sourceBeginningLineNumber = sourceIndex;
				targetBeginningLineNumber = targetIndex;
			}
			else if (doLinesMatch)
			{
				comparisonRanges.Add(new StringComparisonRange(false, sourceBeginningLineNumber, sourceIndex, targetBeginningLineNumber, targetIndex));
				isCurrentRangeMatched = true;
				sourceBeginningLineNumber = sourceIndex;
				targetBeginningLineNumber = targetIndex;
				++sourceIndex;
				++targetIndex;
				continue;
			}

			if (skipLocalRangeComparisons)
			{
				++sourceIndex;
				++targetIndex;
				continue;
			}

			int? nextSourceIndexMatchOffset = null;
			var localSourceUpperBound = isUnlimitedComparisonOffset
				? sourceUpperBound
				: Math.Min(sourceUpperBound, sourceIndex + maxComparisonOffset + 1);

			for (var localSourceIndex = sourceIndex + 1; localSourceIndex < localSourceUpperBound; ++localSourceIndex)
			{
				if (DoLinesMatch(sourceLines[localSourceIndex], targetLines[targetIndex], whitespaceNormalizationTypeForLines))
				{
					nextSourceIndexMatchOffset = localSourceIndex - sourceIndex;
					break;
				}
			}

			if (nextSourceIndexMatchOffset != null && nextSourceIndexMatchOffset < 3)
			{
				var nextMatchedSourceIndex = sourceIndex + nextSourceIndexMatchOffset.Value;
				comparisonRanges.Add(new StringComparisonRange(false, sourceBeginningLineNumber, nextMatchedSourceIndex, targetBeginningLineNumber, targetIndex));

				isCurrentRangeMatched = true;
				sourceBeginningLineNumber = nextMatchedSourceIndex;
				sourceIndex = nextMatchedSourceIndex + 1;
				targetBeginningLineNumber = targetIndex;
				++targetIndex;
				continue;
			}

			int? nextTargetIndexMatchOffset = null;
			var localTargetUpperBound = isUnlimitedComparisonOffset
				? targetUpperBound
				: Math.Min(targetUpperBound, targetIndex + maxComparisonOffset + 1);
			for (var localTargetIndex = targetIndex + 1; localTargetIndex < localTargetUpperBound; ++localTargetIndex)
			{
				if (DoLinesMatch(sourceLines[sourceIndex], targetLines[localTargetIndex], whitespaceNormalizationTypeForLines))
				{
					nextTargetIndexMatchOffset = localTargetIndex - targetIndex;
					break;
				}
			}

			if (nextSourceIndexMatchOffset != null
				&& (nextTargetIndexMatchOffset == null || nextSourceIndexMatchOffset < nextTargetIndexMatchOffset))
			{
				var nextMatchedSourceIndex = sourceIndex + nextSourceIndexMatchOffset.Value;
				comparisonRanges.Add(new StringComparisonRange(false, sourceBeginningLineNumber, nextMatchedSourceIndex, targetBeginningLineNumber, targetIndex));

				isCurrentRangeMatched = true;
				sourceBeginningLineNumber = nextMatchedSourceIndex;
				sourceIndex = nextMatchedSourceIndex + 1;
				targetBeginningLineNumber = targetIndex;
				++targetIndex;

				continue;
			}

			if (nextTargetIndexMatchOffset != null)
			{
				var nextMatchedTargetIndex = targetIndex + nextTargetIndexMatchOffset.Value;
				comparisonRanges.Add(new StringComparisonRange(false, sourceBeginningLineNumber, sourceIndex, targetBeginningLineNumber, nextMatchedTargetIndex));

				isCurrentRangeMatched = true;
				targetBeginningLineNumber = nextMatchedTargetIndex;
				targetIndex = nextMatchedTargetIndex + 1;
				sourceBeginningLineNumber = sourceIndex;
				++sourceIndex;

				continue;
			}

			++sourceIndex;
			++targetIndex;
		}

		if (isCurrentRangeMatched)
		{
			comparisonRanges.Add(new StringComparisonRange(true, sourceBeginningLineNumber, sourceIndex, targetBeginningLineNumber, targetIndex));
			sourceBeginningLineNumber = sourceIndex;
			targetBeginningLineNumber = targetIndex;
		}

		if (sourceBeginningLineNumber < sourceUpperBound || targetBeginningLineNumber < targetUpperBound)
			comparisonRanges.Add(new StringComparisonRange(false, sourceBeginningLineNumber, sourceUpperBound, targetBeginningLineNumber, targetUpperBound));

		return new StringComparisonResult(comparisonRanges, sourceLines, targetLines);
	}

	private static bool DoLinesMatch(
		string? sourceLine,
		string? targetLine,
		WhitespaceNormalizationType whitespaceNormalizationType)
	{
		if (sourceLine is null)
			return targetLine is null;

		if (targetLine is null)
			return false;

		switch (whitespaceNormalizationType)
		{
			case WhitespaceNormalizationType.None:
				break;

			case WhitespaceNormalizationType.RemoveCarriageReturns:
#if NETFRAMEWORK || NETSTANDARD2_0
				sourceLine = sourceLine.Replace("\r", string.Empty);
				targetLine = targetLine.Replace("\r", string.Empty);
#else
				sourceLine = sourceLine.Replace("\r", string.Empty, StringComparison.Ordinal);
				targetLine = targetLine.Replace("\r", string.Empty, StringComparison.Ordinal);
#endif
				break;

			case WhitespaceNormalizationType.TrimLeadingAndTrailingWhitespace:
				sourceLine = sourceLine.Trim();
				targetLine = targetLine.Trim();
				break;

			case WhitespaceNormalizationType.TrimTrailingWhitespace:
				sourceLine = sourceLine.TrimEnd();
				targetLine = targetLine.TrimEnd();
				break;

			default:
				throw new NotImplementedException($"{nameof(StringComparisonHelper)}.{nameof(DoLinesMatch)} is not implemented for the {nameof(WhitespaceNormalizationType)} value of {whitespaceNormalizationType}");
		}

		return sourceLine.Equals(targetLine, StringComparison.Ordinal);
	}

	/// <summary>
	/// Compares the individual lines between <paramref name="source"/> and <paramref name="target"/> and returns a description of any
	/// differences between the two.
	/// </summary>
	public static string GetComparisonDifferencesString(
		this string source,
		string target,
		int maxComparisonOffset = 5,
		int maxLinesFromRangeDifferenceToOuput = 3,
		WhitespaceNormalizationType whitespaceNormalizationTypeForLines = WhitespaceNormalizationType.TrimTrailingWhitespace)
	{
		var comparisonResult = CompareLinesTo(source, target, whitespaceNormalizationTypeForLines, maxComparisonOffset: maxComparisonOffset);
		if (comparisonResult is null)
			return string.Empty;

		var comparisonRanges = comparisonResult.ComparisonRanges;
		if (comparisonRanges.Count == 1 && comparisonRanges[0].IsMatch)
			return string.Empty;

#if NETFRAMEWORK || NETSTANDARD2_0
		var doesNewlineIncludeCarriageReturn = Environment.NewLine.IndexOf('\r') > -1;
#else
		var doesNewlineIncludeCarriageReturn = Environment.NewLine.IndexOf('\r', StringComparison.Ordinal) > -1;
#endif


		var errorMessageBuilder = new StringBuilder();
		var sourceLines = comparisonResult.SourceLines;
		var targetLines = comparisonResult.TargetLines;
		int localUpperBound;
		int truncatedLines;
		foreach (var range in comparisonRanges)
		{
			if (range.IsMatch)
				continue;

			if (range.TargetEndingLineNumber == range.TargetBeginningLineNumber)
			{
				_ = errorMessageBuilder
					.Append("  -- Unmatched source lines (")
					.Append(range.SourceBeginningLineNumber)
					.Append(" to ")
					.Append(range.SourceEndingLineNumber)
					.Append(')')
					.Append(Environment.NewLine);

				localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
					? range.SourceEndingLineNumber
					: Math.Min(range.SourceEndingLineNumber, range.SourceBeginningLineNumber + maxLinesFromRangeDifferenceToOuput);

				for (var sourceLineIndex = range.SourceBeginningLineNumber; sourceLineIndex < localUpperBound; ++sourceLineIndex)
					_ = errorMessageBuilder.Append("+ ").Append(PrintVisibleWhitespace(sourceLines[sourceLineIndex])).AppendNewlineIfNecessary(doesNewlineIncludeCarriageReturn);

				truncatedLines = range.SourceEndingLineNumber - localUpperBound;
				if (truncatedLines > 0)
					_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

				_ = errorMessageBuilder.Append(Environment.NewLine);
				continue;
			}

			if (range.SourceEndingLineNumber == range.SourceBeginningLineNumber)
			{
				_ = errorMessageBuilder
					.Append("  -- Unmatched target lines (")
					.Append(range.TargetBeginningLineNumber)
					.Append(" to ")
					.Append(range.TargetEndingLineNumber)
					.Append(')')
					.Append(Environment.NewLine);

				localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
					? range.TargetEndingLineNumber
					: Math.Min(range.TargetEndingLineNumber, range.TargetBeginningLineNumber + maxLinesFromRangeDifferenceToOuput);

				for (var targetLineIndex = range.TargetBeginningLineNumber; targetLineIndex < localUpperBound; ++targetLineIndex)
					_ = errorMessageBuilder.Append("- ").Append(PrintVisibleWhitespace(targetLines[targetLineIndex])).AppendNewlineIfNecessary(doesNewlineIncludeCarriageReturn);

				truncatedLines = range.TargetEndingLineNumber - localUpperBound;
				if (truncatedLines > 0)
					_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

				_ = errorMessageBuilder.Append(Environment.NewLine);
				continue;
			}

			_ = errorMessageBuilder
				.Append("  -- Differing lines (source: ")
				.Append(range.SourceBeginningLineNumber)
				.Append(" to ")
				.Append(range.SourceEndingLineNumber)
				.Append(")  (target: ")
				.Append(range.TargetBeginningLineNumber)
				.Append(" to ")
				.Append(range.TargetEndingLineNumber)
				.Append(')')
				.Append(Environment.NewLine);

			localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
					? range.SourceEndingLineNumber
					: Math.Min(range.SourceEndingLineNumber, range.SourceBeginningLineNumber + maxLinesFromRangeDifferenceToOuput);

			for (var sourceLineIndex = range.SourceBeginningLineNumber; sourceLineIndex < localUpperBound; ++sourceLineIndex)
				_ = errorMessageBuilder.Append("+ ").Append(PrintVisibleWhitespace(sourceLines[sourceLineIndex])).AppendNewlineIfNecessary(doesNewlineIncludeCarriageReturn);

			truncatedLines = range.SourceEndingLineNumber - localUpperBound;
			if (truncatedLines > 0)
				_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

			localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
					? range.TargetEndingLineNumber
					: Math.Min(range.TargetEndingLineNumber, range.TargetBeginningLineNumber + maxLinesFromRangeDifferenceToOuput);

			for (var targetLineIndex = range.TargetBeginningLineNumber; targetLineIndex < localUpperBound; ++targetLineIndex)
				_ = errorMessageBuilder.Append("- ").Append(PrintVisibleWhitespace(targetLines[targetLineIndex])).AppendNewlineIfNecessary(doesNewlineIncludeCarriageReturn);

			truncatedLines = range.TargetEndingLineNumber - localUpperBound;
			if (truncatedLines > 0)
				_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

			_ = errorMessageBuilder.Append(Environment.NewLine);
		}

		return errorMessageBuilder.ToString();
	}

	/// <summary>
	/// When building the comparison differences string result we use this method to 'escape'
	/// the \t, \r, and \n characters so that they're visible in the line comparison parts
	/// of the message.
	/// </summary>
	internal static string? PrintVisibleWhitespace(string? stringToModify)
	{
		if (stringToModify is null || stringToModify.Length == 0)
			return stringToModify;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		return stringToModify
			.Replace("\t", "\\t", StringComparison.Ordinal)
			.Replace("\r", "\\r", StringComparison.Ordinal)
			.Replace("\n", "\\n", StringComparison.Ordinal);
#else
		return stringToModify
			.Replace("\t", "\\t")
			.Replace("\r", "\\r")
			.Replace("\n", "\\n");
#endif
	}

	/// <summary>
	/// Return a copy of <paramref name="stringToNormalize"/> with any carriage return + new line sequences normalized to just the newline
	/// character for consistent comparisons.
	/// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
	[return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(stringToNormalize))]
#endif
	public static string? RemoveAllCarriageReturns(this string? stringToNormalize)
	{
		if (stringToNormalize is null)
			return null;

#if NETFRAMEWORK || NETSTANDARD2_0
		return stringToNormalize.Replace("\r", string.Empty);
#else
		return stringToNormalize.Replace("\r", string.Empty, StringComparison.Ordinal);
#endif
	}
}
