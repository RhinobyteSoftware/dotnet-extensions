namespace Rhinobyte.Extensions.TestTools.Assertions
{
	/// <summary>
	/// Data object that represents a range of line numbers from a comparison of two multi-line strings.
	/// </summary>
	public class StringComparisonRange
	{
		/// <summary>
		/// Construct a new comparison range instance.
		/// </summary>
		public StringComparisonRange(
			bool isMatch,
			int sourceEndingLineNumber,
			int sourceStartingLineNumber,
			int targetEndingLineNumber,
			int targetStartingLineNumber)
		{
			IsMatch = isMatch;
			SourceEndingLineNumber = sourceEndingLineNumber;
			SourceStartingLineNumber = sourceStartingLineNumber;
			TargetEndingLineNumber = targetEndingLineNumber;
			TargetStartingLineNumber = targetStartingLineNumber;
		}

		/// <summary>
		/// Whether or not this is a matching or non-matching range of lines.
		/// </summary>
		public bool IsMatch { get; }

		/// <summary>
		/// The ending line number for the range in the source lines array
		/// </summary>
		public int SourceEndingLineNumber { get; }

		/// <summary>
		/// The starting line number for the range in the source lines array
		/// </summary>
		public int SourceStartingLineNumber { get; }

		/// <summary>
		/// The ending line number for the range in the target lines array
		/// </summary>
		public int TargetEndingLineNumber { get; }

		/// <summary>
		/// The starting line number for the range in the target lines array
		/// </summary>
		public int TargetStartingLineNumber { get; }
	}
}
