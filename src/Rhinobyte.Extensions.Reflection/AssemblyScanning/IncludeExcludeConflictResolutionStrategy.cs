namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// The resolution strategy for scenarios where an <see cref="IAssemblyScanner"/> has the same type in both
	/// the <see cref="IAssemblyScanner.ExplicitTypeExcludes"/> and the <see cref="IAssemblyScanner.ExplicitTypeIncludes"/> collections.
	/// </summary>
	public enum IncludeExcludeConflictResolutionStrategy
	{
		ThrowException = 0,
		PrioritizeExcludes = 1,
		PrioritizeIncludes = 2
	}
}
