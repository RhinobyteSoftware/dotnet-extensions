using Rhinobyte.Extensions.Reflection.AssemblyScanning;

[assembly: IgnoreAssemblyScanner]

namespace ExampleIgnoredAttributeAssembly
{
	[IgnoreAssemblyScanner]
	public class DummyClass
	{
	}
}
