using Rhinobyte.Extensions.DependencyInjection;

namespace ExampleLibrary1
{
	[RegisterForDependencyInjection(typeof(ClassWithRegisterAttribute))]
	public class ClassWithRegisterAttribute
	{
	}

	[RegisterForDependencyInjection(typeof(ClassWithRegisterAndConstructorSelectionAttributes))]
	public class ClassWithRegisterAndConstructorSelectionAttributes
	{
		public ClassWithRegisterAndConstructorSelectionAttributes(string something, string somethingElse) { }

		public ClassWithRegisterAndConstructorSelectionAttributes(ISomethingOptions somethingOptions) { }

		[DependencyInjectionConstructor]
		public ClassWithRegisterAndConstructorSelectionAttributes(ISomethingService somethingService, ITypeWithRegisterAttribute typeWithRegisterAttribute) { }
	}
}
