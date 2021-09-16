using Rhinobyte.Extensions.DependencyInjection;

namespace ExampleLibrary1
{
	[RegisterForDependencyInjection(typeof(ClassWithRegisterAttribute))]
	public class ClassWithRegisterAttribute
	{
	}
}
