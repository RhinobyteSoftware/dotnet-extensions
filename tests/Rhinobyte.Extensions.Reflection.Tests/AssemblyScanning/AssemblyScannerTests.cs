using ExampleLibrary1;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Linq;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.AssemblyScanning;

[TestClass]
public class AssemblyScannerTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void AddAssembly_successfully_adds_the_assemblies()
	{
		var thisAssembly = typeof(AssemblyScannerTests).Assembly;

		var assemblyScanner = AssemblyScanner.CreateDefault();

		assemblyScanner.Add(null!);
		assemblyScanner.AssembliesToScan.Should().BeEmpty();

		assemblyScanner.Add(thisAssembly);
		assemblyScanner.FindAssemblyInclude(thisAssembly).Should().NotBeNull();
		assemblyScanner.AssembliesToScan.Should().Contain(new AssemblyInclude(thisAssembly));

		var ignoredAssembly = typeof(ExampleIgnoredAttributeAssembly.DummyClass).Assembly;
		assemblyScanner.Add(ignoredAssembly);
		assemblyScanner.FindAssemblyInclude(ignoredAssembly).Should().NotBeNull();
		assemblyScanner.AssembliesToScan.Should().Contain(new AssemblyInclude(ignoredAssembly));
	}

	[TestMethod]
	public void AddAssembly_with_non_exported_types_included_does_not_capture_compiler_generated_types()
	{
		var reflectionAssembly = typeof(Rhinobyte.Extensions.Reflection.MethodBaseExtensions).Assembly;
		var compilerGeneratedTypes = reflectionAssembly.GetTypes().Where(type => type.IsCompilerGenerated()).ToArray();
		compilerGeneratedTypes.Should().NotBeEmpty();

		var assemblyScanner = AssemblyScanner.CreateDefault();
		assemblyScanner.Add(reflectionAssembly, areNonExportedTypesIncluded: true);

		var scanResult = assemblyScanner.ScanAssemblies();
		scanResult.AllDiscoveredTypes.Should().NotContain(compilerGeneratedTypes);
		scanResult.IgnoredTypes.Should().NotContain(compilerGeneratedTypes);
	}

	[TestMethod]
	public void AddAssemblyFilter_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();
		assemblyScanner.AddAssemblyFilter(scannedAssemblyFilter: null!).Should().Be(assemblyScanner);
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(1);

		assemblyScanner.AddAssemblyFilter(new DummyFilter()).Should().Be(assemblyScanner);
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(2);
	}

	[TestMethod]
	public void AddForType_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();

		assemblyScanner.AssembliesToScan.Should().BeEmpty();
		assemblyScanner.AddForType(null!);
		assemblyScanner.AssembliesToScan.Should().BeEmpty();

		assemblyScanner.AddForType<ISomethingOptions>();

		assemblyScanner.AssembliesToScan.Should().Contain(new AssemblyInclude(typeof(ISomethingOptions).Assembly));
	}

	[TestMethod]
	public void AddTypeFilter_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();
		assemblyScanner.AddTypeFilter(scannedTypeFilter: null!).Should().Be(assemblyScanner);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(1);

		assemblyScanner.AddTypeFilter(new DummyFilter()).Should().Be(assemblyScanner);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(2);
	}

	[TestMethod]
	public void Constructor_throws_if_required_arguments_are_null()
	{
		Invoking(() => new AssemblyScanner(null!, new HashSet<IScannedTypeFilter>()))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scannedAssemblyFilters*");

		Invoking(() => new AssemblyScanner(new HashSet<IScannedAssemblyFilter>(), null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scannedTypeFilters*");

		var assemblyScanner = new AssemblyScanner(new HashSet<IScannedAssemblyFilter>(), new HashSet<IScannedTypeFilter>());
		assemblyScanner.Should().NotBeNull();
	}

	[TestMethod]
	public void CreateDefault_includes_the_ignored_attribute_filters()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();
		assemblyScanner.ScannedAssemblyFilters.Any(assemblyFilter => assemblyFilter.GetType() == typeof(IgnoredAttributeFilter)).Should().BeTrue();
		assemblyScanner.ScannedTypeFilters.Any(typeFilter => typeFilter.GetType() == typeof(IgnoredAttributeFilter)).Should().BeTrue();
	}

	[TestMethod]
	public void ExcludeTypes_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1();

		assemblyScanner.ExcludeTypes(new[] { typeof(ISomethingOptions), typeof(SomethingOptions) });
		assemblyScanner.ExplicitTypeExcludes.Should().Contain(typeof(ISomethingOptions));
		assemblyScanner.ExplicitTypeExcludes.Should().Contain(typeof(SomethingOptions));

		var scanResult = assemblyScanner.ScanAssemblies();
		scanResult.AllDiscoveredTypes.Should().NotContain(typeof(ISomethingOptions));
		scanResult.AllDiscoveredTypes.Should().NotContain(typeof(SomethingOptions));

		scanResult.IgnoredTypes.Should().Contain(typeof(ISomethingOptions));
		scanResult.IgnoredTypes.Should().Contain(typeof(SomethingOptions));
	}

	[TestMethod]
	public void IncludeTypes_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();

		assemblyScanner.IncludeTypes(new[] { typeof(ISomethingOptions), typeof(SomethingOptions) });
		assemblyScanner.ExplicitTypeIncludes.Should().Contain(typeof(ISomethingOptions));
		assemblyScanner.ExplicitTypeIncludes.Should().Contain(typeof(SomethingOptions));

		var scanResult = assemblyScanner.ScanAssemblies();
		scanResult.AllDiscoveredTypes.Should().Contain(typeof(ISomethingOptions));
		scanResult.AllDiscoveredTypes.Should().Contain(typeof(SomethingOptions));

		scanResult.IgnoredAssemblies.Should().BeEmpty();
		scanResult.ScannedAssemblies.Should().BeEmpty();
	}

	[TestMethod]
	public void Remove_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();

		assemblyScanner.AddForType<ISomethingOptions>();
		assemblyScanner.AssembliesToScan.Count.Should().Be(1);
		assemblyScanner.AssembliesToScan.Should().Contain(new AssemblyInclude(typeof(ISomethingOptions).Assembly));

		assemblyScanner.Remove(assembly: null!).Should().Be(assemblyScanner);
		assemblyScanner.AssembliesToScan.Count.Should().Be(1);
		assemblyScanner.AssembliesToScan.Should().Contain(new AssemblyInclude(typeof(ISomethingOptions).Assembly));

		assemblyScanner.Remove(typeof(AssemblyScannerTests).Assembly).Should().Be(assemblyScanner);
		assemblyScanner.AssembliesToScan.Count.Should().Be(1);
		assemblyScanner.AssembliesToScan.Should().Contain(new AssemblyInclude(typeof(ISomethingOptions).Assembly));

		assemblyScanner.Remove(typeof(ISomethingOptions).Assembly);
		assemblyScanner.AssembliesToScan.Should().BeEmpty();
	}

	[TestMethod]
	public void RemoveAssemblyFilter_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(1);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(1);

		assemblyScanner.RemoveAssemblyFilter(null!).Should().Be(assemblyScanner);
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(1);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(1);

		var filterToRemove = assemblyScanner.ScannedAssemblyFilters.First();

		assemblyScanner.RemoveAssemblyFilter(filterToRemove);
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(0);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(1);
	}

	[TestMethod]
	public void RemoveTypeFilter_behaves_as_expected()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault();
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(1);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(1);

		assemblyScanner.RemoveTypeFilter(null!).Should().Be(assemblyScanner);
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(1);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(1);

		var filterToRemove = assemblyScanner.ScannedTypeFilters.First();

		assemblyScanner.RemoveTypeFilter(filterToRemove);
		assemblyScanner.ScannedAssemblyFilters.Count.Should().Be(1);
		assemblyScanner.ScannedTypeFilters.Count.Should().Be(0);
	}

	[TestMethod]
	public void ScanAssemblies_behaves_as_expected_for_each_IncludeExcludeConflictResolutionStrategy()
	{
		var assemblyScanner = AssemblyScanner.CreateDefault()
			.ExcludeType<ISomethingOptions>()
			.IncludeType<ISomethingOptions>();

		var scanResult = assemblyScanner.ScanAssemblies(IncludeExcludeConflictResolutionStrategy.PrioritizeExcludes);
		scanResult.AllDiscoveredTypes.Should().NotContain(typeof(ISomethingOptions));
		scanResult.IgnoredTypes.Should().Contain(typeof(ISomethingOptions));

		scanResult = assemblyScanner.ScanAssemblies(IncludeExcludeConflictResolutionStrategy.PrioritizeIncludes);
		scanResult.AllDiscoveredTypes.Should().Contain(typeof(ISomethingOptions));
		scanResult.IgnoredTypes.Should().NotContain(typeof(ISomethingOptions));

		Invoking(() => assemblyScanner.ScanAssemblies(IncludeExcludeConflictResolutionStrategy.ThrowException))
			.Should()
			.Throw<InvalidOperationException>();
	}

	[TestMethod]
	public void ScanAssemblies_ignores_compiler_generated_types()
	{
		var reflectionExtensionsAssembly = typeof(IAssemblyScanner).Assembly;
		var compilerGeneratedTypes = reflectionExtensionsAssembly.GetTypes().Where(type => type.IsCompilerGenerated()).ToList();
		compilerGeneratedTypes.Should().NotBeEmpty();

		var scanResult = AssemblyScanner.CreateDefault()
			.Add(reflectionExtensionsAssembly)
			.ScanAssemblies();

		scanResult.ScannedAssemblies.Should().Contain(new AssemblyInclude(reflectionExtensionsAssembly));
		scanResult.AllDiscoveredTypes.Any(type => compilerGeneratedTypes.Contains(type)).Should().BeFalse();
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

	[TestMethod]
	public void ScanAssemblies_should_reuse_the_scan_result_when_nothing_has_changed()
	{
		var assemblyScanner = AssemblyScanner
			.CreateDefault()
			.AddExampleLibrary1();

		var scanResult1 = assemblyScanner.ScanAssemblies();
		var scanResult2 = assemblyScanner.ScanAssemblies();
		scanResult1.Should().BeSameAs(scanResult2);

		assemblyScanner.ExcludeType<ISomethingOptions>();
		var scanResult3 = assemblyScanner.ScanAssemblies();
		scanResult3.Should().NotBeSameAs(scanResult2);
	}



	/******     TEST SETUP     *****************************
	 *******************************************************/
	public class DummyFilter : IScannedAssemblyFilter, IScannedTypeFilter
	{
		public bool ShouldIgnoreAssembly(AssemblyInclude assemblyInclude, IAssemblyScanner scanner, IAssemblyScanResult scanResult) => false;
		public bool ShouldIgnoreType(AssemblyInclude assemblyInclude, Type scannedType, IAssemblyScanner scanner, IAssemblyScanResult scanResult) => false;
	}
}
