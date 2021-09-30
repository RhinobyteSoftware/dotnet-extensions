using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// An assembly scan result
	/// </summary>
	public interface IAssemblyScanResult
	{
		/// <summary>
		/// The collection of types discovered by the assembly scan. 
		/// </summary>
		IReadOnlyCollection<Type> AllDiscoveredTypes { get; }
		/// <summary>
		/// The subset of concrete class types discovered by the assembly scan. 
		/// </summary>
		IReadOnlyCollection<Type> ConcreteTypes { get; }
		/// <summary>
		/// The assemblies registered against the <see cref="IAssemblyScanner"/> that are flagged as ignored by one of the <see cref="IAssemblyScanner.ScannedAssemblyFilters"/>
		/// </summary>
		IReadOnlyCollection<AssemblyInclude> IgnoredAssemblies { get; }
		/// <summary>
		/// The types discovered during the assembly scan that are flagged as ignored by one of the <see cref="IAssemblyScanner.ScannedTypeFilters"/>
		/// or by the types presence in the <see cref="IAssemblyScanner.ExplicitTypeExcludes"/> collection.
		/// </summary>
		IReadOnlyCollection<Type> IgnoredTypes { get; }
		/// <summary>
		/// The subset of interface types discovered by the assembly scan. 
		/// </summary>
		IReadOnlyCollection<Type> InterfaceTypes { get; }
		/// <summary>
		/// The subset of open generic types discovered by the assembly scan. 
		/// </summary>
		IReadOnlyCollection<Type> OpenGenericTypes { get; }
		/// <summary>
		/// The assemblies registered against the <see cref="IAssemblyScanner"/> whose types are included.
		/// <para>Assemblies that are flagged as ignored by any of the <see cref="IAssemblyScanner.ScannedAssemblyFilters"/> will be placed in the <see cref="IgnoredAssemblies"/> collection instead.</para>
		/// </summary>
		IReadOnlyCollection<AssemblyInclude> ScannedAssemblies { get; }
	}
}
