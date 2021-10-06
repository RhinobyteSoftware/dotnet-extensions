using FluentAssertions;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.TestTools.Assertions;
using Rhinobyte.Tools.ProjectStructureTests.Tasks;
using Rhinobyte.Tools.ProjectStructureTests.Tests.Setup;
using System.IO;

namespace Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks
{
	[TestClass]
	public class GenerateProjectStructureTestsTaskTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void Execute_does_not_generate_a_file_when_there_are_no_test_methods_to_create()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var outputFilePath = Path.Combine(generatedFilesOutputDirectory, "FileThatShouldNotGetCreated1.cs");

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				ClassLibraryNamespaceValidationChecks = null,
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "C#",
				OutputFile = new TaskItem(outputFilePath),
				ShouldGenerateMissingTestMethodAttributeTestCase = false
			};

			var mockBuildEngine = new MockBuildEngine();
			systemUnderTest.BuildEngine = mockBuildEngine;

			systemUnderTest.Execute().Should().BeTrue();
			File.Exists(outputFilePath).Should().BeFalse();

			mockBuildEngine.LogMessageEventRequests.Count.Should().Be(1);
			mockBuildEngine.LogMessageEventRequests[0].Message.Should().Be("GenerateProjectStructureTestsTask did not receive any configuration values that would result in the generation of the tests code file.");
		}

		[TestMethod]
		public void Execute_logs_and_returns_false_for_invalid_language_value()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "FakeLanguage",
				OutputDirectory = new TaskItem(generatedFilesOutputDirectory),
				ShouldGenerateMissingTestMethodAttributeTestCase = true
			};

			var mockBuildEngine = new MockBuildEngine();
			systemUnderTest.BuildEngine = mockBuildEngine;

			systemUnderTest.Execute().Should().BeFalse();
			mockBuildEngine.LogErrorEventRequests.Count.Should().Be(1);
			mockBuildEngine.LogErrorEventRequests[0].Message.Should().Be(@"ProjectStructureTests code for the language ""FakeLanguage"" could not be generated.  There is no CodeDom provider defined for the language.");
		}

		[TestMethod]
		public void Execute_logs_and_returns_false_for_invalid_property_configurations()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				ShouldGenerateMissingTestMethodAttributeTestCase = true
			};

			var mockBuildEngine = new MockBuildEngine();
			systemUnderTest.BuildEngine = mockBuildEngine;

			systemUnderTest.Execute().Should().BeFalse();
			mockBuildEngine.LogErrorEventRequests.Count.Should().Be(1);
			mockBuildEngine.LogErrorEventRequests[0].Message.Should().Be(@"""GenerateProjectStructureTestsTask"" received an invalid value for the ""Language"" parameter.");

			mockBuildEngine.LogErrorEventRequests.Clear();
			systemUnderTest.Language = "C#";

			systemUnderTest.Execute().Should().BeFalse();
			mockBuildEngine.LogErrorEventRequests.Count.Should().Be(1);
			mockBuildEngine.LogErrorEventRequests[0].Message.Should().Be(@"""GenerateProjectStructureTestsTask"" received a null value for both the ""OutputFile"" and the ""OutputDirectory"" task item parameters.");
		}

		[TestMethod]
		public void GenerateCode_logs_errors_for_invalid_namespace_validation_check_items()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "C#",
				OutputDirectory = new TaskItem(generatedFilesOutputDirectory),
				ShouldGenerateMissingTestMethodAttributeTestCase = true
			};

			var invalidNamespaceValidationItem1 = new TaskItem(itemSpec: string.Empty);
			var invalidNamespaceValidationItem2 = new TaskItem(itemSpec: typeof(GenerateProjectStructureTestsTask).FullName);
			var invalidNamespaceValidationItem3 = new TaskItem(itemSpec: typeof(GenerateProjectStructureTestsTask).FullName);
			invalidNamespaceValidationItem3.SetMetadata(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces, "; ;");
			systemUnderTest.ClassLibraryNamespaceValidationChecks = new[]
			{
				invalidNamespaceValidationItem1,
				invalidNamespaceValidationItem2,
				invalidNamespaceValidationItem3
			};

			var mockBuildEngine = new MockBuildEngine();
			systemUnderTest.BuildEngine = mockBuildEngine;

			var generatedCode = systemUnderTest.GenerateCodeFile(out var fileExtension);
			fileExtension.Should().BeNull();
			generatedCode.Should().BeNullOrEmpty();

			mockBuildEngine.LogErrorEventRequests.Count.Should().Be(3);
			mockBuildEngine.LogErrorEventRequests[0].Message.ShouldBeSameAs(@"The ClassLibraryNamespaceValidationChecks task item at index 0 has an invalid ItemSpec value. Make sure the xml item's ""Include"" attribute is specified correctly.");
			mockBuildEngine.LogErrorEventRequests[1].Message.ShouldBeSameAs("The ClassLibraryNamespaceValidationChecks task item at index 1 has an invalid or missing ValidNamespaces metadata item.\nItem:  <NamespaceValidationCheck Include=\"Rhinobyte.Tools.ProjectStructureTests.Tasks.GenerateProjectStructureTestsTask\" />\nMetadata: <ValidNamespacesCheck></ValidNamespacesCheck>");
			mockBuildEngine.LogErrorEventRequests[2].Message.ShouldBeSameAs("The ClassLibraryNamespaceValidationChecks task item at index 2 has an invalid or missing ValidNamespaces metadata item.\nItem:  <NamespaceValidationCheck Include=\"Rhinobyte.Tools.ProjectStructureTests.Tasks.GenerateProjectStructureTestsTask\" />\nMetadata: <ValidNamespacesCheck>; ;</ValidNamespacesCheck>");

			mockBuildEngine.LogErrorEventRequests.Clear();
			systemUnderTest.Language = "VB";

			generatedCode = systemUnderTest.GenerateCodeFile(out fileExtension);
			fileExtension.Should().BeNull();
			generatedCode.Should().BeNullOrEmpty();

			mockBuildEngine.LogErrorEventRequests.Count.Should().Be(3);
			mockBuildEngine.LogErrorEventRequests[0].Message.ShouldBeSameAs(@"The ClassLibraryNamespaceValidationChecks task item at index 0 has an invalid ItemSpec value. Make sure the xml item's ""Include"" attribute is specified correctly.");
			mockBuildEngine.LogErrorEventRequests[1].Message.ShouldBeSameAs("The ClassLibraryNamespaceValidationChecks task item at index 1 has an invalid or missing ValidNamespaces metadata item.\nItem:  <NamespaceValidationCheck Include=\"Rhinobyte.Tools.ProjectStructureTests.Tasks.GenerateProjectStructureTestsTask\" />\nMetadata: <ValidNamespacesCheck></ValidNamespacesCheck>");
			mockBuildEngine.LogErrorEventRequests[2].Message.ShouldBeSameAs("The ClassLibraryNamespaceValidationChecks task item at index 2 has an invalid or missing ValidNamespaces metadata item.\nItem:  <NamespaceValidationCheck Include=\"Rhinobyte.Tools.ProjectStructureTests.Tasks.GenerateProjectStructureTestsTask\" />\nMetadata: <ValidNamespacesCheck>; ;</ValidNamespacesCheck>");
		}

		[TestMethod]
		public void GenerateCode_returns_the_expected_result1()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "C#",
				OutputDirectory = new TaskItem(generatedFilesOutputDirectory),
				ShouldGenerateMissingTestMethodAttributeTestCase = true
			};

			var generatedCode = systemUnderTest.GenerateCodeFile(out var fileExtension);

			fileExtension.Should().Be("cs");

			// TODO: Consider loading the expected result from a file with an extension for the target framework version...
#if NETFRAMEWORK
			var runtimeVersionLine = "\n//     Runtime Version:4.0.30319.42000";
#else
			var runtimeVersionLine = string.Empty;
#endif

			var expectedCode =
$@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.{runtimeVersionLine}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated by the Rhinobyte.Extensions.TestTools.GenerateProjectStructureTestsTask class.
namespace Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks {{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhinobyte.Extensions.Reflection;
    using Rhinobyte.Extensions.TestTools;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;


    [System.CodeDom.Compiler.GeneratedCodeAttribute(""GenerateProjectStructureTestsTask"", ""1.0.0.0"")]
    [TestClass()]
    public partial class ProjectStructureTests {{

        [TestMethod()]
        public virtual void TestMethods_are_not_missing_the_TestMethod_attribute() {{
            var discoveredTestTypes = this.GetType().Assembly.GetTypes();

            var missingTestMethodAttributes = new List<string>();
            foreach (var testType in discoveredTestTypes)
            {{
                if (testType.IsCompilerGenerated() || !testType.IsDefined(typeof(TestClassAttribute), false))
                    continue;

                var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var testMethod in testMethods)
                {{
                    if (testMethod.IsDefined(typeof(TestMethodAttribute), true)
                        || testMethod.IsDefined(typeof(NotATestMethodAttribute), false)
                        || testMethod.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
                    {{
                        continue;
                    }}

                    missingTestMethodAttributes.Add($""{{testType.Name}}.{{testMethod.Name}}"");
                }}
            }}

            if (missingTestMethodAttributes.Count > 0)
                throw new AssertFailedException($""The following methods do not have a [TestMethod] attribute:{{Environment.NewLine}}{{Environment.NewLine}}{{string.Join(Environment.NewLine, missingTestMethodAttributes)}}"");
        }}
    }}
}}
";

			generatedCode!.ShouldBeSameAs(expectedCode, whitespaceNormalizationType: WhitespaceNormalizationType.TrimTrailingWhitespace);
		}

		[TestMethod]
		public void GenerateCode_returns_the_expected_result2()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "C#",
				OutputDirectory = new TaskItem(generatedFilesOutputDirectory),
				ShouldGenerateMissingTestMethodAttributeTestCase = false
			};

			var namespaceValidationCheckItem = new TaskItem("Rhinobyte.Extensions.TestTools.NotATestMethodAttribute");
			namespaceValidationCheckItem.SetMetadata(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces, "Rhinobyte.Extensions.TestTools;Rhinobyte.Extensions.TestTools.Assertions;");
			systemUnderTest.ClassLibraryNamespaceValidationChecks = new[] { namespaceValidationCheckItem };

			var generatedCode = systemUnderTest.GenerateCodeFile(out var fileExtension);

			fileExtension.Should().Be("cs");

			// TODO: Consider loading the expected result from a file with an extension for the target framework version...
#if NETFRAMEWORK
			var runtimeVersionLine = "\n//     Runtime Version:4.0.30319.42000";
#else
			var runtimeVersionLine = string.Empty;
#endif

			var expectedCode =
$@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.{runtimeVersionLine}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated by the Rhinobyte.Extensions.TestTools.GenerateProjectStructureTestsTask class.
namespace Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks {{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhinobyte.Extensions.Reflection;
    using Rhinobyte.Extensions.TestTools;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;


    [System.CodeDom.Compiler.GeneratedCodeAttribute(""GenerateProjectStructureTestsTask"", ""1.0.0.0"")]
    [TestClass()]
    public partial class ProjectStructureTests {{

        [TestMethod()]
        public virtual void Library_types_all_match_one_of_the_valid_namespaces() {{
            var libraryTypes = typeof(Rhinobyte.Extensions.TestTools.NotATestMethodAttribute).Assembly.GetTypes();
            var validNamespaces = new [] {{ ""Rhinobyte.Extensions.TestTools"", ""Rhinobyte.Extensions.TestTools.Assertions"" }};

            var invalidTypes = new List<string>();
            foreach (var libraryType in libraryTypes)
            {{
                if (libraryType.IsCompilerGenerated())
                    continue;

                var fullTypeName = libraryType?.FullName;
                if (fullTypeName is null)
                    continue;

                var lastDotIndex = fullTypeName.LastIndexOf('.');
                if (lastDotIndex == -1)
                    continue;

                var typeNamespace = fullTypeName.Substring(0, lastDotIndex);
               if (!validNamespaces.Contains(typeNamespace) && typeNamespace?.StartsWith(""Coverlet.Core.Instrumentation"") != true)
                    invalidTypes.Add(fullTypeName);

            }}

            if (invalidTypes.Count > 0)
                throw new AssertFailedException($""The following types have an incorrect namespace:{{Environment.NewLine}}{{Environment.NewLine}}{{string.Join(Environment.NewLine, invalidTypes)}}"");
        }}
    }}
}}
";

			generatedCode!.ShouldBeSameAs(expectedCode, whitespaceNormalizationType: WhitespaceNormalizationType.TrimTrailingWhitespace);
		}

		[TestMethod]
		public void GenerateCode_returns_the_expected_with_custom_method_names_to_ignore()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "C#",
				MissingTestMethodAttributeIgnoredMethodNames = "BuildServiceProvider;SetupContext;SetupServiceProvider",
				OutputDirectory = new TaskItem(generatedFilesOutputDirectory),
				ShouldGenerateMissingTestMethodAttributeTestCase = true
			};

			var generatedCode = systemUnderTest.GenerateCodeFile(out var fileExtension);
			fileExtension.Should().Be("cs");

			var expectedResult =
@"            var discoveredTestTypes = this.GetType().Assembly.GetTypes();
            var methodNamesToIgnore = new [] { ""BuildServiceProvider"", ""SetupContext"", ""SetupServiceProvider"" };

            var missingTestMethodAttributes = new List<string>();";

			generatedCode.RemoveAllCarriageReturns().Should().Contain(expectedResult.RemoveAllCarriageReturns());

			systemUnderTest.Language = "VB";
			generatedCode = systemUnderTest.GenerateCodeFile(out fileExtension);
			fileExtension.Should().Be("vb");

			expectedResult =
@"            Dim discoveredTestTypes() As System.Type = Me.GetType.Assembly.GetTypes
            Dim methodNamesToIgnore() As String = New [string]() {""BuildServiceProvider"", ""SetupContext"", ""SetupServiceProvider""}
            Dim missingTestMethodAttributes As System.Collections.Generic.List(Of String) = New System.Collections.Generic.List(Of String)()";

			generatedCode.RemoveAllCarriageReturns().Should().Contain(expectedResult.RemoveAllCarriageReturns());
		}

		[TestMethod]
		public void GenerateCode_returns_the_expected_result_for_VB()
		{
			var generatedFilesOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedTestFiles");
			if (!Directory.Exists(generatedFilesOutputDirectory))
				Directory.CreateDirectory(generatedFilesOutputDirectory);

			var systemUnderTest = new GenerateProjectStructureTestsTask()
			{
				GeneratedClassName = "ProjectStructureTests",
				GeneratedClassNamespaceToUse = "Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks",
				Language = "VB",
				OutputDirectory = new TaskItem(generatedFilesOutputDirectory),
				ShouldGenerateMissingTestMethodAttributeTestCase = true
			};

			var namespaceValidationCheckItem = new TaskItem("Rhinobyte.Extensions.TestTools.NotATestMethodAttribute");
			namespaceValidationCheckItem.SetMetadata(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces, "Rhinobyte.Extensions.TestTools;Rhinobyte.Extensions.TestTools.Assertions;");
			systemUnderTest.ClassLibraryNamespaceValidationChecks = new[] { namespaceValidationCheckItem };

			var generatedCode = systemUnderTest.GenerateCodeFile(out var fileExtension);

			fileExtension.Should().Be("vb");

			// TODO: Consider loading the expected result from a file with an extension for the target framework version...
#if NETFRAMEWORK
			var runtimeVersionLine = "\n'     Runtime Version:4.0.30319.42000";
#else
			var runtimeVersionLine = string.Empty;
#endif

			var expectedCode =
$@"'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.{runtimeVersionLine}
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Rhinobyte.Extensions.Reflection
Imports Rhinobyte.Extensions.TestTools
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection

'Generated by the Rhinobyte.Extensions.TestTools.GenerateProjectStructureTestsTask class.
Namespace Rhinobyte.Tools.ProjectStructureTests.Tests.Tasks
    
    <System.CodeDom.Compiler.GeneratedCodeAttribute(""GenerateProjectStructureTestsTask"", ""1.0.0.0""),  _
     TestClass()>  _
    Partial Public Class ProjectStructureTests
        
        <TestMethod()>  _
        Public Overridable Sub Library_types_all_match_one_of_the_valid_namespaces()
            Dim libraryTypes() As System.Type = GetType(Rhinobyte.Extensions.TestTools.NotATestMethodAttribute).Assembly.GetTypes
            Dim validNamespaces() As String = New [string]() {{""Rhinobyte.Extensions.TestTools"", ""Rhinobyte.Extensions.TestTools.Assertions""}}
            Dim invalidTypes As System.Collections.Generic.List(Of String) = New System.Collections.Generic.List(Of String)()
            Dim libraryTypeIndex As Integer = 0
            Do While (libraryTypeIndex < libraryTypes.Length)
                Dim libraryType As System.Type = libraryTypes(libraryTypeIndex)
                If libraryType.IsCompilerGenerated Then
                    goto continueIterationLabel
                End If
                Dim fullTypeName As String = libraryType.FullName
                If (fullTypeName Is Nothing) Then
                    goto continueIterationLabel
                End If
                Dim lastDotIndex As Integer = fullTypeName.LastIndexOf(Global.Microsoft.VisualBasic.ChrW(46))
                If (lastDotIndex = -1) Then
                    goto continueIterationLabel
                End If
                Dim typeNamespace As String = fullTypeName.Substring(0, lastDotIndex)
                If ((validNamespaces.Contains(typeNamespace) = false)  _
                            AndAlso (typeNamespace.StartsWith(""Coverlet.Core.Instrumentation"") = false)) Then
                    invalidTypes.Add(fullTypeName)
                End If
            continueIterationLabel:
                libraryTypeIndex = (libraryTypeIndex + 1)
            Loop
            If (invalidTypes.Count > 0) Then
                Throw New AssertFailedException(String.Concat(""The following types have an incorrect namespace:"", System.Environment.NewLine, System.Environment.NewLine, String.Join(System.Environment.NewLine, invalidTypes)))
            End If
        End Sub
        
        <TestMethod()>  _
        Public Overridable Sub TestMethods_are_not_missing_the_TestMethod_attribute()
            Dim discoveredTestTypes() As System.Type = Me.GetType.Assembly.GetTypes
            Dim missingTestMethodAttributes As System.Collections.Generic.List(Of String) = New System.Collections.Generic.List(Of String)()
            Dim discoveredTypeIndex As Integer = 0
            Do While (discoveredTypeIndex < discoveredTestTypes.Length)
                Dim testType As System.Type = discoveredTestTypes(discoveredTypeIndex)
                If (testType.IsCompilerGenerated  _
                            OrElse (testType.IsDefined(GetType(TestClassAttribute), false) = false)) Then
                    goto continueTestTypeIterationLabel
                End If
                Dim testMethods() As System.Reflection.MethodInfo = testType.GetMethods((System.Reflection.BindingFlags.Public  _
                                Or (System.Reflection.BindingFlags.NonPublic  _
                                Or (System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.DeclaredOnly))))
                Dim testMethodIndex As Integer = 0
                Do While (testMethodIndex < testMethods.Length)
                    Dim testMethod As System.Reflection.MethodInfo = testMethods(testMethodIndex)
                    If (testMethod.IsDefined(GetType(TestMethodAttribute), true)  _
                                OrElse (testMethod.IsDefined(GetType(NotATestMethodAttribute), false) OrElse testMethod.IsDefined(GetType(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))) Then
                        goto continueTestMethodIterationLabel
                    End If
                    missingTestMethodAttributes.Add(String.Concat(testType.Name, ""."", testMethod.Name))
                continueTestMethodIterationLabel:
                    testMethodIndex = (testMethodIndex + 1)
                Loop
            continueTestTypeIterationLabel:
                discoveredTypeIndex = (discoveredTypeIndex + 1)
            Loop
            If (missingTestMethodAttributes.Count > 0) Then
                Throw New AssertFailedException(String.Concat(""The following types have an incorrect namespace:"", System.Environment.NewLine, System.Environment.NewLine, String.Join(System.Environment.NewLine, missingTestMethodAttributes)))
            End If
        End Sub
    End Class
End Namespace
";

			generatedCode!.ShouldBeSameAs(expectedCode, whitespaceNormalizationType: WhitespaceNormalizationType.TrimLeadingAndTrailingWhitespace);
		}



		/******     TEST SETUP     *****************************
		 *******************************************************/
	}
}
