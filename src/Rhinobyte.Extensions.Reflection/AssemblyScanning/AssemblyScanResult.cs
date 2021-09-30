using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// Concrete implementation of <see cref="IAssemblyScanResult"/>.
	/// <para>Implements the various collections using <see cref="HashSet{T}"/></para>
	/// </summary>
	public class AssemblyScanResult : IAssemblyScanResult
	{
		/// <summary>
		/// The collection of types discovered by the assembly scan. 
		/// </summary>
		public HashSet<Type> AllDiscoveredTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.AllDiscoveredTypes => AllDiscoveredTypes;

		/// <summary>
		/// The subset of concrete class types discovered by the assembly scan. 
		/// </summary>
		public HashSet<Type> ConcreteTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.ConcreteTypes => ConcreteTypes;

		/// <summary>
		/// The assemblies registered against the <see cref="IAssemblyScanner"/> that are flagged as ignored by one of the <see cref="IAssemblyScanner.ScannedAssemblyFilters"/>
		/// </summary>
		public HashSet<AssemblyInclude> IgnoredAssemblies { get; } = new HashSet<AssemblyInclude>();
		IReadOnlyCollection<AssemblyInclude> IAssemblyScanResult.IgnoredAssemblies => IgnoredAssemblies;

		/// <summary>
		/// The types discovered during the assembly scan that are flagged as ignored by one of the <see cref="IAssemblyScanner.ScannedTypeFilters"/>
		/// or by the types presence in the <see cref="IAssemblyScanner.ExplicitTypeExcludes"/> collection.
		/// </summary>
		public HashSet<Type> IgnoredTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.IgnoredTypes => IgnoredTypes;

		/// <summary>
		/// The subset of interface types discovered by the assembly scan. 
		/// </summary>
		public HashSet<Type> InterfaceTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.InterfaceTypes => InterfaceTypes;

		/// <summary>
		/// The subset of open generic types discovered by the assembly scan. 
		/// </summary>
		public HashSet<Type> OpenGenericTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.OpenGenericTypes => OpenGenericTypes;

		/// <summary>
		/// The assemblies registered against the <see cref="IAssemblyScanner"/> whose types are included.
		/// <para>Assemblies that are flagged as ignored by any of the <see cref="IAssemblyScanner.ScannedAssemblyFilters"/> will be placed in the <see cref="IgnoredAssemblies"/> collection instead.</para>
		/// </summary>
		public HashSet<AssemblyInclude> ScannedAssemblies { get; } = new HashSet<AssemblyInclude>();
		IReadOnlyCollection<AssemblyInclude> IAssemblyScanResult.ScannedAssemblies => ScannedAssemblies;
	}
}
