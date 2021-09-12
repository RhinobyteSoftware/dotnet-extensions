using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface IServiceRegistrationConvention
	{
		void HandleType(Type discoveredType, IAssemblyScanResult scanResult, IServiceCollection serviceCollection);
	}
}
