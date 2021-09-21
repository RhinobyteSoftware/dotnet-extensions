using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// IAssemblyScanner type representing collections of configured assemblies, exclude/include types, and filters used to perform a scan
	/// and produce an <see cref="IAssemblyScanResult"/>
	/// </summary>
	public interface IAssemblyScanner
	{
		IReadOnlyCollection<AssemblyInclude> AssembliesToScan { get; }
		IReadOnlyCollection<Type> ExplicitTypeExcludes { get; }
		IReadOnlyCollection<Type> ExplicitTypeIncludes { get; }
		IReadOnlyCollection<IScannedAssemblyFilter> ScannedAssemblyFilters { get; }
		IReadOnlyCollection<IScannedTypeFilter> ScannedTypeFilters { get; }

		IAssemblyScanResult ScanAssemblies(
			IncludeExcludeConflictResolutionStrategy includeExcludeConflictResolutionStrategy = IncludeExcludeConflictResolutionStrategy.PrioritizeExcludes
		);
	}
}
