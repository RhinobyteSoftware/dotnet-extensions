# Rhinobyte.Extensions.DependencyInjection

[![NuGet version (Rhinobyte.Extensions.DependencyInjection)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.DependencyInjection.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.DependencyInjection/)

This library contains assembly scanning and convention based registration extensions for the out-of-the-box .NET dependency injection libraries.

## Usage Examples

#### Register Types Example
```cs
// ExampleLibrary
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;

namespace ExampleLibrary
{
	public static class DependencyInjectionExtensions
	{
		public static AssemblyScanner AddExampleLibrary(this AssemblyScanner assemblyScanner)
		{
			_ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));
			return assemblyScanner
				.Add(typeof(DependencyInjectionExtensions).Assembly)
				.ExcludeType<IManuallyConfiguredType>()
				.ExcludeType<ManuallyConfiguredType>()
		}

		public static IServiceCollection AddExampleLibrary(this IServiceCollection services, AssemblyScanner assemblyScanner)
		{
			_ = services ?? throw new ArgumentNullException(nameof(services));
			_ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));

			// Manually configure this as a singleton
			services.AddSingleton<IManuallyConfiguredType>(new ManuallyConfiguredType(new Uri("https://api.fake.com")));

            services.AddDbContext<IExampleDbContext, ExampleDbContext>(dbContextOptionsBuilder => {
                /** db context configuration **/
            });

			assemblyScanner.AddExampleLibrary();
			services = services
				.RegisterAttributeDecoratedTypes(assemblyScanner)
				.RegisterInterfaceImplementations(assemblyScanner, InterfaceImplementationResolutionStrategy.DefaultConventionOrAll);

			return services;
		}

		public static IServiceCollection AddExampleLibrary(this IServiceCollection services)
			=> AddExampleLibrary(services, AssemblyScanner.CreateDefault());
	}
}

// ExampleProgram
using ExampleLibrary;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleProgram
{
    class Program
    {
        public static async Task<int> Main(string[] args) 
        {
            // The extensions are designed to function 100% while using the 
            // OOTB Microsoft.Extensions.DependencyInjection types and methods.
            // There is no need for constructing a custom 'service collection type' and no need
            // to use a custom container in lieu of the OOTB BuildServiceProvider()
            using var serviceProvider = new ServiceCollection()
                .AddExampleLibrary()
                .BuildServiceProvider();

            var somethingService = serviceProvider.GetRequiredService<ISomethingService>();
        }
    }
}

// ExampleProgram2
using ExampleLibrary;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleProgram2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services = services
                .AddControllers()
                .AddExampleLibrary()
                .AddSwaggerGen(c => /** swagger configuration **/);

            /** additional configuration **/
        }
    }
}

// ExamleLibrary.Tests
using ExampleLibrary;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;

namespace ExampleLibrary.Tests
{
    public TestContext TestContext { get; set; }

    [TestClass]
    public class SomethingServiceTests
    {
        [TestMethod]
        public async Task DoSomethingAsync_meets_some_criteria()
        {
            using var serviceProvider = new ServiceCollection()
                .AddExampleLibrary()
                .UseStubSomethingOptions()
                .BuildServiceProvider();

            var systemUnderTest = serviceProvider.GetRequiredService<ISomethingService>();
            var result = await systemUnderTest.DoSomethingAsync(TestContext.CancellationTokenSource.Token);
            result.Should().MeetSomeCriteria();
        }

        [TestMethod]
        public async Task DoSomethingElseAsync_meets_some_criteria()
        {
            using var serviceProvider = new ServiceCollection()
                .AddExampleLibraryWithStubs()
                .BuildServiceProvider();

            var systemUnderTest = serviceProvider.GetRequiredService<ISomethingService>();
            var result = await systemUnderTest.DoSomethingElseAsync(TestContext.CancellationTokenSource.Token);
            result.Should().MeetSomeCriteria();
        }
    }

    public class StubDependency : IDependency
    {
        /** Stub Code For Testing **/
    }

    public class StubSomethingOptions : ISomethingOptions
    { 
        /** Stub Code For Testing **/
    }

    public static class TestDependencyInjectionExtensions
    {
        // Replace a single registration that AddExampleLibrary() adds
        public static IServiceCollection UseStubSomethingOptions(this IServiceCollection serviceCollection)
            => serviceCollection
                .RemoveAll<ISomethingOptions>()
                .AddScoped<ISomethingOptions, StubSomethingOptions>();

        // Customize the behavior of AddExampleLibrary() using our own type filters
        public static IServiceCollection AddExampleLibraryWithStubs(this IServiceCollection serviceCollection)
        {
            var exampleLibraryTestsAssembly = typeof(StubSomethingOptions).Assembly;
            var exampleLibraryStubTypes = exampleLibraryTestsAssembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.Name.StartsWith("Stub"));

            var customizedAssemblyScanner = AssemblyScanner.CreateDefault()
                .AddExampleLibrary()
                .Add(exampleLibraryTestsAssembly)
                .AddTypeFilter((assemblyInclude, discoveredType, scanner, currentScanResult) =>
                {
                    // Return true to ignore concrete types from ExampleLibrary when we have
                    // a stub type to use for it instead...
                    return assemblyInclude != exampleLibraryTestsAssembly
                        && discoveredType.IsClass
                        && !discoveredType.IsAbstract
                        && exampleLibraryStubTypes.Any(stubType => stubType.Name == $"Stub{discoveredType.Name}")
                });

            return serviceCollection.AddExampleLibrary(customizedAssemblyScanner);
        }
    }
}

```

#### Custom Registration Convention Example

```cs
// SomeCustomConvention1
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhinobyte.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SomeLibrary
{
    public class SomeCustomConvention1 : IServiceRegistrationConvention
    {
        public bool HandleType(
            Type discoveredType,
			IAssemblyScanResult scanResult,
			ServiceRegistrationCache serviceRegistrationCache)
		{
            if (!MeetsOurConventionCriteria(discoveredType))
                return false;

            var implementationType = scanResult.ConcreteTypes
                .FirstOrDefault(concreteType => MeetsOurImplementationTypeCriteria(discoveredType, concreteType));

            // ServiceRegistrationCache has some convenience methods to check for existing registrations
            // by serviceType, implementationType, or both
            if (serviceRegistrationCache.HasAnyByImplementationType(implementationType))
                return false;

            // Other than that, it is a normal (wrapper) implementation of IServiceCollection so any of the
            // normal methods or extension methods can be used
            serviceRegistrationCache.AddScoped(discoveredType, implementationType);
        }

        public bool MeetsOurConventionCriteria(Type discoveredType)
        {
            /** convention criteria check **/
        }

        public bool MeetsOurImplementationTypeCriteria(Type discoveredType, Type implementationType) 
        {
            /** implementation type check **/
        }
    }

    public class SomeCustomConvention2 : ServiceRegistrationConventionBase
    {
        public SomeCustomConvention2()
            : base(
                defaultLifetime: ServiceLifetime.Singleton, 
                defaultOverwriteBehavior: ServiceRegistrationType.ReplaceAll,
                skipImplementationTypesAlreadyInUse: false
            )
        { }

        public override ServiceRegistrationParameters? GetServiceRegistrationParameters(
			Type discoveredType,
			IAssemblyScanResult scanResult,
			ServiceRegistrationCache serviceRegistrationCache)
		{
            // ServiceRegistrationConventionBase will handle most of the registration behavior
            // e.g. Add vs TryAdd vs ReplaceAll
            // Subclasses can simply override this method to provide the implementation selection behavior
            if (!typeof(FluentValidation.AbstractValidator<>).IsAssignableFrom(discoveredType) || discoveredType.IsAbstract)
                return null;

            var serviceDescriptor = BuildServiceDescriptor(discoveredType, discoveredType, serviceRegistrationCache);
            return serviceDescriptor is null
                ? null
                : new ServiceRegistrationParameters(serviceDescriptor);
        }
    }

    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection RegisterUsingOurCustomConvention1(
            this IServiceCollection serviceCollection,
            IAssemblyScanner assemblyScanner)
        {
            _ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));
            return RegisterUsingOurCustomConvention1(serviceCollection, assemblyScanner.ScanAssemblies());
        }

        public static IServiceCollection RegisterUsingOurCustomConvention1(
            this IServiceCollection serviceCollection,
            IAssemblyScanResult assemblyScanResult)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _ = assemblyScanResult ?? throw new ArgumentNullException(nameof(assemblyScanResult));

            // RegisterTypes will wrap IServiceCollection in an ServiceRegistrationCache, if not
            // already wrapped. Return that as the result so subsequent fluent / chained calls 
            // don't need to wrap it again
            return serviceCollection.RegisterTypes(serviceCollection, assemblyScanResult, new SomeCustomConvention1());
        }

        public static IServiceCollection RegisterUsingOurCustomConvention2(
            this IServiceCollection serviceCollection,
            IAssemblyScanner assemblyScanner)
        {
            _ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));
            return RegisterUsingOurCustomConvention2(serviceCollection, assemblyScanner.ScanAssemblies());
        }

        public static IServiceCollection RegisterUsingOurCustomConvention2(
            this IServiceCollection serviceCollection,
            IAssemblyScanResult assemblyScanResult)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _ = assemblyScanResult ?? throw new ArgumentNullException(nameof(assemblyScanResult));

            // RegisterTypes will wrap IServiceCollection in an ServiceRegistrationCache, if not
            // already wrapped. Return that as the result so subsequent fluent / chained calls 
            // don't need to wrap it again
            return serviceCollection.RegisterTypes(serviceCollection, assemblyScanResult, new SomeCustomConvention2());
        }

        public static IServiceCollection RegisterUsingOurCustomConventions(
            this IServiceCollection serviceCollection,
            IAssemblyScanner assemblyScanner)
        {
            _ = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));
            return RegisterUsingOurCustomConventions(serviceCollection, assemblyScanner.ScanAssemblies());
        }

        public static IServiceCollection RegisterUsingOurCustomConventions(
            this IServiceCollection serviceCollection,
            IAssemblyScanResult assemblyScanResult)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _ = assemblyScanResult ?? throw new ArgumentNullException(nameof(assemblyScanResult));

            var ourConventions = new AggregateConvention(
                new IServiceRegistrationConvention[] { new SomeCustomConvention1(), new SomeCustomConvention2() });

            return serviceCollection.RegisterTypes(serviceCollection, assemblyScanResult, ourConventions);
        }
    }
}
```