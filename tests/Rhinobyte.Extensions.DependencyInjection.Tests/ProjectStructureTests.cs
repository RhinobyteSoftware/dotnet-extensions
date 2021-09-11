using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection.Tests
{
	[TestClass]
	public class ProjectStructureTests
	{
		[TestMethod]
		public void Library_types_all_use_the_root_namespace()
		{
			// Even though I divide the files into subfolder for slightly easier organization I want them all to use the same Rhinobyte.Extensions.DependencyInjection
			// root namespace. Verify that I didn't forget to adjust it if I add new types to one of the subfolders.
			var libraryTypes = typeof(Rhinobyte.Extensions.DependencyInjection.TypeExtensions).Assembly.GetTypes();

			var invalidTypes = new List<string>();
			foreach (var libraryType in libraryTypes)
			{
				var typeNamespace = libraryType?.FullName?.Substring(0, libraryType.FullName.LastIndexOf('.'));
				if (libraryType?.FullName != null && typeNamespace != "Rhinobyte.Extensions.DependencyInjection")
					invalidTypes.Add(libraryType.FullName);
			}

			if (invalidTypes.Count > 0)
				throw new AssertFailedException($"The following types have an incorrect namespace:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, invalidTypes)}");
		}
	}
}
