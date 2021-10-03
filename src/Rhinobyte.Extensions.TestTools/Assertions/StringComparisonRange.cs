using System;

namespace Rhinobyte.Extensions.TestTools.Assertions
{
	/// <summary>
	/// Data object that represents a range of line numbers from a comparison of two multi-line strings.
	/// </summary>
	public class StringComparisonRange : IEquatable<StringComparisonRange>
	{
		/// <summary>
		/// Construct a new comparison range instance.
		/// </summary>
		public StringComparisonRange(
			bool isMatch,
			int sourceBeginningLineNumber,
			int sourceEndingLineNumber,
			int targetBeginningLineNumber,
			int targetEndingLineNumber)
		{
			IsMatch = isMatch;
			SourceBeginningLineNumber = sourceBeginningLineNumber;
			SourceEndingLineNumber = sourceEndingLineNumber;
			TargetBeginningLineNumber = targetBeginningLineNumber;
			TargetEndingLineNumber = targetEndingLineNumber;
		}

		/// <summary>
		/// Whether or not this is a matching or non-matching range of lines.
		/// </summary>
		public bool IsMatch { get; }

		/// <summary>
		/// The beginning line number for the range in the source lines array
		/// </summary>
		public int SourceBeginningLineNumber { get; }

		/// <summary>
		/// The ending line number for the range in the source lines array
		/// </summary>
		public int SourceEndingLineNumber { get; }

		/// <summary>
		/// The beginning line number for the range in the target lines array
		/// </summary>
		public int TargetBeginningLineNumber { get; }

		/// <summary>
		/// The ending line number for the range in the target lines array
		/// </summary>
		public int TargetEndingLineNumber { get; }


		/// <inheritdoc />
		public override bool Equals(object? obj) =>
			obj is StringComparisonRange other && Equals(other: other);

		/// <inheritdoc />
		public bool Equals(StringComparisonRange? other)
		{
			if (other is null)
				return false;

			return other.IsMatch == this.IsMatch
				&& other.SourceBeginningLineNumber == this.SourceBeginningLineNumber
				&& other.SourceEndingLineNumber == this.SourceEndingLineNumber
				&& other.TargetBeginningLineNumber == this.TargetBeginningLineNumber
				&& other.TargetEndingLineNumber == this.TargetEndingLineNumber;
		}

#if NETSTANDARD2_1_OR_GREATER
		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCode.Combine(this.IsMatch, this.SourceEndingLineNumber, this.SourceBeginningLineNumber, this.TargetEndingLineNumber, this.TargetBeginningLineNumber);
#elif NETCOREAPP
		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCode.Combine(this.IsMatch, this.SourceEndingLineNumber, this.SourceBeginningLineNumber, this.TargetEndingLineNumber, this.TargetBeginningLineNumber);
#else
		// TODO: If we every need to do this elsewhere, created a file to share across projects
		// See: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/HashCode.cs
		private const uint Prime2 = 2246822519U;
		private const uint Prime3 = 3266489917U;
		private const uint Prime4 = 668265263U;

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			var hashCode1 = (uint)SourceBeginningLineNumber.GetHashCode();
			var hashCode2 = (uint)SourceEndingLineNumber.GetHashCode();
			var hashCode3 = (uint)TargetBeginningLineNumber.GetHashCode();
			var hashCode4 = (uint)TargetEndingLineNumber.GetHashCode();
			var mixedHash = ((hashCode1 << 1) | (hashCode1 >> 31))
				+ ((hashCode2 << 7) | (hashCode2 >> 25))
				+ ((hashCode3 << 12) | (hashCode3 >> 20))
				+ ((hashCode4 << 18) | (hashCode3 >> 14));

			mixedHash += 20;
			mixedHash += (((uint)IsMatch.GetHashCode()) * Prime3);
			mixedHash = ((mixedHash << 17) | (mixedHash >> 15)) * Prime4;
			mixedHash ^= hashCode1 >> 15;
			mixedHash *= Prime2;
			mixedHash ^= mixedHash >> 13;
			mixedHash *= Prime3;
			mixedHash ^= mixedHash >> 16;
			return (int)mixedHash;
		}
#endif

		/// <inheritdoc />
		public override string ToString()
			=> $"IsMatch: {IsMatch}   Source: [{SourceBeginningLineNumber}, {SourceEndingLineNumber})   Target: [{TargetBeginningLineNumber}, {TargetEndingLineNumber})";
	}
}
