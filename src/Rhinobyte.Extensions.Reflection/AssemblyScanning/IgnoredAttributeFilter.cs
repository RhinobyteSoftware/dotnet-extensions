using System;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// Filters out assemblies or types decorated with the <see cref="IgnoreAssemblyScannerAttribute"/>
	/// </summary>
	public class IgnoredAttributeFilter : IScannedAssemblyFilter, IScannedTypeFilter
	{
		public bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult)
			=> assemblyInclude.AssemblyToInclude.IsDefined(typeof(IgnoreAssemblyScannerAttribute), false);

		public bool ShouldIgnoreType(AssemblyInclude assemblyInclude, Type scannedType, IAssemblyScanner scanner, IAssemblyScanResult scanResult)
			=> scannedType?.IsDefined(typeof(IgnoreAssemblyScannerAttribute), false) ?? true;
	}
}
