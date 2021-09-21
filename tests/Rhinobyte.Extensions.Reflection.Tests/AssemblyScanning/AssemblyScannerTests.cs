using ExampleLibrary1;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System.Linq;

namespace Rhinobyte.Extensions.Reflection.Tests.AssemblyScanning
{
	[TestClass]
	public class AssemblyScannerTests
	{
		[TestMethod]
		public void AddAssembly_successfully_adds_the_assemblies()
		{
			var thisAssembly = typeof(AssemblyScannerTests).Assembly;

			var assemblyScanner = AssemblyScanner.CreateDefault();
			assemblyScanner.Add(thisAssembly);
			assemblyScanner.FindAssemblyInclude(thisAssembly).Should().NotBeNull();

			var ignoredAssembly = typeof(ExampleIgnoredAttributeAssembly.DummyClass).Assembly;
			assemblyScanner.Add(ignoredAssembly);
			assemblyScanner.FindAssemblyInclude(ignoredAssembly).Should().NotBeNull();
		}

		[TestMethod]
		public void CreateDefault_includes_the_ignored_attribute_filters()
		{
			var assemblyScanner = AssemblyScanner.CreateDefault();
			assemblyScanner.ScannedAssemblyFilters.Any(assemblyFilter => assemblyFilter.GetType() == typeof(IgnoredAttributeFilter)).Should().BeTrue();
			assemblyScanner.ScannedTypeFilters.Any(typeFilter => typeFilter.GetType() == typeof(IgnoredAttributeFilter)).Should().BeTrue();
		}

		[TestMethod]
		public void ScanAssemblies_lambda_assembly_filter_works_as_expected()
		{
			var assemblyScanner = AssemblyScanner
				.CreateDefault()
				.AddExampleLibrary1()
				.AddAssemblyFilter((assemblyInclude, scanner, currentScanResult) =>
				{
					// Return true to ignore the type
					return assemblyInclude.AssemblyToInclude.GetName().Name?.Contains("ExampleLibrary1") == true;
				});

			var exampleLibrary1Assembly = typeof(ISomethingOptions).Assembly;
			var exampleLibrary1AssemblyInclude = new AssemblyInclude(exampleLibrary1Assembly);

			var scanResult = assemblyScanner.ScanAssemblies();
			scanResult.IgnoredAssemblies.Should().Contain(exampleLibrary1AssemblyInclude);
			scanResult.ScannedAssemblies.Should().NotContain(exampleLibrary1AssemblyInclude);

			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(ISomethingService));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(SomethingService));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(AlternateSomethingService));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(SomethingService3));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(ISomethingOptions));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(SomethingOptions));
			scanResult.IgnoredTypes.Should().NotContain(typeof(ISomethingOptions));
			scanResult.IgnoredTypes.Should().NotContain(typeof(SomethingOptions));
		}

		[TestMethod]
		public void ScanAssemblies_lambda_type_filter_works_as_expected()
		{
			var assemblyScanner = AssemblyScanner
				.CreateDefault()
				.AddExampleLibrary1()
				.AddTypeFilter((assemblyInclude, discoveredType, scanner, currentScanResult) =>
				{
					// Return true to ignore the type
					return !discoveredType.Name.EndsWith("Service");
				});

			var scanResult = assemblyScanner.ScanAssemblies();
			scanResult.AllDiscoveredTypes.Should().Contain(typeof(ISomethingService));
			scanResult.AllDiscoveredTypes.Should().Contain(typeof(SomethingService));
			scanResult.AllDiscoveredTypes.Should().Contain(typeof(AlternateSomethingService));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(SomethingService3)); // Ignored since it ends in Service3, not Service
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(ISomethingOptions));
			scanResult.AllDiscoveredTypes.Should().NotContain(typeof(SomethingOptions));

			scanResult.IgnoredTypes.Should().Contain(typeof(ISomethingOptions));
			scanResult.IgnoredTypes.Should().Contain(typeof(SomethingOptions));

			scanResult.InterfaceTypes.Should().NotContain(typeof(ISomethingOptions));
			scanResult.ConcreteTypes.Should().NotContain(typeof(SomethingOptions));
		}



		[TestMethod]
		public void ScanAssemblies_populates_the_scan_result_collections()
		{
			var assemblyScanner = AssemblyScanner
				.CreateDefault()
				.AddExampleLibrary1();

			var scanResult = assemblyScanner.ScanAssemblies();
			scanResult.AllDiscoveredTypes.Should().Contain(typeof(ISomethingOptions));
			scanResult.InterfaceTypes.Should().Contain(typeof(ISomethingOptions));
			scanResult.ConcreteTypes.Should().NotContain(typeof(ISomethingOptions));

			scanResult.AllDiscoveredTypes.Should().Contain(typeof(SomethingOptions));
			scanResult.InterfaceTypes.Should().NotContain(typeof(SomethingOptions));
			scanResult.ConcreteTypes.Should().Contain(typeof(SomethingOptions));

			scanResult.AllDiscoveredTypes.Should().Contain(typeof(ISomeOpenGenericType<,>));
			scanResult.OpenGenericTypes.Should().Contain(typeof(ISomeOpenGenericType<,>));
			scanResult.InterfaceTypes.Should().Contain(typeof(ISomeOpenGenericType<,>));
			scanResult.ConcreteTypes.Should().NotContain(typeof(ISomeOpenGenericType<,>));

			scanResult.AllDiscoveredTypes.Should().Contain(typeof(PartiallyOpenGeneric<>));
			scanResult.OpenGenericTypes.Should().Contain(typeof(PartiallyOpenGeneric<>));
			scanResult.InterfaceTypes.Should().NotContain(typeof(PartiallyOpenGeneric<>));
			scanResult.ConcreteTypes.Should().Contain(typeof(PartiallyOpenGeneric<>));

			scanResult.AllDiscoveredTypes.Should().Contain(typeof(ClassThatClosedOpenGenericOne));
			scanResult.OpenGenericTypes.Should().NotContain(typeof(ClassThatClosedOpenGenericOne));
			scanResult.ConcreteTypes.Should().Contain(typeof(ClassThatClosedOpenGenericOne));

			scanResult.AllDiscoveredTypes.Should().Contain(typeof(ClassThatClosedOpenGenericTwo));
			scanResult.OpenGenericTypes.Should().NotContain(typeof(ClassThatClosedOpenGenericTwo));
			scanResult.ConcreteTypes.Should().Contain(typeof(ClassThatClosedOpenGenericTwo));
		}
	}
}
