namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface IScannedAssemblyFilter
	{
		bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult);
	}
}
