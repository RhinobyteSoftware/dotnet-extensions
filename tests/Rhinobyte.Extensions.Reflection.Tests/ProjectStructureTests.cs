using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.Tests
{
	[TestClass]
	public class ProjectStructureTests
	{
		[TestMethod]
		public void Library_types_all_match_one_of_the_valid_namespaces()
		{
			// Even though I divide the files into subfolder for slightly easier organization I want them all to use the same Rhinobyte.Extensions.DependencyInjection
			// root namespace. Verify that I didn't forget to adjust it if I add new types to one of the subfolders.
			var libraryTypes = typeof(Rhinobyte.Extensions.Reflection.TypeExtensions).Assembly.GetTypes();
			var validNamespaces = new[]
			{
				"Rhinobyte.Extensions.Reflection",
				"Rhinobyte.Extensions.Reflection.AssemblyScanning",
				"Rhinobyte.Extensions.Reflection.IntermediateLanguage"
			};

			var invalidTypes = new List<string>();
			foreach (var libraryType in libraryTypes)
			{
				if (libraryType.IsCompilerGenerated())
					continue;


				var fullTypeName = libraryType?.FullName;
				if (fullTypeName == null)
					continue;

				var lastDotIndex = fullTypeName.LastIndexOf('.');
				if (lastDotIndex == -1)
					continue;

				var typeNamespace = fullTypeName.Substring(0, lastDotIndex);

				if (!validNamespaces.Contains(typeNamespace) && typeNamespace?.StartsWith("Coverlet.Core.Instrumentation") != true)
					invalidTypes.Add(fullTypeName);
			}

			if (invalidTypes.Count > 0)
				throw new AssertFailedException($"The following types have an incorrect namespace:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, invalidTypes)}");
		}

		[TestMethod]
		public void TestMethods_are_not_be_missing_the_TestMethod_attribute()
		{
			var discoveredTestTypes = typeof(ProjectStructureTests).Assembly.GetTypes();

			var missingTestMethodAttributes = new List<string>();
			foreach (var testType in discoveredTestTypes)
			{
				if (testType.IsCompilerGenerated() || !testType.IsDefined(typeof(TestClassAttribute), false))
					continue;

				var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				foreach (var testMethod in testMethods)
				{
					if (testMethod.IsDefined(typeof(TestMethodAttribute), true))
						continue;

					missingTestMethodAttributes.Add($"{testType.Name}.{testMethod.Name}");
				}
			}

			if (missingTestMethodAttributes.Count > 0)
				throw new AssertFailedException($"The following methods do not have a [TestMethod] attribute:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, missingTestMethodAttributes)}");
		}
	}
}
