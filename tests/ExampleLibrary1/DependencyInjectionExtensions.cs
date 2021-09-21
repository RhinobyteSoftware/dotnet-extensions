using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;

namespace ExampleLibrary1
{
	public static class DependencyInjectionExtensions
	{
		public static AssemblyScanner AddExampleLibrary1(this AssemblyScanner assemblyScanner)
		{
			_ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));
			return assemblyScanner
				.Add(typeof(DependencyInjectionExtensions).Assembly)
				.ExcludeType<IManuallyConfiguredType>()
				.ExcludeType<ManuallyConfiguredType>();
		}

		public static IServiceCollection AddExampleLibrary1(this IServiceCollection services, AssemblyScanner assemblyScanner)
		{
			_ = services ?? throw new ArgumentNullException(nameof(services));
			_ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));

			// Manually configure this as a singleton
			services.AddSingleton<IManuallyConfiguredType>(new ManuallyConfiguredType(new Uri("https://api.fake.com")));

			assemblyScanner.AddExampleLibrary1();
			services
				.RegisterAttributeDecoratedTypes(assemblyScanner)
				.RegisterInterfaceImplementations(assemblyScanner, InterfaceImplementationResolutionStrategy.DefaultConventionOrAll);

			return services;
		}

		public static IServiceCollection AddExampleLibrary1(this IServiceCollection services)
			=> AddExampleLibrary1(services, AssemblyScanner.CreateDefault().AddExampleLibrary1());
	}
}
