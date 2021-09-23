using Rhinobyte.Extensions.DependencyInjection;

namespace ExampleLibrary1
{
	public interface IExplicitConstructorType { }

	public class ExplicitConstructorType : IExplicitConstructorType
	{
		public ExplicitConstructorType(IManuallyConfiguredType manuallyConfiguredType)
		{
			ConstructorUsedIndex = 1;
		}

		public ExplicitConstructorType(ISomethingService somethingService)
		{
			ConstructorUsedIndex = 2;
		}

		[DependencyInjectionConstructor]
		public ExplicitConstructorType(ISomethingOptions somethingOptions)
		{
			SomethingOptions = somethingOptions;
			ConstructorUsedIndex = 3;
		}

		public ExplicitConstructorType(
			IManuallyConfiguredType manuallyConfiguredType,
			ISomethingOptions somethingOptions,
			ISomethingService somethingService,
			ITypeWithRegisterAttribute? typeWithRegisterAttribute = null)
		{
			SomethingOptions = somethingOptions;
			ConstructorUsedIndex = 4;
		}

		public int ConstructorUsedIndex { get; }

		public ISomethingOptions? SomethingOptions { get; }
	}
}
