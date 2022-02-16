using System.Collections.Generic;

namespace Rhinobyte.Extensions.TestTools.Assertions;

/// <summary>
/// String comparison result between two multi-line strings
/// </summary>
public class StringComparisonResult
{
	/// <summary>
	/// Construct a new comparison result
	/// </summary>
	public StringComparisonResult(IReadOnlyList<StringComparisonRange> comparisonRanges, string[] sourceLines, string[] targetLines)
	{
		ComparisonRanges = comparisonRanges;
		SourceLines = sourceLines;
		TargetLines = targetLines;
	}

	/// <summary>
	/// The matching and unmatching comparison ranges
	/// </summary>
	public IReadOnlyList<StringComparisonRange> ComparisonRanges { get; }

	/// <summary>
	/// The individual lines of the source string
	/// </summary>
	public IReadOnlyList<string> SourceLines { get; }

	/// <summary>
	/// The individual lines of the target string
	/// </summary>
	public IReadOnlyList<string> TargetLines { get; }
}
