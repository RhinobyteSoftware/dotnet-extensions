# Rhinobyte.Extensions.Reflection

This repo contains the code to build the `Rhinobyte.Extensions.Reflection` library for .Net

## Nuget Package

[![NuGet version (Rhinobyte.Extensions.Reflection)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.Reflection.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.Reflection/)


## Assembly Scanning Extensions

The assembly scanning features contain helper types and methods for convenient re-usable code for discovering from a set of assemblies. 
The AssemblyScanner type supports a variety of filtering features and will return a scan result that can be used for efficient lookup of discovered
types within given criteria like interface types or concrete types.

See the [Rhinobyte.Extensions.DependencyInjection](/../Rhinobyte.Extensions.DependencyInjection/README.md) library for extensions that leverage the
assembly scanning types to automatically register types against an `IServiceCollection` instance.

## Intermediate Langauge Extensions

The IL (intermediate language) features provide helper types extension methods for parsing the IL bytes from a method body into a collection of strongly
typed instruction objects. The parsed instructions can then be leveraged for advanced reflection logic such as determining if a specific member is
referenced within the body of a target method.

Convenience types and extension methods to output a concise human readable description of the IL instructions are also provided.

## Other Extensions

Extension methods for rendering type name and method signature information to a string that is more human readable and more closely resembles an actual code signature.

Type information convenience methods such as `type.IsCompilerGenerated()` and `type.IsOpenGeneric()`.


## Usage Examples

#### Assembly Scanner Example Usage:
```
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System.Reflection;

public namespace ExampleLibrary
{
    public interface ISomething { }

    public class Something : ISomething { }

    public class SomethingToIgnore1 { }

    public class SomethingToIgnore2 { }

    // Attribute support for excluding types from the assembly scanning result
    [IgnoreAssemblyScanner]
    public class SomethingWeDontWantScanned1 { }

    public class SomethingWeDontWantScanned2 { }

    public class SomethingWeDontWantScanned3 { }

    public class SomethingWeDontWantScanned4 { }

    public class SomeService
    {
        public void DoSomethingWithReflection()
        {
            var assemblyScanner = AssemblyScanner.CreateDefault()
                .AddForType<SomeService>() // Add the ExampleLibrary assembly to the list of assemblies to scan
                .Add(typeof(SomeOtherLibrary).Assembly)
                .ExcludeType<SomethingWeDontWantScanned2>()
                .ExcludeTypes(new [] { typeof(SomethingWeDontWantScanned3), typeof(SomethingWeDontWantScanned4) })
                .AddTypeFilter((assemblyInclude, discoveredType, scanner, currentScanResult) =>
                {
                    // Return true to ignore type names containing ToIgnore
                    return discoveredType.Name.Contains("ToIgnore");
                });

            var scanResult = assemblyScanner.ScanAssemblies();

            // Iterate through all of the discovered, non-ignored types
            foreach (var type in scanResult.AllDiscoveredTypes) 
            {
                // ... something ...
            }

            // Iterate through just the interface types
            foreach (var interfaceType in scanResult.InterfaceTypes)
            {
                var implementations = scanResult.ConcreteTypes.Where(concreteType => interfaceType.IsAssignableFrom(concreteType)).ToList();
            }
        }
    }
}
```

#### IL Method Body Parsing Example Usage:

```
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System.Reflection;
```

