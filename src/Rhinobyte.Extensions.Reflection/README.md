# Rhinobyte.Extensions.Reflection

[![NuGet version (Rhinobyte.Extensions.Reflection)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.Reflection.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.Reflection/)

This library contains extensions for .Net reflection including assembly scanning/type discovery extensions and support for parsing IL (intermediate-language) instructions.


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
```cs
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
```cs
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Something.SomeLegacyWinFormsApp.UnitTests
{
    [TestMethod]
    public class LegacyProjectTests
    {
        [TestMethod]
        public void Form_types_that_use_embedded_resources_can_resolve_the_resources_successfully()
        {
            var scanResult = AssemblyScanner.CreateDefault()
                .AddForType<Something.SomeLegacyWinFormsApp.Program>()
                .ScanAssemblies();

            var failedResources = new List<string>();
            foreach (var discoveredType in scanResult.ConcreteTypes)
            {
                if (!typeof(System.Windows.Forms.Form).IsAssignableFrom(discoveredType))
                    continue;
                
                var initializeComponentMethod = discoveredType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                initializeComponentMethod.Should().NotBeNull();

                initializeComponentMethodInstructions = initializeComponentMethod!.ParseInstructions();
                ComponentResourceManager? resourceManager = null;
                foreach (var ilInstruction in initializeComponentMethodInstructions)
                {
                    if (ilInstruction is not MethodReferenceInstruction methodReferenceInstruction)
                        continue;

                    if (!methodReferenceInstruction.MethodReference.DeclaringType.Name.Contains("ResourceManager")
                        || methodReferenceInstruction.MethodReference.Name != nameof(ComponentResourceManager.GetObject))
                        continue;

                    // Get the string instruction that serves as the parameter for the resources.GetObject(resourceName) call
                    var resourceNameInstruction = (StringInstruction)methodReferenceInstruction.PreviousInstruction!;
                    
                    if (resourceManager is null)
                        resourceManager = new ComponentResourceManager(discoveredType);

                    try
                    {
                        object resource = resourceManager.GetObject(resourceNameInstruction.Value);
                        resource.Should().NotBeNull();
                    }
                    catch (Exception)
                    {
                        failedResources.Add($"{discoveredType.FullName}.resource -> {resourceName}");
                    }
                }
            }

            if (failedResources.Count > 0)
                Assert.Fail($"The following resources we're not found. Make sure the resource file/item exists and that the Form namespace matches the .resx file/folder structure:{Environment.NewLine}{string.Join(Environment.NewLine, failedResources)}");
        }
    }
}
```

#### Misc Extension Examples
```cs
using FluentAssertions;
using Rhinobyte.Extensions.Reflection;
using System.Reflection;

namespace SomeNamespace
{
    [TestClass]
    public class ExtensionExamples
    {
        [TestMethod]
        public void GetSignature_example_test()
        {
            var tryGetSomethingMethod = typeof(SomeClass<>).GetMethods().FirstOrDefault(method => method.Name == "TryGetSomething");

            // .ToString() vs .GetSignature() extension method
            tryGetSomethingMethod.ToString().Should().Be(
            "System.Nullable`1[T] TryGetSomething()");

            tryGetSomethingMethod.GetSignature().Should().Be(
            "public T? TryGetSomething()");

            tryGetSomethingMethod.GetSignature(useFullTypeName: true).Should().Be(
            "public T? SomeNamespace.GenericStruct<T>.TryGetSomething() where T : System.IConvertible, struct");
        }
    }

    public struct GenericStruct<T>
        where T : struct, IConvertible
    {
        public T? TryGetSomething() { /** code **/ }
    }
}
```


