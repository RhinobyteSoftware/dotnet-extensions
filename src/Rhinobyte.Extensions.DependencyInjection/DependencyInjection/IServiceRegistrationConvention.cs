using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface IServiceRegistrationConvention
	{
		bool HandleType(Type discoveredType, IAssemblyScanResult scanResult, ServiceRegistrationCache serviceCollection);
	}
}
