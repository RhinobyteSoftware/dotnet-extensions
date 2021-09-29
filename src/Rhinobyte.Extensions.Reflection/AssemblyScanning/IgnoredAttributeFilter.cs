using System;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// Filters out assemblies or types decorated with the <see cref="IgnoreAssemblyScannerAttribute"/>
	/// </summary>
	public class IgnoredAttributeFilter : IScannedAssemblyFilter, IScannedTypeFilter
	{
		/// <summary>
		/// Implementation of <see cref="IScannedAssemblyFilter.ShouldIgnoreAssembly(AssemblyInclude, IAssemblyScanner, IAssemblyScanResult)"/> that
		/// returns true when the assembly is decorated an <see cref="IgnoreAssemblyScannerAttribute"/>.
		/// </summary>
		public bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult)
			=> assemblyInclude.AssemblyToInclude.IsDefined(typeof(IgnoreAssemblyScannerAttribute), false);

		/// <summary>
		/// Implementation of <see cref="IScannedTypeFilter.ShouldIgnoreType(AssemblyInclude, Type, IAssemblyScanner, IAssemblyScanResult)"/>
		/// that returns true when the <paramref name="scannedType"/> is decorated with an <see cref="IgnoreAssemblyScannerAttribute"/>.
		/// </summary>
		public bool ShouldIgnoreType(AssemblyInclude assemblyInclude, Type scannedType, IAssemblyScanner scanner, IAssemblyScanResult scanResult)
			=> scannedType?.IsDefined(typeof(IgnoreAssemblyScannerAttribute), false) ?? true;
	}
}
