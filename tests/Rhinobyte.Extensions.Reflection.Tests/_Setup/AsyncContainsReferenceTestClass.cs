using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public class AsyncContainsReferenceTestClass
	{
		private int _field1;
		protected int _field2;
		public int _field3;

		public bool Property1 { get; set; }
		public bool Property2 => _field2 > 0;
#pragma warning disable CA1044 // Properties should not be write only
		public bool Property3 { set => _field3 = value ? 10 : 0; }
#pragma warning restore CA1044 // Properties should not be write only

		public void MethodThatIsReferenced()
		{
			if (_field1 == 0)
				_field1 = 100;
		}

		public bool MethodThatIsNotReferenced1() => true;

		public bool MethodThatIsNotReferenced2() => false;

		public int DoSomething()
		{
			System.Console.WriteLine("START OF METHOD");

			MethodThatIsReferenced();

			Property1 = Property2;

			System.Console.WriteLine("MIDDLE OF METHOD");

			var sum = _field1 + _field2;

			System.Console.WriteLine("END OF METHOD");

			return sum;
		}

		public async Task<int> DoSomethingAsync(CancellationToken cancellationToken = default)
		{
			System.Console.WriteLine("START OF METHOD");

			await Task.Yield();

			MethodThatIsReferenced();

			await Task.Delay(250, cancellationToken).ConfigureAwait(false);

			Property1 = Property2;

			System.Console.WriteLine("MIDDLE OF METHOD");

			await Task.Delay(250, cancellationToken).ConfigureAwait(false);

			var sum = _field1 + _field2;

			await Task.Delay(250, cancellationToken).ConfigureAwait(false);

			System.Console.WriteLine("END OF METHOD");

			return sum;
		}
	}
}
