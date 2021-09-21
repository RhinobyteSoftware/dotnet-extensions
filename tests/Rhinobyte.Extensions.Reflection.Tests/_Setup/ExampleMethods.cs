using System;

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
			sum1 = sum1 * sum1;
			var sum2 = simpleInstructionValue + byteValue + signedByteValue + shortValue + unsignedShortValue + sum1 + unsignedIntValue + longValue + (long)unsignedLongValue;

			double doubleValue = 0.5d;
			float floatValue = 0.5f;
			var sum3 = sum2 + doubleValue + floatValue;

			var stringValue = "SomeString";
			var stringValue2 = $"{prefix}:  {stringValue} {sum3}  - {typeof(ExampleMethods).FullName}.{nameof(LocalStringField)}: {this.LocalStringField}";

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
			if (parameter == null) throw new ArgumentNullException(nameof(parameter));

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
