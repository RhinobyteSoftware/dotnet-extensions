namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// A filter that can be applied to an <see cref="AssemblyScanner"/> in order to ignore specific assemblies during scanning.
	/// </summary>
	public interface IScannedAssemblyFilter
	{
		bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult);
	}
}
