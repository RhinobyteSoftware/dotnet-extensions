using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using Rhinobyte.Extensions.TestTools;
using System;
using System.Reflection;
using System.Reflection.Emit;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage;

[TestClass]
public class MemberReferenceMatchInfoTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void Constructor_throws_ArgumentNullException_for_a_number_member_reference()
	{
		Invoking(() => new MemberReferenceMatchInfo(null!, false, false))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*memberInfoToLookFor*");
	}

	[TestMethod]
	public void DoesInstructionReferenceMatch_behaves_as_expected()
	{
		var baseClassPublicIntProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.InheritedIntProperty), BindingFlags.Public | BindingFlags.Instance);
		var systemUnderTest = new MemberReferenceMatchInfo(baseClassPublicIntProperty!, false, false);

		systemUnderTest.DoesInstructionReferenceMatch(instructionMemberReference: null!).Should().BeFalse();

		systemUnderTest.DoesInstructionReferenceMatch(ilInstruction: null!).Should().BeFalse();
		systemUnderTest.DoesInstructionReferenceMatch(new FieldReferenceInstruction(0, 0, OpCodes.Nop, null)).Should().BeFalse();
		systemUnderTest.DoesInstructionReferenceMatch(new MethodReferenceInstruction(0, 0, OpCodes.Nop, null)).Should().BeFalse();
		systemUnderTest.DoesInstructionReferenceMatch(new TypeReferenceInstruction(0, 0, OpCodes.Nop, null!)).Should().BeFalse();
		systemUnderTest.DoesInstructionReferenceMatch(new UnknownMemberReferenceInstruction(0, 0, OpCodes.Nop, null!)).Should().BeFalse();
		systemUnderTest.DoesInstructionReferenceMatch(new SimpleInstruction(0, 0, OpCodes.Nop)).Should().BeFalse();

		systemUnderTest.DoesInstructionReferenceMatch(new MethodReferenceInstruction(0, 0, OpCodes.Nop, baseClassPublicIntProperty!.GetMethod)).Should().BeTrue();
		systemUnderTest.DoesInstructionReferenceMatch(new MethodReferenceInstruction(0, 0, OpCodes.Nop, baseClassPublicIntProperty!.SetMethod)).Should().BeTrue();

		systemUnderTest = new MemberReferenceMatchInfo(typeof(MemberReferenceMatchInfoTests), false, false);
		systemUnderTest.DoesInstructionReferenceMatch(typeof(MemberReferenceMatchInfoTests)).Should().BeTrue();
		systemUnderTest.DoesInstructionReferenceMatch(new TypeReferenceInstruction(0, 0, OpCodes.Nop, typeof(MemberReferenceMatchInfoTests))).Should().BeTrue();
		systemUnderTest.DoesInstructionReferenceMatch(new TypeReferenceInstruction(0, 0, OpCodes.Nop, typeof(MethodBodyParser))).Should().BeFalse();
	}

	[TestMethod]
	public void Initialize_sets_the_expected_property_values_for_a_FieldInfo_reference()
	{
		var mockFieldReference = new Mock<FieldInfo>();
		mockFieldReference.SetupAllProperties();
		VerifyInitializeBehavior(mockFieldReference.Object, false, null, null, null, null);

		var inheritedFieldFromBase = typeof(ExampleInheritedMemberBaseClass).GetField("inheritedStringField", BindingFlags.Instance | BindingFlags.NonPublic);
		var inheritedFieldFromChild = typeof(ExampleInheritedMemberSubClass).GetField("inheritedStringField", BindingFlags.Instance | BindingFlags.NonPublic);
		var inheritedFieldFromGrandChild = typeof(GrandChildMemberClass).GetField("inheritedStringField", BindingFlags.Instance | BindingFlags.NonPublic);

		VerifyInitializeBehavior(inheritedFieldFromBase, false, null, null, 0, null);
		VerifyInitializeBehavior(inheritedFieldFromChild, false, null, null, 0, inheritedFieldFromBase);
		VerifyInitializeBehavior(inheritedFieldFromGrandChild, false, null, null, 0, inheritedFieldFromBase);

		var systemUnderTest = new MemberReferenceMatchInfo(inheritedFieldFromGrandChild!, false, false);
		VerifyInitializeBehavior(systemUnderTest, false, null, null, null, null);

		var staticField = typeof(ExampleInheritedMemberBaseClass).GetField(nameof(ExampleInheritedMemberBaseClass.staticTimeSpanField), BindingFlags.Static | BindingFlags.Public);
		VerifyInitializeBehavior(staticField, true, null, null, 0, null);

		var fieldFromBaseNotYetHidden = typeof(ExampleInheritedMemberBaseClass).GetField("baseIntFieldThatWillBeHidden", BindingFlags.Instance | BindingFlags.Public);
		fieldFromBaseNotYetHidden!.DeclaringType.Should().Be<ExampleInheritedMemberBaseClass>();
		VerifyInitializeBehavior(fieldFromBaseNotYetHidden, false, null, null, 0, null);

		var fieldFromChildThatHidesBase = typeof(ExampleInheritedMemberSubClass).GetField("baseIntFieldThatWillBeHidden", BindingFlags.Instance | BindingFlags.Public);
		fieldFromChildThatHidesBase!.DeclaringType.Should().Be<ExampleInheritedMemberSubClass>();
		VerifyInitializeBehavior(fieldFromChildThatHidesBase, false, null, null, 1, null);

		var fieldFromGrandChildThatHidesBase = typeof(GrandChildMemberClass).GetField("baseIntFieldThatWillBeHidden", BindingFlags.Instance | BindingFlags.Public);
		fieldFromGrandChildThatHidesBase!.DeclaringType.Should().Be<GrandChildMemberClass>();

		// In this case there should be 3 total base members to check
		// 2 from the child type (the new field and the inherited field) + 1 from the base type
		VerifyInitializeBehavior(fieldFromGrandChildThatHidesBase, false, null, null, 3, null);
	}

	[TestMethod]
	public void Initialize_sets_the_expected_property_values_for_a_MethodBase_reference()
	{
		var mockMethod = new MockMethodBase(declaringType: null, "MockMethod");
		VerifyInitializeBehavior(mockMethod, false, null, null, null, null);

		var baseMethod = typeof(ExampleInheritedMemberBaseClass).GetMethod(nameof(ExampleInheritedMemberBaseClass.PublicVirtualMethod));
		var childMethod = typeof(ExampleInheritedMemberSubClass).GetMethod(nameof(ExampleInheritedMemberSubClass.PublicVirtualMethod));
		var grandChildMethod = typeof(GrandChildMemberClass).GetMethod(nameof(GrandChildMemberClass.PublicVirtualMethod));

		VerifyInitializeBehavior(baseMethod, false, null, null, 0, null);
		VerifyInitializeBehavior(childMethod, false, null, null, 1, null);
		VerifyInitializeBehavior(grandChildMethod, false, null, null, 2, null);

		var inheritedMethodFromBase = typeof(ExampleInheritedMemberBaseClass).GetMethod("InheritedMethod", BindingFlags.Instance | BindingFlags.NonPublic);
		var inheritedMethodFromChild = typeof(ExampleInheritedMemberSubClass).GetMethod("InheritedMethod", BindingFlags.Instance | BindingFlags.NonPublic);
		var inheritedMethodFromGrandChild = typeof(GrandChildMemberClass).GetMethod("InheritedMethod", BindingFlags.Instance | BindingFlags.NonPublic);

		VerifyInitializeBehavior(inheritedMethodFromBase, false, null, null, 0, null);
		VerifyInitializeBehavior(inheritedMethodFromChild, false, null, null, 0, inheritedMethodFromBase);
		VerifyInitializeBehavior(inheritedMethodFromGrandChild, false, null, null, 0, inheritedMethodFromBase);

		var systemUnderTest = new MemberReferenceMatchInfo(inheritedMethodFromGrandChild!, false, false);
		VerifyInitializeBehavior(systemUnderTest, false, null, null, null, null);
	}

	[TestMethod]
	public void Initialize_sets_the_expected_property_values_for_a_PropertyInfo_reference()
	{
		var baseClassPublicIntProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.InheritedIntProperty), BindingFlags.Public | BindingFlags.Instance);
		var baseClassReadOnlyProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.InheritedPublicPropertyWithNoSetter), BindingFlags.Public | BindingFlags.Instance);
		var baseClassWriteOnlyProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.WriteOnlyProperty), BindingFlags.Public | BindingFlags.Instance);

		VerifyInitializeBehavior(baseClassPublicIntProperty, false, baseClassPublicIntProperty!.GetMethod, baseClassPublicIntProperty!.SetMethod, 0, null);
		VerifyInitializeBehavior(baseClassReadOnlyProperty, false, baseClassReadOnlyProperty!.GetMethod, null, 0, null);
		VerifyInitializeBehavior(baseClassWriteOnlyProperty, false, null, baseClassWriteOnlyProperty!.SetMethod, 0, null);


		var baseClassStaticProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.StaticPropertyOnBaseClass), BindingFlags.Public | BindingFlags.Static);
		var staticReadOnlyProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.StaticReadOnlyProperty), BindingFlags.Public | BindingFlags.Static);
		var staticWriteOnlyProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.StaticWriteOnlyProperty), BindingFlags.Public | BindingFlags.Static);
		VerifyInitializeBehavior(baseClassStaticProperty, true, baseClassStaticProperty!.GetMethod, baseClassStaticProperty!.SetMethod, null, null);
		VerifyInitializeBehavior(staticReadOnlyProperty, true, staticReadOnlyProperty!.GetMethod, null, null, null);
		VerifyInitializeBehavior(staticWriteOnlyProperty, true, null, staticWriteOnlyProperty!.SetMethod, null, null);

		var sutDontMatchBaseMembers = new MemberReferenceMatchInfo(baseClassPublicIntProperty!, false, false);
		VerifyInitializeBehavior(sutDontMatchBaseMembers, false, baseClassPublicIntProperty!.GetMethod, baseClassPublicIntProperty?.SetMethod, null, null);

		var inheritedClassPublicIntProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.InheritedIntProperty), BindingFlags.Public | BindingFlags.Instance);
		var inheritedClassReadOnlyProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.InheritedPublicPropertyWithNoSetter), BindingFlags.Public | BindingFlags.Instance);
		var inheritedClassWriteOnlyProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.WriteOnlyProperty), BindingFlags.Public | BindingFlags.Instance);

		VerifyInitializeBehavior(inheritedClassPublicIntProperty, false, inheritedClassPublicIntProperty!.GetMethod, inheritedClassPublicIntProperty!.SetMethod, 0, baseClassPublicIntProperty);
		VerifyInitializeBehavior(inheritedClassReadOnlyProperty, false, inheritedClassReadOnlyProperty!.GetMethod, null, 0, baseClassReadOnlyProperty);
		VerifyInitializeBehavior(inheritedClassWriteOnlyProperty, false, null, inheritedClassWriteOnlyProperty!.SetMethod, 0, baseClassWriteOnlyProperty);

		var baseVirtualProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty("ProtectedVirtualShortProperty", BindingFlags.NonPublic | BindingFlags.Instance);
		baseVirtualProperty.Should().NotBeNull();
		VerifyInitializeBehavior(baseVirtualProperty, false, baseVirtualProperty!.GetMethod, baseVirtualProperty!.SetMethod, 0, null);

		var overridenVirtualProperty = typeof(ExampleInheritedMemberSubClass).GetProperty("ProtectedVirtualShortProperty", BindingFlags.NonPublic | BindingFlags.Instance);
		overridenVirtualProperty.Should().NotBeNull();
		VerifyInitializeBehavior(overridenVirtualProperty, false, overridenVirtualProperty!.GetMethod, overridenVirtualProperty!.SetMethod, 1, null);
	}




	/******     TEST SETUP     *****************************
	 *******************************************************/
	[NotATestMethod]
	public void VerifyInitializeBehavior(
		MemberInfo? memberInfoToTestWith,
		bool expectedIsStaticValue,
		MethodInfo? expectedPropertyGetMethod,
		MethodInfo? expectedPropertySetMethod,
		int? expectedBaseMembersCount,
		MemberInfo? expectedDeclaringMemberTypeToCheck)
		=> VerifyInitializeBehavior(new MemberReferenceMatchInfo(memberInfoToTestWith!, true, true), expectedIsStaticValue, expectedPropertyGetMethod, expectedPropertySetMethod, expectedBaseMembersCount, expectedDeclaringMemberTypeToCheck);

	[NotATestMethod]
	public void VerifyInitializeBehavior(
		MemberReferenceMatchInfo systemUnderTest,
		bool expectedIsStaticValue,
		MethodInfo? expectedPropertyGetMethod,
		MethodInfo? expectedPropertySetMethod,
		int? expectedBaseMembersCount,
		MemberInfo? expectedDeclaringMemberTypeToCheck)
	{
		systemUnderTest.IsInitialized.Should().BeFalse();
		systemUnderTest.Initialize();
		systemUnderTest.DidSwallowException.Should().BeFalse();
		systemUnderTest.IsInitialized.Should().BeTrue();

		systemUnderTest.IsStaticMember.Should().Be(expectedIsStaticValue);
		systemUnderTest.PropertyGetMethod.Should().BeSameAs(expectedPropertyGetMethod);
		systemUnderTest.PropertySetMethod.Should().BeSameAs(expectedPropertySetMethod);

		if (expectedBaseMembersCount == null)
			systemUnderTest.BaseClassMembersToCheck.Should().BeNull();
		else
			systemUnderTest.BaseClassMembersToCheck?.Count.Should().Be(expectedBaseMembersCount);

		if (expectedDeclaringMemberTypeToCheck == null)
			systemUnderTest.DeclaringTypeMemberToCheck.Should().BeNull();
		else
			systemUnderTest.DeclaringTypeMemberToCheck!.MemberInfoToLookFor.Should().Be(expectedDeclaringMemberTypeToCheck);
	}
}
