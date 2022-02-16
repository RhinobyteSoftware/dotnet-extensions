using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;

namespace Rhinobyte.Extensions.TestTools.Tests;

[TestClass]
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class ApartmentStateTestClassAttributeTests
{
	[TestMethod]
	public void Constructors_do_not_throw_for_invalid_arguments()
	{
		// Attribute constructors should not throw to prevent unexpected exceptions by calls to GetCustomAttribute(s)
		new ApartmentStateTestClassAttribute(ApartmentState.Unknown).Should().NotBeNull();
		new ApartmentStateTestClassAttribute(ApartmentState.STA).Should().NotBeNull();
		new ApartmentStateTestClassAttribute(ApartmentState.MTA).Should().NotBeNull();
	}

	[TestMethod]
	public void GetTestMethodAttribute_does_not_wrap_when_the_attribute_is_already_an_ApartmentStateTestMethodAttribute()
	{
		var systemUnderTest = new ApartmentStateTestClassAttribute(ApartmentState.STA);
		var attributeThatShouldNotBeWrapped = new ApartmentStateTestMethodAttribute(ApartmentState.MTA);

		var notWrappedAttribute = systemUnderTest.GetTestMethodAttribute(attributeThatShouldNotBeWrapped);
		notWrappedAttribute.Should().BeSameAs(attributeThatShouldNotBeWrapped);
		notWrappedAttribute.DisplayName.Should().BeNull();
		((ApartmentStateTestMethodAttribute)notWrappedAttribute).TestApartmentState.Should().Be(ApartmentState.MTA);
		((ApartmentStateTestMethodAttribute)notWrappedAttribute).TestMethodAttribute.Should().BeNull();

		attributeThatShouldNotBeWrapped = new ApartmentStateTestMethodAttribute("CustomDisplayName", ApartmentState.MTA);
		notWrappedAttribute = systemUnderTest.GetTestMethodAttribute(attributeThatShouldNotBeWrapped);
		notWrappedAttribute.Should().BeSameAs(attributeThatShouldNotBeWrapped);
		notWrappedAttribute.DisplayName.Should().Be("CustomDisplayName");
	}

	[DataTestMethod]
	[DataRow(ApartmentState.STA)]
	[DataRow(ApartmentState.MTA)]
	public void GetTestMethodAttribute_forwards_the_wrapped_attribute_DisplayName(ApartmentState apartmentStateToTest)
	{
		var systemUnderTest = new ApartmentStateTestClassAttribute(apartmentStateToTest);
		var attributeToWrap = new TestMethodAttribute();
		var wrappedAttribute = systemUnderTest.GetTestMethodAttribute(attributeToWrap);
		wrappedAttribute.Should().BeOfType<ApartmentStateTestMethodAttribute>();
		wrappedAttribute.DisplayName.Should().BeNull();

		attributeToWrap = new TestMethodAttribute("CustomDisplayName");
		wrappedAttribute = systemUnderTest.GetTestMethodAttribute(attributeToWrap);
		wrappedAttribute.Should().BeOfType<ApartmentStateTestMethodAttribute>();
		wrappedAttribute.DisplayName.Should().Be("CustomDisplayName");
	}

	[DataTestMethod]
	[DataRow(ApartmentState.STA)]
	[DataRow(ApartmentState.MTA)]
	public void GetTestMethodAttribute_wraps_a_non_apartment_thread_attribute(ApartmentState apartmentStateToTest)
	{
		var systemUnderTest = new ApartmentStateTestClassAttribute(apartmentStateToTest);
		var attributeToWrap = new TestMethodAttribute();
		var wrappedAttribute = systemUnderTest.GetTestMethodAttribute(attributeToWrap);
		wrappedAttribute.Should().BeOfType<ApartmentStateTestMethodAttribute>();
		((ApartmentStateTestMethodAttribute)wrappedAttribute).TestApartmentState.Should().Be(apartmentStateToTest);
		((ApartmentStateTestMethodAttribute)wrappedAttribute).TestMethodAttribute.Should().BeSameAs(attributeToWrap);
	}
}
