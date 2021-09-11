using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Rhinobyte.Extensions.DependencyInjection.Tests
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
	}
}
