using System;
using System.Collections.Generic;
using System.Text;

namespace Rhinobyte.Extensions.TestTools.Assertions
{
	/// <summary>
	/// Helper class for performing comparison between strings
	/// </summary>
	public static class StringComparisonHelper
	{
		/// <summary>
		/// Splits the source string and the expected strings in an array of lines using the '\n' character.
		/// <para>Compares the lines ingoring line ending differences and throws an AssertionFailedException if the lines don't match</para>
		/// <para></para>
		/// </summary>
		public static StringComparisonResult? CompareLinesTo(this string source, string target, bool trimTrailingWhitespaceFromLines)
		{
			if (source is null || target is null)
				return null;

			return CompareLinesTo(source.Split('\n'), target.Split('\n'), trimTrailingWhitespaceFromLines);
		}

		/// <summary>
		/// Compares the source and target lines and returns a collection of comparison range results indicating which ranges of lines matched and
		/// which ranges of lines did not match.
		/// </summary>
		public static StringComparisonResult? CompareLinesTo(
			string[] sourceLines,
			string[] targetLines,
			bool trimTrailingWhitespaceFromLines,
			int? maxComparisonOffset = 20)
		{
			if (sourceLines is null || targetLines is null || sourceLines.Length < 1 || targetLines.Length < 1)
				return null;

			var skipLocalRangeComparisons = maxComparisonOffset != null && maxComparisonOffset < 1;
			var isCurrentRangeMatched = DoLinesMatch(sourceLines[0], targetLines[0], trimTrailingWhitespaceFromLines);

			var sourceStartingLineNumber = 0;
			var targetStartingLineNumber = 0;
			var sourceIndex = 1;
			var targetIndex = 1;
			var sourceUpperBound = sourceLines.Length;
			var targetUpperBound = targetLines.Length;

			var comparisonRanges = new List<StringComparisonRange>();

			while (sourceIndex < sourceUpperBound && targetIndex < targetUpperBound)
			{
				var doLinesMatch = DoLinesMatch(sourceLines[sourceIndex], targetLines[targetIndex], trimTrailingWhitespaceFromLines);
				if (isCurrentRangeMatched)
				{
					if (doLinesMatch)
					{
						++sourceIndex;
						++targetIndex;
						continue;
					}

					comparisonRanges.Add(new StringComparisonRange(true, sourceIndex, sourceStartingLineNumber, targetIndex, targetStartingLineNumber));
					isCurrentRangeMatched = false;
					sourceStartingLineNumber = sourceIndex;
					targetStartingLineNumber = targetIndex;
					++sourceIndex;
					++targetIndex;
					continue;
				}

				if (doLinesMatch)
				{
					comparisonRanges.Add(new StringComparisonRange(false, sourceIndex, sourceStartingLineNumber, targetIndex, targetStartingLineNumber));
					isCurrentRangeMatched = true;
					sourceStartingLineNumber = sourceIndex;
					targetStartingLineNumber = targetIndex;
					++sourceIndex;
					++targetIndex;
				}

				if (skipLocalRangeComparisons)
				{
					++sourceIndex;
					++targetIndex;
					continue;
				}

				int? nextSourceIndexMatchOffset = null;
				var localSourceUpperBound = maxComparisonOffset == null
					? sourceUpperBound
					: Math.Min(sourceUpperBound, sourceIndex + maxComparisonOffset.Value + 1);

				for (var localSourceIndex = sourceIndex + 1; localSourceIndex < localSourceUpperBound; ++localSourceIndex)
				{
					if (DoLinesMatch(sourceLines[localSourceIndex], targetLines[targetIndex], trimTrailingWhitespaceFromLines))
					{
						nextSourceIndexMatchOffset = localSourceIndex - sourceIndex;
						break;
					}
				}

				if (nextSourceIndexMatchOffset != null && nextSourceIndexMatchOffset < 3)
				{
					var nextMatchedSourceIndex = sourceIndex + nextSourceIndexMatchOffset.Value;
					comparisonRanges.Add(new StringComparisonRange(false, nextMatchedSourceIndex, sourceStartingLineNumber, targetIndex, targetStartingLineNumber));

					isCurrentRangeMatched = true;
					sourceStartingLineNumber = nextMatchedSourceIndex;
					sourceIndex = nextMatchedSourceIndex + 1;
					targetStartingLineNumber = targetIndex;
					++targetIndex;
					continue;
				}

				int? nextTargetIndexMatchOffset = null;
				var localTargetUpperBound = maxComparisonOffset == null
					? targetUpperBound
					: Math.Min(targetUpperBound, targetIndex + maxComparisonOffset.Value + 1);
				for (var localTargetIndex = targetIndex + 1; localTargetIndex < localTargetUpperBound; ++localTargetIndex)
				{
					if (DoLinesMatch(sourceLines[sourceIndex], targetLines[localTargetIndex], trimTrailingWhitespaceFromLines))
					{
						nextTargetIndexMatchOffset = localTargetIndex - targetIndex;
						break;
					}
				}

				if (nextSourceIndexMatchOffset != null
					&& (nextTargetIndexMatchOffset == null || nextSourceIndexMatchOffset < nextTargetIndexMatchOffset))
				{
					var nextMatchedSourceIndex = sourceIndex + nextSourceIndexMatchOffset.Value;
					comparisonRanges.Add(new StringComparisonRange(false, nextMatchedSourceIndex, sourceStartingLineNumber, targetIndex, targetStartingLineNumber));

					isCurrentRangeMatched = true;
					sourceStartingLineNumber = nextMatchedSourceIndex;
					sourceIndex = nextMatchedSourceIndex + 1;
					targetStartingLineNumber = targetIndex;
					++targetIndex;

					continue;
				}

				if (nextTargetIndexMatchOffset != null)
				{
					var nextMatchedTargetIndex = targetIndex + nextTargetIndexMatchOffset.Value;
					comparisonRanges.Add(new StringComparisonRange(false, sourceIndex, sourceStartingLineNumber, nextMatchedTargetIndex, targetStartingLineNumber));

					isCurrentRangeMatched = true;
					targetStartingLineNumber = nextMatchedTargetIndex;
					targetIndex = nextMatchedTargetIndex + 1;
					sourceStartingLineNumber = sourceIndex;
					++sourceIndex;

					continue;
				}

				++sourceIndex;
				++targetIndex;
			}

			comparisonRanges.Add(new StringComparisonRange(isCurrentRangeMatched, sourceIndex, sourceStartingLineNumber, targetIndex, targetStartingLineNumber));
			if (sourceIndex < sourceUpperBound)
			{
				comparisonRanges.Add(new StringComparisonRange(false, sourceUpperBound, sourceIndex, targetUpperBound, targetUpperBound));
			}
			else if (targetIndex < targetUpperBound)
			{
				comparisonRanges.Add(new StringComparisonRange(false, sourceUpperBound, sourceUpperBound, targetUpperBound, targetIndex));
			}

			return new StringComparisonResult(comparisonRanges, sourceLines, targetLines);
		}

		private static bool DoLinesMatch(string sourceLine, string targetLine, bool trimTrailingWhitespace)
		{
			return trimTrailingWhitespace
				? sourceLine.TrimEnd() == targetLine.TrimEnd()
				: sourceLine.Equals(targetLine, StringComparison.Ordinal);
		}

		/// <summary>
		/// Compares the individual lines between <paramref name="source"/> and <paramref name="target"/> and returns a description of any
		/// differences between the two.
		/// </summary>
		public static string GetComparisonDifferencesString(
			this string source,
			string target,
			int maxLinesFromRangeDifferenceToOuput = 3,
			bool trimTrailingWhitespaceFromLines = true)
		{
			var comparisonResult = CompareLinesTo(source, target, trimTrailingWhitespaceFromLines);
			if (comparisonResult is null || comparisonResult.ComparisonRanges.Count < 1)
				return string.Empty;

			var comparisonRanges = comparisonResult.ComparisonRanges;
			if (comparisonRanges.Count == 1 && comparisonRanges[0].IsMatch)
				return string.Empty;

			var errorMessageBuilder = new StringBuilder();
			var sourceLines = comparisonResult.SourceLines;
			var targetLines = comparisonResult.TargetLines;
			int localUpperBound;
			int truncatedLines;
			foreach (var range in comparisonRanges)
			{
				if (range.IsMatch)
					continue;

				if (range.TargetEndingLineNumber == range.TargetStartingLineNumber)
				{
					_ = errorMessageBuilder
						.Append("-- Unmatched source lines (")
						.Append(range.SourceStartingLineNumber)
						.Append(" to ")
						.Append(range.SourceEndingLineNumber)
						.Append(')')
						.Append(Environment.NewLine);

					localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
						? range.SourceEndingLineNumber
						: Math.Min(range.SourceEndingLineNumber, range.SourceStartingLineNumber + maxLinesFromRangeDifferenceToOuput);

					for (var sourceLineIndex = range.SourceStartingLineNumber; sourceLineIndex < localUpperBound; ++sourceLineIndex)
						_ = errorMessageBuilder.Append("+ ").Append(sourceLines[sourceLineIndex]);

					truncatedLines = range.SourceEndingLineNumber - localUpperBound;
					if (truncatedLines > 0)
						_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

					_ = errorMessageBuilder.Append(Environment.NewLine);
					continue;
				}

				if (range.SourceEndingLineNumber == range.SourceStartingLineNumber)
				{
					_ = errorMessageBuilder
						.Append("-- Unmatched target lines (")
						.Append(range.TargetStartingLineNumber)
						.Append(" to ")
						.Append(range.TargetEndingLineNumber)
						.Append(')')
						.Append(Environment.NewLine);

					localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
						? range.TargetEndingLineNumber
						: Math.Min(range.TargetEndingLineNumber, range.TargetStartingLineNumber + maxLinesFromRangeDifferenceToOuput);

					for (var targetLineIndex = range.TargetStartingLineNumber; targetLineIndex < localUpperBound; ++targetLineIndex)
						_ = errorMessageBuilder.Append("- ").Append(targetLines[targetLineIndex]);

					truncatedLines = range.TargetEndingLineNumber - localUpperBound;
					if (truncatedLines > 0)
						_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

					_ = errorMessageBuilder.Append(Environment.NewLine);
					continue;
				}

				_ = errorMessageBuilder
					.Append("-- Differing lines (source: ")
					.Append(range.SourceStartingLineNumber)
					.Append(" to ")
					.Append(range.SourceEndingLineNumber)
					.Append(")  (target: ")
					.Append(range.TargetStartingLineNumber)
					.Append(" to ")
					.Append(range.TargetEndingLineNumber)
					.Append(')')
					.Append(Environment.NewLine);

				localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
						? range.SourceEndingLineNumber
						: Math.Min(range.SourceEndingLineNumber, range.SourceStartingLineNumber + maxLinesFromRangeDifferenceToOuput);

				for (var sourceLineIndex = range.SourceStartingLineNumber; sourceLineIndex < localUpperBound; ++sourceLineIndex)
					_ = errorMessageBuilder.Append("+ ").Append(sourceLines[sourceLineIndex]);

				truncatedLines = range.SourceEndingLineNumber - localUpperBound;
				if (truncatedLines > 0)
					_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

				localUpperBound = maxLinesFromRangeDifferenceToOuput < 0
						? range.TargetEndingLineNumber
						: Math.Min(range.TargetEndingLineNumber, range.TargetStartingLineNumber + maxLinesFromRangeDifferenceToOuput);

				for (var targetLineIndex = range.TargetStartingLineNumber; targetLineIndex < localUpperBound; ++targetLineIndex)
					_ = errorMessageBuilder.Append("- ").Append(targetLines[targetLineIndex]);

				truncatedLines = range.TargetEndingLineNumber - localUpperBound;
				if (truncatedLines > 0)
					_ = errorMessageBuilder.Append("    ... ").Append(truncatedLines).Append(" additional lines ...").Append(Environment.NewLine);

				_ = errorMessageBuilder.Append(Environment.NewLine);
			}

			return errorMessageBuilder.ToString();
		}
	}
}
