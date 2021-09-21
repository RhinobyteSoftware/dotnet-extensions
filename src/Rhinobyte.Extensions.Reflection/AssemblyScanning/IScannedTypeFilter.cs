using System;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// A filter that can be applied to an <see cref="AssemblyScanner"/> in order to ignore specific types during scanning.
	/// </summary>
	public interface IScannedTypeFilter
	{
		bool ShouldIgnoreType(AssemblyInclude assemblyInclude, Type scannedType, IAssemblyScanner scanner, IAssemblyScanResult scanResult);
	}
}
