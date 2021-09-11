using Rhinobyte.Extensions.DependencyInjection;

[assembly: IgnoreAssemblyScanner]

namespace ExampleIgnoredAttributeAssembly
{
	[IgnoreAssemblyScanner]
	public class DummyClass
    {
    }
}
