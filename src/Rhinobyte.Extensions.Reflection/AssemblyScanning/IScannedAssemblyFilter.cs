namespace Rhinobyte.Extensions.Reflection.AssemblyScanning;

/// <summary>
/// A filter that can be applied to an <see cref="AssemblyScanner"/> in order to ignore specific assemblies during scanning.
/// </summary>
public interface IScannedAssemblyFilter
{
	/// <summary>
	/// The filter method that will be applied during the scan call. If this method returns true the <paramref name="assemblyInclude"/>
	/// will be ignored.
	/// </summary>
	/// <param name="assemblyInclude">The <see cref="AssemblyInclude"/> to consider for filtering</param>
	/// <param name="scanner">The <see cref="IAssemblyScanner"/> performing the scan</param>
	/// <param name="scanResult">The <see cref="IAssemblyScanResult"/> that is being populated</param>
	/// <returns>true if <paramref name="assemblyInclude"/> should be ignored by the scan, false otherwise</returns>
	bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult);
}
