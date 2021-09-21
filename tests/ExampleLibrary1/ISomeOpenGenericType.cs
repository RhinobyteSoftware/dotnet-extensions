namespace ExampleLibrary1
{
	public interface ISomeNullableOpenGenericType<TOne, TTwo>
		where TOne : class?
		where TTwo : class?
	{
		public TOne One { get; }
		public TTwo Two { get; }
	}

	public interface ISomeNullableOpenGenericTypeTwo<TOne, TTwo>
	{
		public TOne One { get; }
		public TTwo Two { get; }
	}

	public interface ISomeOpenGenericType<TOne, TTwo>
		where TOne : class
		where TTwo : class
	{
		public TOne One { get; }
		public TTwo Two { get; }
	}

	public class PartiallyOpenGeneric<TTwo> : ISomeOpenGenericType<object, TTwo>
		where TTwo : class
	{
		public PartiallyOpenGeneric(object one, TTwo two)
		{
			One = one;
			Two = two;
		}

		public object One { get; }
		public TTwo Two { get; }
	}

	public class ClassThatClosedOpenGenericOne : ISomeOpenGenericType<object, object>
	{
		public ClassThatClosedOpenGenericOne(object one, object two)
		{
			One = one;
			Two = two;
		}

		public object One { get; }
		public object Two { get; }
	}

	public class ClassThatClosedOpenGenericTwo : PartiallyOpenGeneric<object>
	{
		public ClassThatClosedOpenGenericTwo(object one, object two)
			: base(one, two)
		{
		}
	}

	public class ClassWithGenericMethod
	{
		public ISomeNullableOpenGenericType<object?, object>? MethodWithGenericResult1()
		{
			return null;
		}

		public ISomeNullableOpenGenericTypeTwo<TOne, TTwo>? MethodWithGenericResult2<TOne, TTwo>()
		{
			return null;
		}

		public ISomeNullableOpenGenericTypeTwo<TOne, TTwo>? MethodWithGenericResult3<TOne, TTwo>(TOne? paramOne, TTwo? paramTwo)
		{
			return null;
		}

		public (TOne, TTwo) MethodWithGenericResult4<TOne, TTwo>(TOne paramOne, TTwo paramTwo)
			where TOne : notnull
			where TTwo : notnull
		{
			return (paramOne, paramTwo);
		}

		public TSomething MethodWithGenericConstraints<TSomething, TSomethingElse>(TSomething something, TSomethingElse somethingElse)
			where TSomething : notnull, System.Enum
			where TSomethingElse : System.Enum
		{
			return something;
		}
	}
}
