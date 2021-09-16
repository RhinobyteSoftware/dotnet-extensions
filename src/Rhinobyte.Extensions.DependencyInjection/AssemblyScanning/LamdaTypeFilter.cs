﻿using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Implementation of <see cref="IScannedTypeFilter"/> that executes a constructor provided filter function
	/// </summary>
	public class LamdaTypeFilter : IScannedTypeFilter
	{
		private readonly Func<AssemblyInclude, Type, IAssemblyScanner, IAssemblyScanResult, bool> _filter;

		public LamdaTypeFilter(Func<AssemblyInclude, Type, IAssemblyScanner, IAssemblyScanResult, bool> filter)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		public bool ShouldIgnoreType(AssemblyInclude assemblyInclude, Type scannedType, IAssemblyScanner scanner, IAssemblyScanResult scanResult)
			=> _filter.Invoke(assemblyInclude, scannedType, scanner, scanResult);
	}
}
