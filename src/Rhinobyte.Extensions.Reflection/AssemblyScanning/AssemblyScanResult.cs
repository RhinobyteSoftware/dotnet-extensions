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
		public HashSet<Type> AllDiscoveredTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.AllDiscoveredTypes => AllDiscoveredTypes;

		public HashSet<Type> ConcreteTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.ConcreteTypes => ConcreteTypes;

		public HashSet<AssemblyInclude> IgnoredAssemblies { get; } = new HashSet<AssemblyInclude>();
		IReadOnlyCollection<AssemblyInclude> IAssemblyScanResult.IgnoredAssemblies => IgnoredAssemblies;

		public HashSet<Type> IgnoredTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.IgnoredTypes => IgnoredTypes;

		public HashSet<Type> InterfaceTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.InterfaceTypes => InterfaceTypes;

		public HashSet<Type> OpenGenericTypes { get; } = new HashSet<Type>();
		IReadOnlyCollection<Type> IAssemblyScanResult.OpenGenericTypes => OpenGenericTypes;

		public HashSet<AssemblyInclude> ScannedAssemblies { get; } = new HashSet<AssemblyInclude>();
		IReadOnlyCollection<AssemblyInclude> IAssemblyScanResult.ScannedAssemblies => ScannedAssemblies;
	}
}
