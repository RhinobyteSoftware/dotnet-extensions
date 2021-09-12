namespace Rhinobyte.Extensions.DependencyInjection.Tests
{
	public class ClassWithAmbiguousConstructorDependencies
	{
		public ClassWithAmbiguousConstructorDependencies(IDependency1 dependency1, IDependency2 dependency2) { }

		public ClassWithAmbiguousConstructorDependencies(IDependency3 dependency3, IDependency4 dependency4, IDependency5 dependency5) { }
	}

	public class ClassWithAmbiguousConstructorDependenciesDecorated
	{
		[DependencyInjectionConstructor]
		public ClassWithAmbiguousConstructorDependenciesDecorated(IDependency1 dependency1, IDependency2 dependency2) { }

		public ClassWithAmbiguousConstructorDependenciesDecorated(IDependency3 dependency3, IDependency4 dependency4, IDependency5 dependency5) { }
	}

	public class ClassWithConstructorSelectionAttributeOnInvalidConstructor
	{
		[DependencyInjectionConstructor]
		public ClassWithConstructorSelectionAttributeOnInvalidConstructor(string something, string somethingElse)
		{

		}

		public ClassWithConstructorSelectionAttributeOnInvalidConstructor(ISomethingOptions somethingOptions)
		{

		}
	}

	public class ClassWithConstructorSelectionAttributeTwoConstructors
	{
		public ClassWithConstructorSelectionAttributeTwoConstructors(string something, string somethingElse)
		{

		}

		[DependencyInjectionConstructor]
		public ClassWithConstructorSelectionAttributeTwoConstructors(ISomethingOptions somethingOptions)
		{

		}
	}

	public class ClassWithConstructorSelectionAttributeThreeConstructors
	{
		public ClassWithConstructorSelectionAttributeThreeConstructors(string something, string somethingElse)
		{

		}

		[DependencyInjectionConstructor]
		public ClassWithConstructorSelectionAttributeThreeConstructors(ISomethingOptions somethingOptions)
		{

		}

		public ClassWithConstructorSelectionAttributeThreeConstructors(ISomethingOptions somethingOptions, IDependency1 dependency1, IDependency2 dependency2, IDependency3 dependency3)
		{

		}
	}

	public class ClassWithMultipleCompatibleConstructors
	{
		public ClassWithMultipleCompatibleConstructors(IDependency1 dependency1) { }

		public ClassWithMultipleCompatibleConstructors(IDependency1 dependency1, IDependency2 dependency2) { }

		public ClassWithMultipleCompatibleConstructors(IDependency1 dependency1, IDependency2 dependency2, IDependency3 dependency3) { }
	}

	public class ClassWithMultipleConstructorSelectionAttributes
	{
		[DependencyInjectionConstructor]
		public ClassWithMultipleConstructorSelectionAttributes(IDependency1 dependency1) { }

		[DependencyInjectionConstructor]
		public ClassWithMultipleConstructorSelectionAttributes(IDependency2 dependency2) { }
	}

	public class ClassWithSingleConstructor
	{
		public ClassWithSingleConstructor(IDependency1 dependency1) { }
	}


	public interface IDependency1 { }

	public interface IDependency2 { }

	public interface IDependency3 { }

	public interface IDependency4 { }

	public interface IDependency5 { }

	public interface ISomethingOptions
	{
		string? Something { get; }
		string? SomethingElse { get; }
	}

	public class SomethingOptions : ISomethingOptions
	{
		public string? Something { get; set; }
		public string? SomethingElse { get; set; }
	}

	public class SubclassWithNoConstructorSelectionAttribute : ClassWithConstructorSelectionAttributeTwoConstructors
	{
		public SubclassWithNoConstructorSelectionAttribute(ISomethingOptions somethingOptions)
			: base(somethingOptions)
		{

		}
	}
}
