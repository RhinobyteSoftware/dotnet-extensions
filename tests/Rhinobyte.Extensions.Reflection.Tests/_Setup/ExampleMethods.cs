using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public class ExampleMethods
	{
		public static int AddLocalVariables_For_5_And_15()
		{
			var value1 = 5;
			var value2 = 10;
			return value1 + value2;
		}

		public static int AddTwoValues(int value1, int value2)
		{
			return value1 + value2;
		}

		public static int AddTwoValues_Of_7_And_14()
		{
			return AddTwoValues(7, 14);
		}

		public static int AddTwoValues_Of_7_And_14_Using_Delegate_Function()
		{
			Func<int, int, int> addDelegate = AddTwoValues;
			return addDelegate(7, 14);
		}

		public int LocalIntegerProperty { get; set; } = -1;
		public int InstanceMethodWithLotsOfParameters(int value1, int value2, int value3, int value4, int value5, int value6, int value7, int value8, int value9, int value10)
		{
			var sum = this.LocalIntegerProperty + value1 + value2 + value3 + value4 + value5 + value6 + value7 + value8 + value9 + value10;
			return sum;
		}

		public readonly string LocalStringField = nameof(LocalStringField);
		public string MethodWithEachTypeOfInstruction(string prefix)
		{
			var simpleInstructionValue = 5;

			// Need to use values > 8 and in the range of the actual type for the rest of these or else it will end up using simpler instructions than expected
#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE0004 // Remove Unnecessary Cast
			byte byteValue = (byte)12;
			sbyte signedByteValue = (sbyte)-12;
			short shortValue = (short)-300;
			ushort unsignedShortValue = (ushort)65000;
			int intValue = -70_000;
			uint unsignedIntValue = 70_000U;
			// Int32.MaxValue == 2_147_483_647;
			long longValue = -3_000_000_000L;
			// Int64.MaxValue == 9_223_372_036_854_775_807
			ulong unsignedLongValue = 10_000_000_000_000_000_000UL;
			unsignedLongValue -= long.MaxValue;
			var sum1 = AddTwoValues(intValue, intValue);
#pragma warning disable IDE0054 // Use compound assignment
			sum1 = sum1 * sum1;
#pragma warning restore IDE0054 // Use compound assignment
			var sum2 = simpleInstructionValue + byteValue + signedByteValue + shortValue + unsignedShortValue + sum1 + unsignedIntValue + longValue + (long)unsignedLongValue;

			double doubleValue = 0.5d;
			float floatValue = 0.5f;
			var sum3 = sum2 + doubleValue + floatValue;

			var stringValue = "SomeString";
			var stringValue2 = $"{prefix}:  {stringValue} {sum3}  - {typeof(ExampleMethods).FullName}.{nameof(LocalStringField)}: {this.LocalStringField}";
#pragma warning restore IDE0004 // Remove Unnecessary Cast
#pragma warning restore IDE0007 // Use implicit type

			switch (byteValue)
			{
				case 1:
				case 2:
				case 3:
				case 4:
					stringValue2 = $"{stringValue2}  - switch statement case 1/2/3/4";
					break;

				case 5:
					stringValue2 = $"{stringValue2}  - switch statement case 5";
					break;

				default:
					stringValue2 = $"{stringValue2}  - switch statement default case";
					break;
			}

			var switchCaseString = "test";
			switch (switchCaseString)
			{
				case "test1":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test2":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test3":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test4":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test5":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test6":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test7":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test8":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test9":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test10":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				case "test11":
					stringValue2 = $"{stringValue2}  - string switch statement case {switchCaseString}";
					break;

				default:
					stringValue2 = $"{stringValue2}  - string switch statement default case";
					break;
			}

			return stringValue2;
		}

		public static void MethodWithForLoopAndContinueStatement()
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

			var invalidTypes = new System.Collections.Generic.List<string>();
			foreach (var libraryType in libraryTypes)
			{
				if (libraryType.IsCompilerGenerated())
					continue;


				var fullTypeName = libraryType?.FullName;
				if (fullTypeName is null)
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

		public void MethodWithGotoLabels()
		{
			var count = 0;

		FirstLabel:
			Console.WriteLine("First Label");

			++count;

			if (count < 3)
				goto FirstLabel;

			SecondLabel:
			Console.WriteLine("Second Label");

			++count;
			if (count < 6)
				goto FirstLabel;
			else if (count < 9)
				goto SecondLabel;
			else
				goto LastLabel;


			LastLabel:
			Console.WriteLine("Last Label");
		}

		public static int OverloadedMethod(int value)
		{
			return value;
		}

		public static int OverloadedMethod(int value1, int value2)
		{
			return value1 + value2;
		}

		public static int OverloadedMethod(int value1, int value2, float value3)
		{
			return value1 + value2 + (int)Math.Floor(value3);
		}

		public static int NullParameterCheck_Type1(string parameter)
		{
			if (parameter is null) throw new ArgumentNullException(nameof(parameter));

			return 5;
		}

		public static int NullParameterCheck_Type2(string parameter)
		{
			_ = parameter ?? throw new ArgumentNullException(nameof(parameter));

			return 5;
		}

		public static int StaticMethodWithLotsOfParameters(int value1, int value2, int value3, int value4, int value5, int value6, int value7, int value8, int value9, int value10)
		{
			var sum = value1 + value2 + value3 + value4 + value5 + value6 + value7 + value8 + value9 + value10;
			return sum;
		}

		public static void UsesConsoleWriteMethods_Bool_Char_And_Int()
		{
			Console.Write(true);
			Console.Write('C');
			Console.Write(5);
		}

		public static int UsesOverloadedMethod1()
		{
			return OverloadedMethod(5);
		}

		public static int UsesOverloadedMethod2()
		{
			return OverloadedMethod(7, 9);
		}

		public static int UsesOverloadedMethod3()
		{
			return OverloadedMethod(2, 3, 4);
		}
	}
}
