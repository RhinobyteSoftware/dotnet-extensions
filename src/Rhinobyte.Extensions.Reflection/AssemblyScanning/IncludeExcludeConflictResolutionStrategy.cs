namespace Rhinobyte.Extensions.Reflection.AssemblyScanning;

/// <summary>
/// The resolution strategy for scenarios where an <see cref="IAssemblyScanner"/> has the same type in both
/// the <see cref="IAssemblyScanner.ExplicitTypeExcludes"/> and the <see cref="IAssemblyScanner.ExplicitTypeIncludes"/> collections.
/// </summary>
public enum IncludeExcludeConflictResolutionStrategy
{
	/// <summary>
	/// Resolution strategy that will result in an exception being thrown if the explicit exclude/include collections overlap.
	/// </summary>
	ThrowException = 0,
	/// <summary>
	/// Resolution strategy that will result in the types being excluded from the scan if they exist in both the exclude and include collections.
	/// </summary>
	PrioritizeExcludes = 1,
	/// <summary>
	/// Resolution strategy that will result in the types being included in the scan if they exist in both the exclude and include collections.
	/// </summary>
	PrioritizeIncludes = 2
}
