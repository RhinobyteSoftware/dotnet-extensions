using System;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// Implementation of <see cref="IScannedAssemblyFilter"/> that executes a constructor provided filter function
	/// </summary>
	public class LambdaAssemblyFilter : IScannedAssemblyFilter
	{
		private readonly Func<AssemblyInclude, IAssemblyScanner, IAssemblyScanResult, bool> _filter;

		public LambdaAssemblyFilter(Func<AssemblyInclude, IAssemblyScanner, IAssemblyScanResult, bool> filter)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		/// <inheritdoc/>
		public bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult)
			=> _filter.Invoke(assemblyInclude, scanner, scanResult);
	}
}
