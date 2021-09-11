using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface IAssemblyScanner
	{
		IReadOnlyCollection<AssemblyInclude> AssembliesToScan { get; }
		IReadOnlyCollection<Type> ExplicitTypeExcludes { get; }
		IReadOnlyCollection<Type> ExplicitTypeIncludes { get; }
		IReadOnlyCollection<IScannedAssemblyFilter> ScannedAssemblyFilters { get; }
		IReadOnlyCollection<IScannedTypeFilter> ScannedTypeFilters { get; }

		AssemblyScanResult ScanAssemblies(
			IncludeExcludeConflictResolutionStrategy includeExcludeConflictResolutionStrategy = IncludeExcludeConflictResolutionStrategy.PrioritizeExcludes
		);
	}
}
