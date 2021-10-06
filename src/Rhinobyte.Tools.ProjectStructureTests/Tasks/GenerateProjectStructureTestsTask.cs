using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Rhinobyte.Tools.ProjectStructureTests.Tasks
{
	/// <summary>
	/// Subclass implementation of the <see cref="Microsoft.Build.Utilities.Task"/> class that can be used to auto-generate
	/// a ProjectStructureTests.generated.cs file for test projects.
	/// </summary>
	public sealed class GenerateProjectStructureTestsTask : Task
	{
		private static readonly string[] NamespaceUsingStatements = new string[]
		{
			"Microsoft.VisualStudio.TestTools.UnitTesting",
			"Rhinobyte.Extensions.Reflection",
			"Rhinobyte.Extensions.TestTools",
			"System",
			"System.Collections.Generic",
			"System.Linq",
			"System.Reflection"
		};

		/// <summary>
		/// Task items used to generate a project structure test method that verfies all of the exported types
		/// for a given class library use one of the specified namespaces.
		/// <para>
		/// The ITaskItem.ItemSpec should be a fully qualified typename that will be used to output a code line in the test in this format:
		/// <code>var libraryTypes = typeof(&lt;TaskItem.ItemSpec&gt;).Assembly.GetTypes()</code>
		/// </para>
		/// </summary>
#pragma warning disable CA1819 // Properties should not return arrays  - Reason: Required format for custom msbuild Task subclasses
		public ITaskItem[]? ClassLibraryNamespaceValidationChecks { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

		/// <summary>
		/// An [optional] parameter that can be used to specify a class name for the generated test class.
		/// <para>The default class name will be <c>ProjectStructureTests</c></para>
		/// </summary>
		public string? GeneratedClassName { get; set; }

		/// <summary>
		/// The namespace to use for the generated test cases file.
		/// </summary>
		public string? GeneratedClassNamespaceToUse { get; set; }

		/// <summary>
		/// Language of code to generate.
		/// Language name can be any language for which a CodeDom provider is
		/// available. For example, "C#", "VisualBasic".
		/// Emitted file will have the default extension for that language.
		/// </summary>
		[Required]
		public string? Language { get; set; }

		/// <summary>
		/// The message importance to use when logging non-error messages.
		/// <para>Importance: high, normal, low (default normal)</para>
		/// </summary>
		public string? LoggingMessageImportance { get; set; }

		/// <summary>
		/// An [optional] semi-colon delimited list of method names that will be added to an ignored names collection
		/// in the generated missing test method attribute test case.
		/// </summary>
		public string? MissingTestMethodAttributeIgnoredMethodNames { get; set; }

		/// <summary>
		/// The directory where the generated 
		/// </summary>
		public ITaskItem? OutputDirectory { get; set; }

		/// <summary>
		/// The absolute path to the file that was generated.
		/// <para>Typically, this will be intermediate output path combined with the default file name of 'ProjectStructureTests.generated.cs'</para>
		/// <para>If a custom <see cref="OutputDirectory"/> is provided the code file will be output there instead.</para>
		/// </summary>
		[Output]
		public ITaskItem? OutputFile { get; set; }

		/// <summary>
		/// Whether or not a test case method should be generated that searches for test class methods
		/// that are missing the [TestMethod] attribute decorator.
		/// </summary>
		public bool ShouldGenerateMissingTestMethodAttributeTestCase { get; set; }


		/// <summary>
		/// Implementation of the <see cref="Task.Execute"/> method that handles generation of the project structure tests
		/// source file.
		/// </summary>
		public override bool Execute()
		{
			var hasItemsToGenerate = ClassLibraryNamespaceValidationChecks?.Length > 0
				|| ShouldGenerateMissingTestMethodAttributeTestCase;

			var messageImportance = ParseMessageImportance();
			if (!hasItemsToGenerate)
			{
				Log.LogMessage(messageImportance, "GenerateProjectStructureTestsTask did not receive any configuration values that would result in the generation of the tests code file.");
				return true;
			}

			// TODO: Move strings into resource files and support localization
			if (string.IsNullOrEmpty(Language))
			{
				Log.LogError(@"""GenerateProjectStructureTestsTask"" received an invalid value for the ""Language"" parameter.");
				return false;
			}

			if (OutputFile == null && OutputDirectory == null)
			{
				Log.LogError(@"""GenerateProjectStructureTestsTask"" received a null value for both the ""OutputFile"" and the ""OutputDirectory"" task item parameters.");
				return false;
			}

			var generatedCode = GenerateCodeFile(out var fileExtension);
			if (Log.HasLoggedErrors)
				return false;

			if (string.IsNullOrEmpty(generatedCode))
			{
				Log.LogMessage(messageImportance, "No ProjectStructureTests output file was written because no code was specified to create");
				OutputFile = null;
				return true;
			}

			try
			{
				if (OutputFile != null && OutputDirectory != null && !Path.IsPathRooted(OutputFile.ItemSpec))
					OutputFile = new TaskItem(Path.Combine(OutputDirectory.ItemSpec, OutputFile.ItemSpec));

				OutputFile ??= new TaskItem(Path.Combine(OutputDirectory!.ItemSpec, $"ProjectStructureTests.generated.{fileExtension}"));

				File.WriteAllText(OutputFile.ItemSpec, generatedCode); // Overwrites file if it already exists (and can be overwritten)
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				Log.LogError($@"""GenerateProjectStructureTestsTask"" encountered an exception writing to the output file: {OutputFile!.ItemSpec}{Environment.NewLine}{exc.Message}");
			}

			return !Log.HasLoggedErrors;
		}

		internal string? GenerateCodeFile(out string? fileExtension)
		{
			CodeDomProvider? codeProvider = null;
			fileExtension = null;
			var isCsharp = Language!.Equals("CSharp", StringComparison.OrdinalIgnoreCase)
				|| Language!.Equals("C#", StringComparison.OrdinalIgnoreCase);

			try
			{
				try
				{
					codeProvider = CodeDomProvider.CreateProvider(Language);
				}
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
				{
					Log.LogError($@"ProjectStructureTests code for the language ""{Language}"" could not be generated.  {exc.Message}");
					return null;
				}

				var errorMessages = new List<string>();
				var hasErrors = false;
				var stringBuilder = new StringBuilder();

				var testClassName = GeneratedClassName;
				if (string.IsNullOrWhiteSpace(testClassName))
					testClassName = "ProjectStructureTests";

				var environmentNewlineExpression = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(System.Environment)), nameof(System.Environment.NewLine));
				var falseLiteral = new CodePrimitiveExpression(false);
				var nullLiteral = new CodePrimitiveExpression(null);
				var stringTypeReferenceExpression = new CodeTypeReferenceExpression(typeof(string));
				var trueLiteral = new CodePrimitiveExpression(true);
				var zeroLiteral = new CodePrimitiveExpression(0);

				// CodeSnippet items doesn't get indented so we'll have to prepend this ourselves
				var methodBodyIndentation = "            ";

				var testMethodDeclarations = new List<CodeMemberMethod>();
				if (ClassLibraryNamespaceValidationChecks != null)
				{
					var namespaceValidationCheckItemCount = ClassLibraryNamespaceValidationChecks.Length;
					var namespaceValidationCheckItemIndex = 0;
					foreach (var namespaceValidationCheckItem in ClassLibraryNamespaceValidationChecks)
					{
						++namespaceValidationCheckItemIndex;

						if (string.IsNullOrWhiteSpace(namespaceValidationCheckItem.ItemSpec))
						{
							errorMessages.Add($@"The ClassLibraryNamespaceValidationChecks task item at index {namespaceValidationCheckItemIndex-1} has an invalid ItemSpec value. Make sure the xml item's ""Include"" attribute is specified correctly.");
							hasErrors = true;
							continue;
						}

						var validNamespacesRaw = namespaceValidationCheckItem.GetMetadata(nameof(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces));
						if (string.IsNullOrWhiteSpace(validNamespacesRaw))
						{
							errorMessages.Add($@"The ClassLibraryNamespaceValidationChecks task item at index {namespaceValidationCheckItemIndex-1} has an invalid or missing {nameof(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces)} metadata item.{Environment.NewLine}Item:  <NamespaceValidationCheck Include=""{namespaceValidationCheckItem.ItemSpec}"" />{Environment.NewLine}Metadata: <ValidNamespacesCheck>{validNamespacesRaw}</ValidNamespacesCheck>");
							hasErrors = true;
							continue;
						}

						var validNamespacesArray = validNamespacesRaw.Split(';');

						// Generate this statement up here so we can include a validation failure message if all the namespace values are blank
						CodeStatement validNamespacesVariableCodeStatement;
						if (isCsharp)
						{
							_ = stringBuilder
								.Clear()
								.Append(methodBodyIndentation)
								.Append("var validNamespaces = new [] {");

							var validNamespaceCount = 0;
							foreach (var validNamespace in validNamespacesArray)
							{
								if (string.IsNullOrWhiteSpace(validNamespace))
									continue;

								++validNamespaceCount;
								_ = stringBuilder.Append(' ').Append('"').Append(validNamespace).Append('"').Append(',');
							}

							if (validNamespaceCount < 1)
							{
								errorMessages.Add($@"The ClassLibraryNamespaceValidationChecks task item at index {namespaceValidationCheckItemIndex-1} has an invalid or missing {nameof(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces)} metadata item.{Environment.NewLine}Item:  <NamespaceValidationCheck Include=""{namespaceValidationCheckItem.ItemSpec}"" />{Environment.NewLine}Metadata: <ValidNamespacesCheck>{validNamespacesRaw}</ValidNamespacesCheck>");
								hasErrors = true;
								continue;
							}

							if (stringBuilder[stringBuilder.Length - 1] == ',')
								stringBuilder[stringBuilder.Length - 1] = ' ';

							_ = stringBuilder.Append("};");
							validNamespacesVariableCodeStatement = new CodeSnippetStatement(stringBuilder.ToString());
						}
						else
						{
							var validNamespacesStringLiterals = new List<CodeExpression>();
							foreach (var validNamespace in validNamespacesArray)
							{
								if (string.IsNullOrWhiteSpace(validNamespace))
									continue;

								validNamespacesStringLiterals.Add(new CodePrimitiveExpression(validNamespace));
							}

							if (validNamespacesStringLiterals.Count < 1)
							{
								errorMessages.Add($@"The ClassLibraryNamespaceValidationChecks task item at index {namespaceValidationCheckItemIndex-1} has an invalid or missing {nameof(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.ValidNamespaces)} metadata item.{Environment.NewLine}Item:  <NamespaceValidationCheck Include=""{namespaceValidationCheckItem.ItemSpec}"" />{Environment.NewLine}Metadata: <ValidNamespacesCheck>{validNamespacesRaw}</ValidNamespacesCheck>");
								hasErrors = true;
								continue;
							}

							var validNamespacesInitExpression = new CodeArrayCreateExpression("string", validNamespacesStringLiterals.ToArray());
							validNamespacesVariableCodeStatement = new CodeVariableDeclarationStatement(typeof(string[]), "validNamespaces", validNamespacesInitExpression);
						}

						// Once we have at least one error there is no need to actually build the code object graph...
						// We're just running through all the items so we can build a complete collection of validation error messages to log
						// before returning
						if (hasErrors)
							continue;

						var testMethodName = GetNamespaceValidationCheckTestMethodName(namespaceValidationCheckItem, namespaceValidationCheckItemIndex, namespaceValidationCheckItemCount);

						var testMethodCodeDeclaration = new CodeMemberMethod()
						{
							Attributes = MemberAttributes.Public,
							Name = testMethodName,
							ReturnType = new CodeTypeReference(typeof(void))
						};
						testMethodDeclarations.Add(testMethodCodeDeclaration);

						_ = testMethodCodeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration("TestMethod"));


						if (isCsharp)
						{
							// Assuming a defacto language of C# so we'll output using raw code snippets for better formatting and fallback on the
							// code dom statements for any other language...
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}var libraryTypes = typeof({namespaceValidationCheckItem.ItemSpec}).Assembly.GetTypes();"));
							_ = testMethodCodeDeclaration.Statements.Add(validNamespacesVariableCodeStatement);
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}var invalidTypes = new List<string>();"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}foreach (var libraryType in libraryTypes)"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}{{"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    if (libraryType.IsCompilerGenerated())"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        continue;"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    var fullTypeName = libraryType?.FullName;"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    if (fullTypeName is null)"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        continue;"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    var lastDotIndex = fullTypeName.LastIndexOf('.');"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    if (lastDotIndex == -1)"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        continue;"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    var typeNamespace = fullTypeName.Substring(0, lastDotIndex);"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($@"{methodBodyIndentation}   if (!validNamespaces.Contains(typeNamespace) && typeNamespace?.StartsWith(""Coverlet.Core.Instrumentation"") != true)"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        invalidTypes.Add(fullTypeName);"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}}}"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}if (invalidTypes.Count > 0)"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($@"{methodBodyIndentation}    throw new AssertFailedException($""The following types have an incorrect namespace:{{Environment.NewLine}}{{Environment.NewLine}}{{string.Join(Environment.NewLine, invalidTypes)}}"");"));
						}
						else
						{
							var typeofExpression = new CodeTypeOfExpression(namespaceValidationCheckItem.ItemSpec);
							var assemblyPropertyExpression = new CodePropertyReferenceExpression(typeofExpression, nameof(Type.Assembly));
							var getTypesMethodCallExpression = new CodeMethodInvokeExpression(assemblyPropertyExpression, nameof(Assembly.GetTypes));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeVariableDeclarationStatement(typeof(Type[]), "libraryTypes", getTypesMethodCallExpression));
							_ = testMethodCodeDeclaration.Statements.Add(validNamespacesVariableCodeStatement);
							_ = testMethodCodeDeclaration.Statements.Add(new CodeVariableDeclarationStatement(typeof(List<string>), "invalidTypes", new CodeObjectCreateExpression(typeof(List<string>))));

							var libraryTypeIndexInitializeStatement = new CodeVariableDeclarationStatement(typeof(int), "libraryTypeIndex", zeroLiteral);
							var libraryTypeIndexReference = new CodeVariableReferenceExpression("libraryTypeIndex");
							var libraryTypesArrayReference = new CodeVariableReferenceExpression("libraryTypes");

#pragma warning disable IDE0028 // Simplify collection initialization
							var forLoopBodyStatements = new List<CodeStatement>();
#pragma warning restore IDE0028 // Simplify collection initialization

							forLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(Type), "libraryType", new CodeArrayIndexerExpression(libraryTypesArrayReference, libraryTypeIndexReference)));

							var nextLibraryTypeReference = new CodeVariableReferenceExpression("libraryType");
							var invalidTypesReference = new CodeVariableReferenceExpression("invalidTypes");

							// Codedom doesn't have anything easy like a continue statement... the closest we can to it, I think, is a label statement as
							// the last item in the iteration loop and a goto statement in place of a continue that jumps to the bottom of the loop so it
							// can come back up to the top
							var continueStatement = new CodeGotoStatement("continueIterationLabel");

							forLoopBodyStatements.Add(new CodeConditionStatement(
								condition: new CodeMethodInvokeExpression(nextLibraryTypeReference, "IsCompilerGenerated"),
								trueStatements: new[] { continueStatement }
							));


							forLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(string), "fullTypeName", new CodePropertyReferenceExpression(nextLibraryTypeReference, "FullName")));
							var fullTypeNameReference = new CodeVariableReferenceExpression("fullTypeName");

							forLoopBodyStatements.Add(new CodeConditionStatement(
								condition: new CodeBinaryOperatorExpression(
									left: fullTypeNameReference,
									op: CodeBinaryOperatorType.IdentityEquality,
									right: nullLiteral
								),
								trueStatements: new[] { continueStatement }
							));


							forLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(int), "lastDotIndex", new CodeMethodInvokeExpression(fullTypeNameReference, nameof(string.LastIndexOf), new CodePrimitiveExpression('.'))));
							var lastDotIndexReference = new CodeVariableReferenceExpression("lastDotIndex");

							forLoopBodyStatements.Add(new CodeConditionStatement(
								condition: new CodeBinaryOperatorExpression(
									left: lastDotIndexReference,
									op: CodeBinaryOperatorType.ValueEquality,
									right: new CodePrimitiveExpression(-1)
								),
								trueStatements: new[] { continueStatement }
							));

							forLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(string), "typeNamespace", new CodeMethodInvokeExpression(fullTypeNameReference, "Substring", zeroLiteral, lastDotIndexReference)));
							var typeNamespaceReference = new CodeVariableReferenceExpression("typeNamespace");

							var validNamespacesDoesNotContainExpression = new CodeBinaryOperatorExpression(
								left: new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("validNamespaces"), "Contains", typeNamespaceReference),
								op: CodeBinaryOperatorType.ValueEquality,
								right: falseLiteral
							);

							var typeNamespaceStartsWithCoverletExpression = new CodeBinaryOperatorExpression(
								left: new CodeMethodInvokeExpression(typeNamespaceReference, nameof(string.StartsWith), new CodePrimitiveExpression("Coverlet.Core.Instrumentation")),
								op: CodeBinaryOperatorType.ValueEquality,
								right: falseLiteral
							);

							var complexAndCondition = new CodeBinaryOperatorExpression(
								left: validNamespacesDoesNotContainExpression,
								op: CodeBinaryOperatorType.BooleanAnd,
								right: typeNamespaceStartsWithCoverletExpression
							);

							forLoopBodyStatements.Add(new CodeConditionStatement(
								condition: complexAndCondition,
								trueStatements: new[] {
									new CodeExpressionStatement(new CodeMethodInvokeExpression(invalidTypesReference, "Add", fullTypeNameReference))
								}
							));

							// Important, make sure the 'continueIterationLabel:' statement is the LAST one added to the forLoopBodyStatements
							forLoopBodyStatements.Add(new CodeLabeledStatement("continueIterationLabel"));

							var forLoopStatement = new CodeIterationStatement(
								initStatement: libraryTypeIndexInitializeStatement,
								testExpression: new CodeBinaryOperatorExpression(
									left: libraryTypeIndexReference,
									op: CodeBinaryOperatorType.LessThan,
									right: new CodePropertyReferenceExpression(libraryTypesArrayReference, nameof(Array.Length))
								),
								incrementStatement: new CodeAssignStatement(
									left: libraryTypeIndexReference,
									right: new CodeBinaryOperatorExpression(
										left: libraryTypeIndexReference,
										op: CodeBinaryOperatorType.Add,
										right: new CodePrimitiveExpression(1)
									)
								),
								statements: forLoopBodyStatements.ToArray()
							);
							_ = testMethodCodeDeclaration.Statements.Add(forLoopStatement);

							var stringJoinExpression = new CodeMethodInvokeExpression(stringTypeReferenceExpression, nameof(string.Join), environmentNewlineExpression, invalidTypesReference);

							var stringConcatExpression = new CodeMethodInvokeExpression(
								targetObject: stringTypeReferenceExpression,
								methodName: nameof(string.Concat),
								parameters: new CodeExpression[] {
									new CodePrimitiveExpression("The following types have an incorrect namespace:"),
									environmentNewlineExpression,
									environmentNewlineExpression,
									stringJoinExpression
								}
							);

							_ = testMethodCodeDeclaration.Statements.Add(new CodeConditionStatement(
								condition: new CodeBinaryOperatorExpression(
									left: new CodePropertyReferenceExpression(invalidTypesReference, "Count"),
									op: CodeBinaryOperatorType.GreaterThan,
									right: zeroLiteral
								),
								trueStatements: new[] {
									new CodeThrowExceptionStatement(new CodeObjectCreateExpression("AssertFailedException", stringConcatExpression))
								}
							));
						}
					}
				}

				if (hasErrors)
				{
					foreach (var errorMessage in errorMessages)
						Log.LogError(errorMessage);

					return null;
				}

				if (ShouldGenerateMissingTestMethodAttributeTestCase)
				{
					var testMethodCodeDeclaration = new CodeMemberMethod()
					{
						Attributes = MemberAttributes.Public,
						Name = "TestMethods_are_not_missing_the_TestMethod_attribute",
						ReturnType = new CodeTypeReference(typeof(void))
					};
					testMethodDeclarations.Add(testMethodCodeDeclaration);

					_ = testMethodCodeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration("TestMethod"));

					var methodNamesToIgnore = MissingTestMethodAttributeIgnoredMethodNames?.Split(';') ?? Array.Empty<string>();

					if (isCsharp)
					{
						// Assuming a defacto language of C# so we'll output using raw code snippets for better formatting and fallback on the
						// code dom statements for any other language...
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}var discoveredTestTypes = this.GetType().Assembly.GetTypes();"));

						var hasMethodNamesToIgnore = false;
						if (methodNamesToIgnore.Length > 0)
						{
							_ = stringBuilder
								.Clear()
								.Append(methodBodyIndentation)
								.Append("var methodNamesToIgnore = new [] {");

							var validMethodNamesCount = 0;
							foreach (var nextMethodName in methodNamesToIgnore)
							{
								if (string.IsNullOrWhiteSpace(nextMethodName))
									continue;

								++validMethodNamesCount;
								_ = stringBuilder.Append(' ').Append('"').Append(nextMethodName).Append('"').Append(',');
							}

							if (validMethodNamesCount > 0)
							{
								// Only add the validMethodNames declaration if we have at least one method name string literal...
								if (stringBuilder[stringBuilder.Length - 1] == ',')
									stringBuilder[stringBuilder.Length - 1] = ' ';

								_ = stringBuilder.Append("};");
								_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(stringBuilder.ToString()));
								hasMethodNamesToIgnore = true;
							}
						}

						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}var missingTestMethodAttributes = new List<string>();"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}foreach (var testType in discoveredTestTypes)"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}{{"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    if (testType.IsCompilerGenerated() || !testType.IsDefined(typeof(TestClassAttribute), false))"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        continue;"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    foreach (var testMethod in testMethods)"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    {{"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        if (testMethod.IsDefined(typeof(TestMethodAttribute), true)"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}            || testMethod.IsDefined(typeof(NotATestMethodAttribute), false)"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}            || testMethod.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        {{"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}            continue;"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        }}"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));

						if (hasMethodNamesToIgnore)
						{
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}        if (methodNamesToIgnore.Contains(testMethod.Name))"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}            continue;"));
							_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
						}

						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($@"{methodBodyIndentation}        missingTestMethodAttributes.Add($""{{testType.Name}}.{{testMethod.Name}}"");"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}    }}"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}}}"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement(string.Empty));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($"{methodBodyIndentation}if (missingTestMethodAttributes.Count > 0)"));
						_ = testMethodCodeDeclaration.Statements.Add(new CodeSnippetStatement($@"{methodBodyIndentation}    throw new AssertFailedException($""The following methods do not have a [TestMethod] attribute:{{Environment.NewLine}}{{Environment.NewLine}}{{string.Join(Environment.NewLine, missingTestMethodAttributes)}}"");"));
					}
					else
					{
						// Type[] discoveredTestTypes = tyhis.GetType().Assembly.GetTypes();
						_ = testMethodCodeDeclaration.Statements.Add(new CodeVariableDeclarationStatement(
							type: typeof(Type[]),
							name: "discoveredTestTypes",
							initExpression: new CodeMethodInvokeExpression(
								targetObject: new CodePropertyReferenceExpression(
									targetObject: new CodeMethodInvokeExpression(
										targetObject: new CodeThisReferenceExpression(),
										methodName: nameof(object.GetType)
									),
									propertyName: nameof(Type.Assembly)
								),
								methodName: nameof(Assembly.GetTypes)
							)
						));
						var discoveredTypesArrayReference = new CodeVariableReferenceExpression("discoveredTestTypes");

						var hasMethodNamesToIgnore = false;
						if (methodNamesToIgnore.Length > 0)
						{
							var methodNameToIgnoreStringLiterals = new List<CodeExpression>();
							foreach (var methodNameToIgnore in methodNamesToIgnore)
							{
								if (string.IsNullOrWhiteSpace(methodNameToIgnore))
									continue;

								methodNameToIgnoreStringLiterals.Add(new CodePrimitiveExpression(methodNameToIgnore));
							}

							if (methodNameToIgnoreStringLiterals.Count > 0)
							{
								// Only add the validMethodNames declaration if we have at least one method name string literal...
								var methodNamesToIgnoreInitExpression = new CodeArrayCreateExpression("string", methodNameToIgnoreStringLiterals.ToArray());
								_ = testMethodCodeDeclaration.Statements.Add(new CodeVariableDeclarationStatement(typeof(string[]), "methodNamesToIgnore", methodNamesToIgnoreInitExpression));
								hasMethodNamesToIgnore = true;
							}
						}

						_ = testMethodCodeDeclaration.Statements.Add(new CodeVariableDeclarationStatement(typeof(List<string>), "missingTestMethodAttributes", new CodeObjectCreateExpression(typeof(List<string>))));
						var missingTestMethodAttributesReference = new CodeVariableReferenceExpression("missingTestMethodAttributes");


						//////////// START OF OUTER FOR LOOP ////////////////////
						var discoveredTypeIndexInitializeStatement = new CodeVariableDeclarationStatement(typeof(int), "discoveredTypeIndex", zeroLiteral);
						var discoveredTypeIndexReference = new CodeVariableReferenceExpression("discoveredTypeIndex");

#pragma warning disable IDE0028 // Simplify collection initialization
						var forTestTypeLoopBodyStatements = new List<CodeStatement>();
#pragma warning restore IDE0028 // Simplify collection initialization

						forTestTypeLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(Type), "testType", new CodeArrayIndexerExpression(discoveredTypesArrayReference, discoveredTypeIndexReference)));
						var nextTestTypeReference = new CodeVariableReferenceExpression("testType");

						// Codedom doesn't have anything easy like a continue statement... the closest we can to it, I think, is a label statement as
						// the last item in the iteration loop and a goto statement in place of a continue that jumps to the bottom of the loop so it
						// can come back up to the top
						var continueOuterStatement = new CodeGotoStatement("continueTestTypeIterationLabel");

						// testType.IsCompilerGenerated() || !testType.IsDefined(typeof(TestClassAttribute), false)
						var topConditional = new CodeBinaryOperatorExpression(
							// testType.IsCompilerGenerated()
							left: new CodeMethodInvokeExpression(nextTestTypeReference, "IsCompilerGenerated"),
							op: CodeBinaryOperatorType.BooleanOr,
							// !testType.IsDefined(...)
							right: new CodeBinaryOperatorExpression(
								left: new CodeMethodInvokeExpression(
									targetObject: nextTestTypeReference,
									methodName: nameof(Type.IsDefined),
									parameters: new CodeExpression[]
									{
										new CodeTypeOfExpression("TestClassAttribute"),
										falseLiteral
									}
								),
								op: CodeBinaryOperatorType.ValueEquality,
								right: falseLiteral
							)
						);

						forTestTypeLoopBodyStatements.Add(new CodeConditionStatement(
							condition: topConditional,
							trueStatements: new[] { continueOuterStatement }
						));


						var getMethodsCallExpression = new CodeMethodInvokeExpression(nextTestTypeReference, nameof(Type.GetMethods),
							// BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly
							new CodeBinaryOperatorExpression(
								left: new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(BindingFlags)), nameof(BindingFlags.Public)),
								op: CodeBinaryOperatorType.BitwiseOr,
								right: new CodeBinaryOperatorExpression(
									left: new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(BindingFlags)), nameof(BindingFlags.NonPublic)),
									op: CodeBinaryOperatorType.BitwiseOr,
									right: new CodeBinaryOperatorExpression(
										left: new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(BindingFlags)), nameof(BindingFlags.Instance)),
										op: CodeBinaryOperatorType.BitwiseOr,
										right: new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(BindingFlags)), nameof(BindingFlags.DeclaredOnly))
									)
								)
							)
						);

						forTestTypeLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(MethodInfo[]), "testMethods", getMethodsCallExpression));
						var testMethodsArrayReference = new CodeVariableReferenceExpression("testMethods");



						// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
						// !!    START OF INNER FOR LOOP STATEMENTS    !!
						// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
						var testMethodIndexInitializeStatement = new CodeVariableDeclarationStatement(typeof(int), "testMethodIndex", zeroLiteral);
						var testMethodIndexReference = new CodeVariableReferenceExpression("testMethodIndex");

#pragma warning disable IDE0028 // Simplify collection initialization
						var forMethodLoopBodyStatements = new List<CodeStatement>();
#pragma warning restore IDE0028 // Simplify collection initialization
						forMethodLoopBodyStatements.Add(new CodeVariableDeclarationStatement(typeof(MethodInfo), "testMethod", new CodeArrayIndexerExpression(testMethodsArrayReference, testMethodIndexReference)));
						var nextTestMethodReference = new CodeVariableReferenceExpression("testMethod");

						// Codedom doesn't have anything easy like a continue statement... the closest we can to it, I think, is a label statement as
						// the last item in the iteration loop and a goto statement in place of a continue that jumps to the bottom of the loop so it
						// can come back up to the top
						var continueInnerStatement = new CodeGotoStatement("continueTestMethodIterationLabel");

						// testMethod.IsDefined(...) || testMethod.IsDefined(..) || testMethod.IsDefined(..)
						var complexTestMethodConditional = new CodeBinaryOperatorExpression(
							left: new CodeMethodInvokeExpression(nextTestMethodReference, nameof(MethodInfo.IsDefined), new CodeTypeOfExpression("TestMethodAttribute"), trueLiteral),
							op: CodeBinaryOperatorType.BooleanOr,
							right: new CodeBinaryOperatorExpression(
								left: new CodeMethodInvokeExpression(nextTestMethodReference, nameof(MethodInfo.IsDefined), new CodeTypeOfExpression("NotATestMethodAttribute"), falseLiteral),
								op: CodeBinaryOperatorType.BooleanOr,
								right: new CodeMethodInvokeExpression(nextTestMethodReference, nameof(MethodInfo.IsDefined), new CodeTypeOfExpression("System.Runtime.CompilerServices.CompilerGeneratedAttribute"), falseLiteral)
							)
						);

						forMethodLoopBodyStatements.Add(new CodeConditionStatement(
							condition: complexTestMethodConditional,
							trueStatements: new[] { continueInnerStatement }
						));

						if (hasMethodNamesToIgnore)
						{
							// if (methodNamesToIgnore.Contains(testMethod.Name) continue;
							forMethodLoopBodyStatements.Add(new CodeConditionStatement(
								condition: new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("methodNamesToIgnore"), "Contains", new CodePropertyReferenceExpression(nextTestMethodReference, nameof(MethodInfo.Name))),
								trueStatements: new[] { continueInnerStatement }
							));
						}

						// missingTestMethodAttributes.Add($"{testType.Name}.{testMethod.Name}");
						var testMethodNameConcatCall = new CodeMethodInvokeExpression(
							targetObject: stringTypeReferenceExpression,
							methodName: nameof(string.Concat),
							parameters: new CodeExpression[] {
									new CodePropertyReferenceExpression(nextTestTypeReference, nameof(Type.Name)),
									new CodePrimitiveExpression("."),
									new CodePropertyReferenceExpression(nextTestMethodReference, nameof(MethodInfo.Name))
							}
						);
						forMethodLoopBodyStatements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(missingTestMethodAttributesReference, "Add", testMethodNameConcatCall)));

						// Important, make sure the 'continueTestMethodIterationLabel:' statement is LAST the one added to the forMethodLoopBodyStatements
						forMethodLoopBodyStatements.Add(new CodeLabeledStatement("continueTestMethodIterationLabel"));

						var innerForLoopStatement = new CodeIterationStatement(
							initStatement: testMethodIndexInitializeStatement,
							testExpression: new CodeBinaryOperatorExpression(
								left: testMethodIndexReference,
								op: CodeBinaryOperatorType.LessThan,
								right: new CodePropertyReferenceExpression(testMethodsArrayReference, nameof(Array.Length))
							),
							incrementStatement: new CodeAssignStatement(
								left: testMethodIndexReference,
								right: new CodeBinaryOperatorExpression(
									left: testMethodIndexReference,
									op: CodeBinaryOperatorType.Add,
									right: new CodePrimitiveExpression(1)
								)
							),
							statements: forMethodLoopBodyStatements.ToArray()
						);
						// Add the inner for loop to the outer for loop body
						forTestTypeLoopBodyStatements.Add(innerForLoopStatement);


						// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! //
						// !!    END OF INNER FOR LOOP STATEMENTS !! //
						// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! //

						// Important, make sure the 'continueTestTypeIterationLabel:' statement is LAST the one added to the forTestTypeLoopBodyStatements
						forTestTypeLoopBodyStatements.Add(new CodeLabeledStatement("continueTestTypeIterationLabel"));

						var outerForLoopStatement = new CodeIterationStatement(
							initStatement: discoveredTypeIndexInitializeStatement,
							testExpression: new CodeBinaryOperatorExpression(
								left: discoveredTypeIndexReference,
								op: CodeBinaryOperatorType.LessThan,
								right: new CodePropertyReferenceExpression(discoveredTypesArrayReference, nameof(Array.Length))
							),
							incrementStatement: new CodeAssignStatement(
								left: discoveredTypeIndexReference,
								right: new CodeBinaryOperatorExpression(
									left: discoveredTypeIndexReference,
									op: CodeBinaryOperatorType.Add,
									right: new CodePrimitiveExpression(1)
								)
							),
							statements: forTestTypeLoopBodyStatements.ToArray()
						);
						// Add the outer for loop to the method body
						_ = testMethodCodeDeclaration.Statements.Add(outerForLoopStatement);

						var stringJoinExpression = new CodeMethodInvokeExpression(stringTypeReferenceExpression, nameof(string.Join), environmentNewlineExpression, missingTestMethodAttributesReference);
						var exceptionMessageConcatExpression = new CodeMethodInvokeExpression(
							targetObject: stringTypeReferenceExpression,
							methodName: nameof(string.Concat),
							parameters: new CodeExpression[] {
									new CodePrimitiveExpression("The following types have an incorrect namespace:"),
									environmentNewlineExpression,
									environmentNewlineExpression,
									stringJoinExpression
							}
						);

						_ = testMethodCodeDeclaration.Statements.Add(new CodeConditionStatement(
							condition: new CodeBinaryOperatorExpression(
								left: new CodePropertyReferenceExpression(missingTestMethodAttributesReference, "Count"),
								op: CodeBinaryOperatorType.GreaterThan,
								right: zeroLiteral
							),
							trueStatements: new[] {
									new CodeThrowExceptionStatement(new CodeObjectCreateExpression("AssertFailedException", exceptionMessageConcatExpression))
							}
						));
					}
				}

				fileExtension = codeProvider.FileExtension;

				var codeCompileUnit = new CodeCompileUnit();

				var namespaceCodeSection = new CodeNamespace();
				_ = codeCompileUnit.Namespaces.Add(namespaceCodeSection);

				_ = namespaceCodeSection.Comments.Add(new CodeCommentStatement("Generated by the Rhinobyte.Extensions.TestTools.GenerateProjectStructureTestsTask class."));

				foreach (var namespaceImport in NamespaceUsingStatements)
					namespaceCodeSection.Imports.Add(new CodeNamespaceImport(namespaceImport));

				if (!string.IsNullOrWhiteSpace(GeneratedClassNamespaceToUse))
					namespaceCodeSection.Name = GeneratedClassNamespaceToUse;

				var classCodeStatement = new CodeTypeDeclaration(testClassName)
				{
					IsClass = true,
					IsPartial = true,
					TypeAttributes = TypeAttributes.Class | TypeAttributes.Public
				};
				_ = namespaceCodeSection.Types.Add(classCodeStatement);

				_ = classCodeStatement.CustomAttributes.Add(new CodeAttributeDeclaration(
					attributeType: new CodeTypeReference(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute)),
					arguments: new CodeAttributeArgument[] {
						new CodeAttributeArgument(new CodePrimitiveExpression(nameof(GenerateProjectStructureTestsTask))),
						new CodeAttributeArgument(new CodePrimitiveExpression(typeof(GenerateProjectStructureTestsTask).Assembly.GetName().Version?.ToString() ?? "1.0.0")),
					}
				));
				_ = classCodeStatement.CustomAttributes.Add(new CodeAttributeDeclaration("TestClass"));

				foreach (var testMethodDeclaration in testMethodDeclarations)
					_ = classCodeStatement.Members.Add(testMethodDeclaration);


				// Generate the code
				_ = stringBuilder.Clear();
				using var stringWriter = new StringWriter(stringBuilder, CultureInfo.CurrentCulture);
				codeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, stringWriter, new CodeGeneratorOptions());

				return stringBuilder.ToString();
			}
			finally
			{
				codeProvider?.Dispose();
			}
		}

		internal static string GetNamespaceValidationCheckTestMethodName(ITaskItem namespaceValidationCheckItem, int itemIndex, int totalItemCount)
		{
			var testMethodName = namespaceValidationCheckItem.GetMetadata(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.FullTestMethodName);
			if (!string.IsNullOrWhiteSpace(testMethodName))
				return testMethodName;

			var libraryName = namespaceValidationCheckItem.GetMetadata(TaskItemMetadataNames.ClassLibraryNamespaceValidationTestItem.LibraryName);
			if (!string.IsNullOrWhiteSpace(libraryName))
			{
				return $"{libraryName}_library_types_all_match_one_of_the_valid_namespaces";
			}

			return totalItemCount > 1
				? $"Library_types_all_match_one_of_the_valid_namespaces_{itemIndex}"
				: "Library_types_all_match_one_of_the_valid_namespaces";
		}

		private MessageImportance ParseMessageImportance()
		{
			if (string.IsNullOrEmpty(LoggingMessageImportance))
				return MessageImportance.Normal;

			try
			{
				// Parse the raw importance string into a strongly typed enumeration.e
				if (Enum.TryParse<MessageImportance>(LoggingMessageImportance, out var messageImportance))
					return messageImportance;
			}
			catch (ArgumentException) { }

			return MessageImportance.Normal;
		}
	}
}
