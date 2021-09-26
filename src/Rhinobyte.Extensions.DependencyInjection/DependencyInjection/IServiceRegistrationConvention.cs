using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Contract for a convention that can handle service collection registration of types discovered via reflection
	/// </summary>
	public interface IServiceRegistrationConvention
	{
		bool HandleType(Type discoveredType, IAssemblyScanResult scanResult, ServiceRegistrationCache serviceCollection);
	}
}
