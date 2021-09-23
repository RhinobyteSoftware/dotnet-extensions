using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;

namespace Rhinobyte.Extensions.Reflection.Tests.AssemblyScanning
{
	[TestClass]
	public class IgnoredAttributeFilterTests
	{
		[TestMethod]
		public void ShouldIgnoreAssembly_returns_the_expected_result()
		{
			var assemblyScanner = AssemblyScanner.CreateDefault();
			var scanResult = new AssemblyScanResult();

			var ignoredAttributeFilter = new IgnoredAttributeFilter();

			// Non-ignored assembly should return false
			var thisAssembly = typeof(IgnoredAttributeFilterTests).Assembly;
			var thisAssemblyInclude = new AssemblyInclude(thisAssembly);
			ignoredAttributeFilter.ShouldIgnoreAssembly(thisAssemblyInclude, assemblyScanner, scanResult).Should().BeFalse();

			// Ignored assembly should return true
			var ignoredAssembly = typeof(ExampleIgnoredAttributeAssembly.DummyClass).Assembly;
			var ignoredAssemblyInclude = new AssemblyInclude(ignoredAssembly);
			ignoredAttributeFilter.ShouldIgnoreAssembly(ignoredAssemblyInclude, assemblyScanner, scanResult).Should().BeTrue();
		}

		[TestMethod]
		public void ShouldIgnoreType_returns_the_expected_result()
		{
			var assemblyScanner = AssemblyScanner.CreateDefault();
			var scanResult = new AssemblyScanResult();

			var ignoredAttributeFilter = new IgnoredAttributeFilter();

			// Non-ignored type should return false
			var thisType = typeof(IgnoredAttributeFilterTests);
			var thisAssemblyInclude = new AssemblyInclude(thisType.Assembly);
			ignoredAttributeFilter.ShouldIgnoreType(thisAssemblyInclude, thisType, assemblyScanner, scanResult).Should().BeFalse();

			// Ignored type should return true
			var ignoredDummyType = typeof(ExampleIgnoredAttributeAssembly.DummyClass);
			var ignoredAssemblyInclude = new AssemblyInclude(ignoredDummyType.Assembly);
			ignoredAttributeFilter.ShouldIgnoreType(ignoredAssemblyInclude, ignoredDummyType, assemblyScanner, scanResult).Should().BeTrue();

			ignoredAttributeFilter.ShouldIgnoreType(thisAssemblyInclude, null!, assemblyScanner, scanResult).Should().BeTrue();
		}
	}
}
