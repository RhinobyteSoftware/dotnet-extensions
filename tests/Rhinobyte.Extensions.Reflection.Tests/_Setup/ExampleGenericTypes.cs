using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public struct GenericStruct<T>
		where T : struct, IConvertible
	{
		public T Something { get; set; }

		private bool _doGet;
		public T? TryGetSomething()
		{
			_doGet = !_doGet;
			return _doGet ? Something : default(T);
		}
	}

	public abstract class ExampleGenericRootType<T, U, V, W>
		where T : System.Enum
		where U : class
		where V : struct, IConvertible
		where W : IDictionary<T, V?>
	{

		protected ExampleGenericRootType(
			T property1,
			U property2,
			V property3,
			W property4)
		{
			Property1 = property1;
			Property2 = property2;
			Property3 = property3;
			Property4 = property4;
		}

		public T Property1 { get; set; }
		public U? Property2 { get; set; }
		public V Property3 { get; set; }
		public W Property4 { get; set; }
	}

	public class ExampleGenericType<T, U, V> : ExampleGenericRootType<T, U, V, Dictionary<T, V?>>
		where T : System.Enum
		where U : class
		where V : struct, IConvertible
	{
		public ExampleGenericType(T property1, U property2, V property3)
			: base(property1, property2, property3, new Dictionary<T, V?>())
		{
		}

		private static bool _doCreate = true;
		public static ExampleGenericType<T, U, V>? TryCreate(T property1, U? property2, V property3)
		{
			if (_doCreate && property2 != null)
			{
				_doCreate = false;
				return new ExampleGenericType<T, U, V>(property1, property2, property3);
			}

			_doCreate = true;
			return null;
		}

		public virtual async Task<ExampleGenericType<T, U, V>?> SomeInstanceMethodAsync(T property1, U? property2, V property3)
		{
			await Task.Delay(500).ConfigureAwait(false);
			return null;
		}
	}

	public class ClosedGenericType : ExampleGenericType<DateTimeKind, string, DateTime>
	{
		public ClosedGenericType(DateTimeKind property1, string property2, DateTime property3)
			: base(property1, property2, property3)
		{
		}

		private static bool _doCreate = true;
		public static new ClosedGenericType? TryCreate(DateTimeKind property1, string property2, DateTime property3)
		{
			if (_doCreate)
			{
				_doCreate = false;
				return new ClosedGenericType(property1, property2, property3);
			}

			_doCreate = true;
			return null;
		}
	}

	public static class StaticGenericMethods<TClass>
		where TClass : struct
	{
		public static TResult? TrySelectItem<TResult, T1, T2>(T1? item1, T2? item2)
			where T1 : TResult
			where T2 : TResult
		{
			if (item1 != null)
			{
				return item1;
			}

			if (item2 != null)
			{
				return item2;
			}

			return default(TResult);
		}
	}
}
