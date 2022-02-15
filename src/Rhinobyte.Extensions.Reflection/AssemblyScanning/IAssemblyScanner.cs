using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning;

/// <summary>
/// IAssemblyScanner type representing collections of configured assemblies, exclude/include types, and filters used to perform a scan
/// and produce an <see cref="IAssemblyScanResult"/>
/// </summary>
public interface IAssemblyScanner
{
	/// <summary>
	/// The collection of assemblies that have been registered against the scanner.
	/// </summary>
	IReadOnlyCollection<AssemblyInclude> AssembliesToScan { get; }
	/// <summary>
	/// The collection of explicit types to exclude from scanning.
	/// </summary>
	IReadOnlyCollection<Type> ExplicitTypeExcludes { get; }
	/// <summary>
	/// The collection of explicit types to include in scanning.
	/// </summary>
	IReadOnlyCollection<Type> ExplicitTypeIncludes { get; }
	/// <summary>
	/// The collection of <see cref="IScannedAssemblyFilter"/> items to apply against the <see cref="AssembliesToScan"/> during a scan.
	/// </summary>
	IReadOnlyCollection<IScannedAssemblyFilter> ScannedAssemblyFilters { get; }
	/// <summary>
	/// The collection of <see cref="IScannedTypeFilter"/> items to apply against the discovered types during a scan.
	/// </summary>
	IReadOnlyCollection<IScannedTypeFilter> ScannedTypeFilters { get; }

	/// <summary>
	/// Executes an assembly scan against the registered items and returns the scan result.
	/// </summary>
	IAssemblyScanResult ScanAssemblies(
		IncludeExcludeConflictResolutionStrategy includeExcludeConflictResolutionStrategy = IncludeExcludeConflictResolutionStrategy.PrioritizeExcludes
	);
}
