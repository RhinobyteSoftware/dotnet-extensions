namespace Rhinobyte.Extensions.TestTools.Assertions;

/// <summary>
/// Enum used to specify how the <see cref="StringComparisonHelper"/> should handle whitespace normalization for string comparison operations.
/// </summary>
public enum WhitespaceNormalizationType
{
	/// <summary>
	/// Don't normalize whitespace
	/// </summary>
	None = 0,

	/// <summary>
	/// Remove all carriage return characters
	/// </summary>
	RemoveCarriageReturns = 1,

	/// <summary>
	/// Trim both the leading and trailing whitespace
	/// </summary>
	TrimLeadingAndTrailingWhitespace = 2,

	/// <summary>
	/// Trim all of the trailing whitespace
	/// </summary>
	TrimTrailingWhitespace = 3
}
