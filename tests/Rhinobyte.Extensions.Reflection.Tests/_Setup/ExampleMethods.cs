using System;

namespace Rhinobyte.Extensions.Reflection.UnitTests.Setup
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
