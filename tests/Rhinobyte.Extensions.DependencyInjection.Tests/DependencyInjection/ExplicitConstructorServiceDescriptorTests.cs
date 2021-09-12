using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests.DependencyInjection
{
	[TestClass]
	public class ExplicitConstructorServiceDescriptorTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void SelectCustomConstructor_returns_the_expected_result_for_AttributeThenDefaultBehavior_selection_type()
		{
			const ConstructorSelectionType constructorSelectionTypeToTest = ConstructorSelectionType.AttributeThenDefaultBehavior;

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependencies>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependenciesDecorated>(constructorSelectionTypeToTest, ExpectedConstructorResult.AttributeDecorated);

			Invoking(() => ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ClassWithConstructorSelectionAttributeOnInvalidConstructor), constructorSelectionTypeToTest))
				.Should()
				.Throw<ConstructorSelectionFailedException>()
				.WithMessage($"* has a constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)} that requires value type/string parameters that won't be injectable");

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeTwoConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
			// ^^ should be null because it should ignore the constructor with primitive (string) parameters, resulting in a single potential constructor

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeThreeConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.AttributeDecorated);

			TestSelectCustomConstructorBehavior<ClassWithMultipleCompatibleConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			Invoking(() => ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ClassWithMultipleConstructorSelectionAttributes), constructorSelectionTypeToTest))
				.Should()
				.Throw<ConstructorSelectionFailedException>()
				.WithMessage($"* has multiple constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)}");

			TestSelectCustomConstructorBehavior<ClassWithSingleConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<SubclassWithNoConstructorSelectionAttribute>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
		}

		[TestMethod]
		public void SelectCustomConstructor_returns_the_expected_result_for_AttributeThenMostParameters_selection_type()
		{
			const ConstructorSelectionType constructorSelectionTypeToTest = ConstructorSelectionType.AttributeThenMostParametersOnly;

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependencies>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependenciesDecorated>(constructorSelectionTypeToTest, ExpectedConstructorResult.AttributeDecorated);

			Invoking(() => ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ClassWithConstructorSelectionAttributeOnInvalidConstructor), constructorSelectionTypeToTest))
				.Should()
				.Throw<ConstructorSelectionFailedException>()
				.WithMessage($"* has a constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)} that requires value type/string parameters that won't be injectable");

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeTwoConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
			// ^^ should be null because it should ignore the constructor with primitive (string) parameters, resulting in a single potential constructor

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeThreeConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.AttributeDecorated);

			TestSelectCustomConstructorBehavior<ClassWithMultipleCompatibleConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			Invoking(() => ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ClassWithMultipleConstructorSelectionAttributes), constructorSelectionTypeToTest))
				.Should()
				.Throw<ConstructorSelectionFailedException>()
				.WithMessage($"* has multiple constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)}");

			TestSelectCustomConstructorBehavior<ClassWithSingleConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<SubclassWithNoConstructorSelectionAttribute>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
		}

		[TestMethod]
		public void SelectCustomConstructor_returns_the_expected_result_for_AttributeThenMostParametersWhenAmbiguous_selection_type()
		{
			const ConstructorSelectionType constructorSelectionTypeToTest = ConstructorSelectionType.AttributeThenMostParametersWhenAmbiguous;

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependencies>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependenciesDecorated>(constructorSelectionTypeToTest, ExpectedConstructorResult.AttributeDecorated);

			Invoking(() => ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ClassWithConstructorSelectionAttributeOnInvalidConstructor), constructorSelectionTypeToTest))
				.Should()
				.Throw<ConstructorSelectionFailedException>()
				.WithMessage($"* has a constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)} that requires value type/string parameters that won't be injectable");

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeTwoConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
			// ^^ should be null because it should ignore the constructor with primitive (string) parameters, resulting in a single potential constructor

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeThreeConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.AttributeDecorated);

			TestSelectCustomConstructorBehavior<ClassWithMultipleCompatibleConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			Invoking(() => ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(ClassWithMultipleConstructorSelectionAttributes), constructorSelectionTypeToTest))
				.Should()
				.Throw<ConstructorSelectionFailedException>()
				.WithMessage($"* has multiple constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)}");

			TestSelectCustomConstructorBehavior<ClassWithSingleConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<SubclassWithNoConstructorSelectionAttribute>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
		}

		[TestMethod]
		public void SelectCustomConstructor_returns_the_expected_result_for_DefaultBehaviorOnly_selection_type()
		{
			const ConstructorSelectionType constructorSelectionTypeToTest = ConstructorSelectionType.DefaultBehaviorOnly;

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependencies>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependenciesDecorated>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeOnInvalidConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeTwoConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeThreeConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithMultipleCompatibleConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithMultipleConstructorSelectionAttributes>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithSingleConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<SubclassWithNoConstructorSelectionAttribute>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
		}

		[TestMethod]
		public void SelectCustomConstructor_returns_the_expected_result_for_MostParametersOnly_selection_type()
		{
			const ConstructorSelectionType constructorSelectionTypeToTest = ConstructorSelectionType.MostParametersOnly;

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependencies>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependenciesDecorated>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeOnInvalidConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeTwoConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
			// ^^ should be null because it should ignore the constructor with primitive (string) parameters, resulting in a single potential constructor

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeThreeConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithMultipleCompatibleConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithMultipleConstructorSelectionAttributes>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithSingleConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<SubclassWithNoConstructorSelectionAttribute>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
		}

		[TestMethod]
		public void SelectCustomConstructor_returns_the_expected_result_for_MostParametersWhenAmbiguous_selection_type()
		{
			const ConstructorSelectionType constructorSelectionTypeToTest = ConstructorSelectionType.MostParametersWhenAmbiguous;

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependencies>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithAmbiguousConstructorDependenciesDecorated>(constructorSelectionTypeToTest, ExpectedConstructorResult.MostParameters);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeOnInvalidConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeTwoConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
			// ^^ should be null because it should ignore the constructor with primitive (string) parameters, resulting in a single potential constructor

			TestSelectCustomConstructorBehavior<ClassWithConstructorSelectionAttributeThreeConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithMultipleCompatibleConstructors>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithMultipleConstructorSelectionAttributes>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<ClassWithSingleConstructor>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);

			TestSelectCustomConstructorBehavior<SubclassWithNoConstructorSelectionAttribute>(constructorSelectionTypeToTest, ExpectedConstructorResult.Null);
		}




		/******     TEST SETUP     *****************************
		 *******************************************************/
		public enum ExpectedConstructorResult
		{
			Null = 0,
			AttributeDecorated = 1,
			MostParameters = 2
		}

		public static void TestSelectCustomConstructorBehavior<TTestType>(ConstructorSelectionType constructorSelectionTypeToTest, ExpectedConstructorResult expectedResult)
		{
			var selectedConstructor = ExplicitConstructorServiceDescriptor.SelectCustomConstructor(typeof(TTestType), constructorSelectionTypeToTest);

			ConstructorInfo? expectedConstructor;
			switch (expectedResult)
			{
				case ExpectedConstructorResult.Null:
					expectedConstructor = null;
					break;

				case ExpectedConstructorResult.AttributeDecorated:
					expectedConstructor = typeof(TTestType).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
						.Single(constructor => constructor.IsDefined(typeof(DependencyInjectionConstructorAttribute), false));
					break;

				case ExpectedConstructorResult.MostParameters:
					var constructors = typeof(TTestType).GetTypeInfo().DeclaredConstructors
						.Where(constructor => !constructor.GetParameters().Any(parameter => !parameter.IsOptional && parameter.ParameterType.IsValueTypeOrString()))
						.OrderByDescending(constructor => constructor.GetParameters().Length)
						.ToArray();

					expectedConstructor = constructors.First();
					if (constructors.Length > 1 && constructors[1].GetParameters().Length == expectedConstructor.GetParameters().Length)
						expectedConstructor = null;
					break;

				default:
					throw new NotImplementedException($"{nameof(TestSelectCustomConstructorBehavior)} is not implemented for the {nameof(ExpectedConstructorResult)} value of {expectedResult}");
			}

			if (selectedConstructor != expectedConstructor)
				Assert.Fail($"({typeof(TTestType).Name}) {nameof(ExplicitConstructorServiceDescriptor.SelectCustomConstructor)} was expected to return {expectedConstructor?.ToString() ?? "NULL"} but instead returned {selectedConstructor}");
		}
	}
}
