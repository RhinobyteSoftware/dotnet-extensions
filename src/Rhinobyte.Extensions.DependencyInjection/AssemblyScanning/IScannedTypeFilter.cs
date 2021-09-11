using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface IScannedTypeFilter
	{
		bool ShouldIgnoreType(AssemblyInclude assemblyInclude, Type scannedType, IAssemblyScanner scanner, IAssemblyScanResult scanResult);
	}
}
