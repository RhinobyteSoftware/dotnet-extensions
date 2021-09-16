using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// An assembly scan result.
	/// <para>
	/// Consumed by the <see cref="RhinobyteServiceCollectionExtensions.RegisterTypes(Microsoft.Extensions.DependencyInjection.IServiceCollection, IAssemblyScanner, IServiceRegistrationConvention)" /> extension methods
	/// and by the <see cref="IServiceRegistrationConvention"/> types in order to register the discovered types against an <see cref="IServiceCollection"/> instance.
	/// </para>
	/// </summary>
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
