using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface IAssemblyScanResult
	{
		IReadOnlyCollection<Type> AllDiscoveredTypes { get; }
		IReadOnlyCollection<Type> ConcreteTypes { get; }
		IReadOnlyCollection<AssemblyInclude> IgnoredAssemblies { get; }
		IReadOnlyCollection<Type> IgnoredTypes { get; }
		IReadOnlyCollection<Type> InterfaceTypes { get; }
		IReadOnlyCollection<Type> OpenGenericTypes { get; }
		IReadOnlyCollection<AssemblyInclude> ScannedAssemblies { get; }
	}
}
