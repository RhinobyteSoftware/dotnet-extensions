using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage;

[TestClass]
public class MethodBodyParserUnitTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/

	[TestMethod]
	public void Constructor_throws_argument_exceptions_for_required_parameter_members()
	{
		Invoking(() => new MethodBodyParser(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null*method*");

		var mockMethodBase = new MockMethodBase("MockMethod");
		Invoking(() => new MethodBodyParser(mockMethodBase))
			.Should()
			.Throw<ArgumentException>()
			.WithMessage("*.GetMethodBody() returned null for the method:*");

		mockMethodBase.SetMethodBody(new MockMethodBody());
		Invoking(() => new MethodBodyParser(mockMethodBase))
			.Should()
			.Throw<ArgumentException>()
			.WithMessage("MethodBody.GetILAsByteArray() returned null for the method:*");

		var actualMethod = typeof(ExampleMethods).GetMethod("AddTwoValues", BindingFlags.Public | BindingFlags.Static);
		mockMethodBase.SetMethodBody(actualMethod!.GetMethodBody());
		Invoking(() => new MethodBodyParser(mockMethodBase))
			.Should()
			.Throw<ArgumentException>()
			.WithMessage("*.Module property is null");

		mockMethodBase = new MockMethodBase(null, "MockMethodNullDeclaringType");
		mockMethodBase.SetMethodBody(actualMethod.GetMethodBody());
		mockMethodBase.SetModule(actualMethod.Module);
		mockMethodBase.DeclaringType.Should().BeNull();

		// Should be able handle the null DeclaringType without throwing
		var methodBodyParser = new MethodBodyParser(mockMethodBase);
		methodBodyParser.Should().NotBeNull();
	}

	[TestMethod]
	public void ContainsReferencesToAll_doesnt_blow_up1()
	{
		var methodReferencesToSearchFor = typeof(System.Console).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

		var methodBodyParser = new MethodBodyParser(_dynamicJumpTableMethod);
		methodBodyParser.ContainsReferencesToAll(methodReferencesToSearchFor, false, false).Should().BeFalse();
	}

	[TestMethod]
	public void ContainsReferenceTo_correctly_matches_property_references1()
	{
		var baseProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.InheritedIntProperty), BindingFlags.Public | BindingFlags.Instance);
		var inheritedProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.InheritedIntProperty), BindingFlags.Public | BindingFlags.Instance);

		var methodThatReferencesBaseThroughInheritedType = typeof(ExampleInheritedMemberStaticClass)
			.GetMethod(nameof(ExampleInheritedMemberStaticClass.MethodThatReferencesTheInheritedPublicProperty), BindingFlags.Static | BindingFlags.Public);

		methodThatReferencesBaseThroughInheritedType!.ContainsReferenceTo(baseProperty!).Should().BeTrue();
		methodThatReferencesBaseThroughInheritedType!.ContainsReferenceTo(inheritedProperty!, matchAgainstDeclaringTypeMember: true).Should().BeTrue();
		methodThatReferencesBaseThroughInheritedType!.ContainsReferenceTo(inheritedProperty!, matchAgainstDeclaringTypeMember: false).Should().BeFalse();

		var methodThatReferencesBaseThroughBaseType = typeof(ExampleInheritedMemberStaticClass)
			.GetMethod(nameof(ExampleInheritedMemberStaticClass.MethodThatReferencesTheBasePublicProperty), BindingFlags.Static | BindingFlags.Public);

		methodThatReferencesBaseThroughBaseType!.ContainsReferenceTo(baseProperty!).Should().BeTrue();
		methodThatReferencesBaseThroughBaseType!.ContainsReferenceTo(inheritedProperty!, matchAgainstDeclaringTypeMember: true).Should().BeTrue();
		methodThatReferencesBaseThroughBaseType!.ContainsReferenceTo(inheritedProperty!, matchAgainstDeclaringTypeMember: false).Should().BeFalse();
	}

	[TestMethod]
	public void ContainsReferenceTo_correctly_matches_property_references2()
	{
		var baseVirtualProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.PublicVirtualBoolProperty), BindingFlags.Public | BindingFlags.Instance);
		var baseVirtualReadOnlyProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.PublicVirtualBoolReadOnlyProperty), BindingFlags.Public | BindingFlags.Instance);
		var baseVirtualWriteOnlyProperty = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.PublicVirtualBoolWriteOnlyProperty), BindingFlags.Public | BindingFlags.Instance);

		var overridenVirtualProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.PublicVirtualBoolProperty), BindingFlags.Public | BindingFlags.Instance);
		var overridenVirtualReadOnlyProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.PublicVirtualBoolReadOnlyProperty), BindingFlags.Public | BindingFlags.Instance);
		var overridenVirtualWriteOnlyProperty = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.PublicVirtualBoolWriteOnlyProperty), BindingFlags.Public | BindingFlags.Instance);

		var methodThatReferencesBaseVirtualProperties = typeof(ExampleInheritedMemberStaticClass)
			.GetMethod(nameof(ExampleInheritedMemberStaticClass.MethodThatReferencesBaseVirtualProperties), BindingFlags.Static | BindingFlags.Public);

		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(baseVirtualProperty!, matchAgainstBaseClassMembers: false).Should().BeTrue();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(baseVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeTrue();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(baseVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeTrue();

		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(baseVirtualProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(baseVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(baseVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();

		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(overridenVirtualProperty!, matchAgainstBaseClassMembers: false).Should().BeFalse();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(overridenVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeFalse();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(overridenVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeFalse();

		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(overridenVirtualProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(overridenVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesBaseVirtualProperties!.ContainsReferenceTo(overridenVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();

		var methodThatReferencesOverrideVirtualProperties = typeof(ExampleInheritedMemberStaticClass)
			.GetMethod(nameof(ExampleInheritedMemberStaticClass.MethodThatReferencesOverrideVirtualProperties), BindingFlags.Static | BindingFlags.Public);

		// For override members the IL reference is to the base member with a CALLVIRT op code, so the reference will still match the base property
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(baseVirtualProperty!, matchAgainstBaseClassMembers: false).Should().BeTrue();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(baseVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeTrue();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(baseVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeTrue();

		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(baseVirtualProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(baseVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(baseVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();

		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(overridenVirtualProperty!, matchAgainstBaseClassMembers: false).Should().BeFalse();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(overridenVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeFalse();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(overridenVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: false).Should().BeFalse();

		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(overridenVirtualProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(overridenVirtualReadOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
		methodThatReferencesOverrideVirtualProperties!.ContainsReferenceTo(overridenVirtualWriteOnlyProperty!, matchAgainstBaseClassMembers: true).Should().BeTrue();
	}

	[TestMethod]
	public void ContainsReferenceTo_correctly_matches_property_references3()
	{
		var basePropertyThatWillBeHidden = typeof(ExampleInheritedMemberBaseClass).GetProperty(nameof(ExampleInheritedMemberBaseClass.PublicPropertyOnBaseThatWillBeHidden), BindingFlags.Public | BindingFlags.Instance);
		basePropertyThatWillBeHidden!.DeclaringType.Should().Be(basePropertyThatWillBeHidden.ReflectedType);
		basePropertyThatWillBeHidden.DeclaringType.Should().Be<ExampleInheritedMemberBaseClass>();

		var subclassPropertyThatHidesBase = typeof(ExampleInheritedMemberSubClass).GetProperty(nameof(ExampleInheritedMemberSubClass.PublicPropertyOnBaseThatWillBeHidden), BindingFlags.Public | BindingFlags.Instance);
		subclassPropertyThatHidesBase!.DeclaringType.Should().Be(subclassPropertyThatHidesBase.ReflectedType);
		subclassPropertyThatHidesBase.DeclaringType.Should().Be<ExampleInheritedMemberSubClass>();

		var methodThatReferencesBaseProperty = typeof(ExampleInheritedMemberStaticClass)
			.GetMethod(nameof(ExampleInheritedMemberStaticClass.MethodThatReferencesBasePropertyNotYetHidden), BindingFlags.Static | BindingFlags.Public);

		methodThatReferencesBaseProperty!.ContainsReferenceTo(basePropertyThatWillBeHidden!, matchAgainstBaseClassMembers: false).Should().BeTrue();
		methodThatReferencesBaseProperty!.ContainsReferenceTo(basePropertyThatWillBeHidden!, matchAgainstBaseClassMembers: true).Should().BeTrue();

		methodThatReferencesBaseProperty!.ContainsReferenceTo(subclassPropertyThatHidesBase!, matchAgainstBaseClassMembers: false).Should().BeFalse();

		// When match against base class members is true, it will find the base class member with the same name so the contains reference result will be true
		methodThatReferencesBaseProperty!.ContainsReferenceTo(subclassPropertyThatHidesBase!, matchAgainstBaseClassMembers: true).Should().BeTrue();

		var methodThatReferencesSubclassPropertyThatHidesBase = typeof(ExampleInheritedMemberStaticClass)
			.GetMethod(nameof(ExampleInheritedMemberStaticClass.MethodThatReferencesSubclassPropertyThatHidesBaseClassProperty), BindingFlags.Static | BindingFlags.Public);

		// For the hidden property reference it should not have an IL instruction that references the base property
		methodThatReferencesSubclassPropertyThatHidesBase!.ContainsReferenceTo(basePropertyThatWillBeHidden!, matchAgainstBaseClassMembers: false).Should().BeFalse();
		methodThatReferencesSubclassPropertyThatHidesBase!.ContainsReferenceTo(basePropertyThatWillBeHidden!, matchAgainstBaseClassMembers: true).Should().BeFalse();

		methodThatReferencesSubclassPropertyThatHidesBase!.ContainsReferenceTo(subclassPropertyThatHidesBase!, matchAgainstBaseClassMembers: false).Should().BeTrue();
		methodThatReferencesSubclassPropertyThatHidesBase!.ContainsReferenceTo(subclassPropertyThatHidesBase!, matchAgainstBaseClassMembers: true).Should().BeTrue();
	}

	[TestMethod]
	public void ContainsReferenceTo_doesnt_blow_up1()
	{
		var methodReferenceToSearchFor = typeof(System.Console).GetMethods(BindingFlags.Public | BindingFlags.Static).First(method => method.Name == "WriteLine");

		var methodBodyParser = new MethodBodyParser(_dynamicJumpTableMethod);
		methodBodyParser.ContainsReferenceTo(methodReferenceToSearchFor, false, false).Should().BeFalse();
	}

	[TestMethod]
	public void ContainsReferenceToAny_doesnt_blow_up1()
	{
		var methodReferencesToSearchFor = typeof(System.Console).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

		var methodBodyParser = new MethodBodyParser(_dynamicJumpTableMethod);
		methodBodyParser.ContainsReferenceToAny(methodReferencesToSearchFor, false, false).Should().BeFalse();
	}

	[TestMethod]
	public void FindMethodOnConstrainingType_handles_overloads_reasonably_well1()
	{
		var interfaceMethods = typeof(IInheritedExampleType).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromInterface = interfaceMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1));
		var doSomethingElse2FromInterface = interfaceMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse2));
		doSomethingElse1FromInterface.GetMethodBody().Should().BeNull();
		doSomethingElse2FromInterface.GetMethodBody().Should().BeNull();

		var abstractBaseMethods = typeof(InheritedAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromAbstractBase = abstractBaseMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1));
		var doSomethingElse2FromAbstractBase = abstractBaseMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse2));
		doSomethingElse1FromAbstractBase.GetMethodBody().Should().BeNull();
		doSomethingElse2FromAbstractBase.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(InheritedAbstractClass), doSomethingElse1FromInterface).Should().BeSameAs(doSomethingElse1FromAbstractBase);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(InheritedAbstractClass), doSomethingElse2FromInterface).Should().BeSameAs(doSomethingElse2FromAbstractBase);

		var childMethods = typeof(ChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromChild = childMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1));
		var doSomethingElse2FromChild = childMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse2));
		doSomethingElse1FromChild.GetMethodBody().Should().NotBeNull();
		doSomethingElse2FromChild.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(ChildOfAbstractClass), doSomethingElse1FromInterface).Should().BeSameAs(doSomethingElse1FromChild);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(ChildOfAbstractClass), doSomethingElse2FromInterface).Should().BeSameAs(doSomethingElse2FromChild);

		var grandChildMethods = typeof(GrandChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromGrandChild = grandChildMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1) && method.DeclaringType == typeof(ChildOfAbstractClass));
		var doSomethingElse2FromGrandChild = grandChildMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse2));
		var doSomethingElseThatHidesOtherMethod = grandChildMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1) && method.DeclaringType == typeof(GrandChildOfAbstractClass));
		doSomethingElse1FromGrandChild.GetMethodBody().Should().NotBeNull();
		doSomethingElse2FromGrandChild.GetMethodBody().Should().NotBeNull();
		doSomethingElseThatHidesOtherMethod.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(GrandChildOfAbstractClass), doSomethingElse1FromInterface).Should().BeSameAs(doSomethingElse1FromGrandChild);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(GrandChildOfAbstractClass), doSomethingElse2FromInterface).Should().BeSameAs(doSomethingElse2FromGrandChild);
	}

	[TestMethod]
	public void FindMethodOnConstrainingType_handles_overloads_reasonably_well2()
	{
		var abstractBaseMethods = typeof(InheritedAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomething1FromAbstractBase = abstractBaseMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomething) && method.GetParameters().Length == 0);
		var doSomething2FromAbstractBase = abstractBaseMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomething) && method.GetParameters().Length > 0);
		var doSomething3FromAbstractBase = abstractBaseMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomethingOverridenInGrandchildButNotChild));
		doSomething1FromAbstractBase.GetMethodBody().Should().BeNull();
		doSomething2FromAbstractBase.GetMethodBody().Should().NotBeNull();
		doSomething3FromAbstractBase.GetMethodBody().Should().NotBeNull();

		var childMethods = typeof(ChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomething1FromChild = childMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomething) && method.GetParameters().Length == 0);
		var doSomething2FromChild = childMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomething) && method.GetParameters().Length > 0);
		var doSomething3FromChild = childMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomethingOverridenInGrandchildButNotChild));
		doSomething1FromChild.GetMethodBody().Should().NotBeNull();
		doSomething2FromChild.GetMethodBody().Should().NotBeNull();
		doSomething3FromChild.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(ChildOfAbstractClass), doSomething1FromAbstractBase).Should().BeSameAs(doSomething1FromChild);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(ChildOfAbstractClass), doSomething2FromAbstractBase).Should().BeSameAs(doSomething2FromChild);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(ChildOfAbstractClass), doSomething3FromAbstractBase).Should().BeSameAs(doSomething3FromChild);

		var grandChildMethods = typeof(GrandChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomething1FromGrandChild = grandChildMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomething) && method.GetParameters().Length == 0);
		var doSomething2FromGrandChild = grandChildMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomething) && method.GetParameters().Length > 0);
		var doSomething3FromGrandChild = grandChildMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomethingOverridenInGrandchildButNotChild));
		doSomething1FromGrandChild.GetMethodBody().Should().NotBeNull();
		doSomething2FromGrandChild.GetMethodBody().Should().NotBeNull();
		doSomething3FromGrandChild.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(GrandChildOfAbstractClass), doSomething1FromAbstractBase).Should().BeSameAs(doSomething1FromGrandChild);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(GrandChildOfAbstractClass), doSomething2FromAbstractBase).Should().BeSameAs(doSomething2FromGrandChild);
		MethodBodyParser.FindMethodOnConstrainingType(typeof(GrandChildOfAbstractClass), doSomething3FromAbstractBase).Should().BeSameAs(doSomething3FromGrandChild);
	}

	[TestMethod]
	public void FindMethodOnConstrainingType_handles_overloads_reasonably_well3()
	{
		var abstractBaseMethods = typeof(InheritedAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromAbstractBase = abstractBaseMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomethingElse1));
		doSomethingElse1FromAbstractBase.GetMethodBody().Should().BeNull();

		var childMethods = typeof(ChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromChild = childMethods.Single(method => method.Name == nameof(InheritedAbstractClass.DoSomethingElse1));
		doSomethingElse1FromChild.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(ChildOfAbstractClass), doSomethingElse1FromAbstractBase).Should().BeSameAs(doSomethingElse1FromChild);

		var grandChildMethods = typeof(GrandChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElse1FromGrandChild = grandChildMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1) && method.DeclaringType == typeof(ChildOfAbstractClass));
		doSomethingElse1FromGrandChild.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(GrandChildOfAbstractClass), doSomethingElse1FromAbstractBase).Should().BeSameAs(doSomethingElse1FromGrandChild);


		var doSomethingElseThatHidesOtherMethodFromGrandChild = grandChildMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1) && method.DeclaringType == typeof(GrandChildOfAbstractClass));
		doSomethingElseThatHidesOtherMethodFromGrandChild.GetMethodBody().Should().NotBeNull();

		var greatGrandChildMethods = typeof(GreatGrandChildOfAbstractClass).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var doSomethingElseThatHidesOtherMethodFromGreatGrandChild = greatGrandChildMethods.Single(method => method.Name == nameof(IInheritedExampleType.DoSomethingElse1));
		doSomethingElseThatHidesOtherMethodFromGreatGrandChild.GetMethodBody().Should().NotBeNull();

		MethodBodyParser.FindMethodOnConstrainingType(typeof(GreatGrandChildOfAbstractClass), doSomethingElseThatHidesOtherMethodFromGrandChild).Should().BeSameAs(doSomethingElseThatHidesOtherMethodFromGreatGrandChild);

		// As the test says, it handles it 'reasonably' well... for hiding via the new keyword we don't bother looking back at the base types so the hidden value would be found in this case
		MethodBodyParser.FindMethodOnConstrainingType(typeof(GreatGrandChildOfAbstractClass), doSomethingElse1FromAbstractBase).Should().BeSameAs(doSomethingElseThatHidesOtherMethodFromGreatGrandChild);
	}

	[TestMethod]
	public void ParseInstructions_handles_instance_method_parameters_correctly()
	{
		var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.InstanceMethodWithLotsOfParameters), BindingFlags.Public | BindingFlags.Instance);
		testMethodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(23);
#else
		instructions.Count.Should().Be(29);
#endif

		var thisKeywordInstruction = instructions.FirstOrDefault(instruction => instruction is ThisKeywordInstruction) as ThisKeywordInstruction;
		thisKeywordInstruction.Should().NotBeNull();
		thisKeywordInstruction!.Method.Should().Be(testMethodInfo);

		var description = new DefaultInstructionFormatter().DescribeInstructions(instructions);
#if IS_RELEASE_TESTING_BUILD
		description.Should().Be(
@"(0) LOAD ARGUMENT (Index 0)  [this keyword]
(1) CALL METHOD  [ExampleMethods.get_LocalIntegerProperty]
(2) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.Int32 value1]
(3) ADD
(4) LOAD ARGUMENT (Index 2)  [Parameter #1]  [ParameterReference: System.Int32 value2]
(5) ADD
(6) LOAD ARGUMENT (Index 3)  [Parameter #2]  [ParameterReference: System.Int32 value3]
(7) ADD
(8) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #3]  [ParameterReference: System.Int32 value4]
(9) ADD
(10) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #4]  [ParameterReference: System.Int32 value5]
(11) ADD
(12) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #5]  [ParameterReference: System.Int32 value6]
(13) ADD
(14) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #6]  [ParameterReference: System.Int32 value7]
(15) ADD
(16) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #7]  [ParameterReference: System.Int32 value8]
(17) ADD
(18) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #8]  [ParameterReference: System.Int32 value9]
(19) ADD
(20) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #9]  [ParameterReference: System.Int32 value10]
(21) ADD
(22) RETURN");
#else
		description.Should().Be(
@"(0) NO-OP
(1) LOAD ARGUMENT (Index 0)  [this keyword]
(2) CALL METHOD  [ExampleMethods.get_LocalIntegerProperty]
(3) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.Int32 value1]
(4) ADD
(5) LOAD ARGUMENT (Index 2)  [Parameter #1]  [ParameterReference: System.Int32 value2]
(6) ADD
(7) LOAD ARGUMENT (Index 3)  [Parameter #2]  [ParameterReference: System.Int32 value3]
(8) ADD
(9) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #3]  [ParameterReference: System.Int32 value4]
(10) ADD
(11) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #4]  [ParameterReference: System.Int32 value5]
(12) ADD
(13) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #5]  [ParameterReference: System.Int32 value6]
(14) ADD
(15) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #6]  [ParameterReference: System.Int32 value7]
(16) ADD
(17) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #7]  [ParameterReference: System.Int32 value8]
(18) ADD
(19) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #8]  [ParameterReference: System.Int32 value9]
(20) ADD
(21) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #9]  [ParameterReference: System.Int32 value10]
(22) ADD
(23) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(25) SET LOCAL VARIABLE (Index 1)  [Of type Int32]
(26) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 27]
(27) LOAD LOCAL VARIABLE (Index 1)  [Of type Int32]
(28) RETURN");
#endif
	}

	[TestMethod]
	public void ParseInstructions_handles_static_method_parameters_correctly()
	{
		var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.StaticMethodWithLotsOfParameters), BindingFlags.Public | BindingFlags.Static);
		testMethodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(20);
#else
		instructions.Count.Should().Be(26);
#endif

		var description = new DefaultInstructionFormatter().DescribeInstructions(instructions);

#if IS_RELEASE_TESTING_BUILD
		description.Should().Be(
@"(0) LOAD ARGUMENT (Index 0)  [Parameter #0]  [ParameterReference: System.Int32 value1]
(1) LOAD ARGUMENT (Index 1)  [Parameter #1]  [ParameterReference: System.Int32 value2]
(2) ADD
(3) LOAD ARGUMENT (Index 2)  [Parameter #2]  [ParameterReference: System.Int32 value3]
(4) ADD
(5) LOAD ARGUMENT (Index 3)  [Parameter #3]  [ParameterReference: System.Int32 value4]
(6) ADD
(7) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #4]  [ParameterReference: System.Int32 value5]
(8) ADD
(9) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #5]  [ParameterReference: System.Int32 value6]
(10) ADD
(11) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #6]  [ParameterReference: System.Int32 value7]
(12) ADD
(13) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #7]  [ParameterReference: System.Int32 value8]
(14) ADD
(15) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #8]  [ParameterReference: System.Int32 value9]
(16) ADD
(17) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #9]  [ParameterReference: System.Int32 value10]
(18) ADD
(19) RETURN");
#else
		description.Should().Be(
@"(0) NO-OP
(1) LOAD ARGUMENT (Index 0)  [Parameter #0]  [ParameterReference: System.Int32 value1]
(2) LOAD ARGUMENT (Index 1)  [Parameter #1]  [ParameterReference: System.Int32 value2]
(3) ADD
(4) LOAD ARGUMENT (Index 2)  [Parameter #2]  [ParameterReference: System.Int32 value3]
(5) ADD
(6) LOAD ARGUMENT (Index 3)  [Parameter #3]  [ParameterReference: System.Int32 value4]
(7) ADD
(8) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #4]  [ParameterReference: System.Int32 value5]
(9) ADD
(10) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #5]  [ParameterReference: System.Int32 value6]
(11) ADD
(12) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #6]  [ParameterReference: System.Int32 value7]
(13) ADD
(14) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #7]  [ParameterReference: System.Int32 value8]
(15) ADD
(16) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #8]  [ParameterReference: System.Int32 value9]
(17) ADD
(18) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #9]  [ParameterReference: System.Int32 value10]
(19) ADD
(20) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(21) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(22) SET LOCAL VARIABLE (Index 1)  [Of type Int32]
(23) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 24]
(24) LOAD LOCAL VARIABLE (Index 1)  [Of type Int32]
(25) RETURN");
#endif
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result1()
	{
		var methodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
		methodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(methodInfo!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(6);
#else
		instructions.Count.Should().Be(12);
#endif

		//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
		//results.Should().NotBeNullOrEmpty();
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result2()
	{
		var nullCheckMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.NullParameterCheck_Type1), BindingFlags.Public | BindingFlags.Static);
		nullCheckMethodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(nullCheckMethodInfo!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(7);
#else
		instructions.Count.Should().Be(15);
#endif

		//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
		//results.Should().NotBeNullOrEmpty();
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result3()
	{
		var nullCheckMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.NullParameterCheck_Type2), BindingFlags.Public | BindingFlags.Static);
		nullCheckMethodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(nullCheckMethodInfo!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(7);
#else
		instructions.Count.Should().Be(11);
#endif

		//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
		//results.Should().NotBeNullOrEmpty();
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result4()
	{
		var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.MethodWithEachTypeOfInstruction), BindingFlags.Public | BindingFlags.Instance);
		testMethodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
		var instructionsDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);

		var instructionTypes = new HashSet<Type>();
		foreach (var instruction in instructions)
		{
			instructionTypes.Add(instruction.GetType());
		}

		// Release/optimized build will have different IL causing our test expectations will fail
		var debuggableAttribute = typeof(ExampleMethods).Assembly.GetCustomAttribute<System.Diagnostics.DebuggableAttribute>();
		var assemblyIsDebugBuildWithoutOptimizations = debuggableAttribute?.IsJITOptimizerDisabled == true;

#if NET6_0 && !IS_RELEASE_TESTING_BUILD
		instructionTypes.Should().Contain(typeof(BranchTargetInstruction));
		//instructionTypes.Should().Contain(typeof(ByteInstruction));
		instructionTypes.Should().Contain(typeof(DoubleInstruction));
		instructionTypes.Should().Contain(typeof(FieldReferenceInstruction));
		instructionTypes.Should().Contain(typeof(FloatInstruction));
		instructionTypes.Should().Contain(typeof(Int32Instruction));
		instructionTypes.Should().Contain(typeof(Int64Instruction));
		instructionTypes.Should().Contain(typeof(LocalVariableInstruction));
		instructionTypes.Should().Contain(typeof(MethodReferenceInstruction));
		instructionTypes.Should().Contain(typeof(ParameterReferenceInstruction));
		//instructionTypes.Should().Contain(typeof(SignatureInstruction));
		instructionTypes.Should().Contain(typeof(SignedByteInstruction));
		instructionTypes.Should().Contain(typeof(SimpleInstruction));
		instructionTypes.Should().Contain(typeof(StringInstruction));
		//instructionTypes.Should().Contain(typeof(SwitchInstruction));
		instructionTypes.Should().Contain(typeof(ThisKeywordInstruction));
		instructionTypes.Should().Contain(typeof(TypeReferenceInstruction));

		// DEBUG (Non Optimizied) Build
		instructions.Count.Should().Be(341);
		instructionsDescription.Should().Be(
@"(0) NO-OP
(1) LOAD INT LITERAL (5)
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) LOAD INT VALUE (Int8)  [SByte Value: 12]
(4) SET LOCAL VARIABLE (Index 1)  [Of type Byte]
(5) LOAD INT VALUE (Int8)  [SByte Value: -12]
(6) SET LOCAL VARIABLE (Index 2)  [Of type SByte]
(7) LOAD INT VALUE (Int32)  [Int32 Value: -300]
(8) SET LOCAL VARIABLE (Index 3)  [Of type Int16]
(9) LOAD INT VALUE (Int32)  [Int32 Value: 65000]
(10) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt16]
(11) LOAD INT VALUE (Int32)  [Int32 Value: -70000]
(12) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(13) LOAD INT VALUE (Int32)  [Int32 Value: 70000]
(14) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt32]
(15) LOAD INT VALUE (Int64)  [Int64 Value: -3000000000]
(16) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int64]
(17) LOAD INT VALUE (Int64)  [Int64 Value: -8446744073709551616]
(18) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(19) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(20) LOAD INT VALUE (Int64)  [Int64 Value: 9223372036854775807]
(21) SUBTRACT
(22) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(23) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(25) CALL METHOD  [ExampleMethods.AddTwoValues]
(26) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(27) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(28) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(29) MULTIPLY
(30) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(31) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(32) LOAD LOCAL VARIABLE (Index 1)  [Of type Byte]
(33) ADD
(34) LOAD LOCAL VARIABLE (Index 2)  [Of type SByte]
(35) ADD
(36) LOAD LOCAL VARIABLE (Index 3)  [Of type Int16]
(37) ADD
(38) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt16]
(39) ADD
(40) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(41) ADD
(42) CONVERT (Int64)
(43) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt32]
(44) CONVERT (UInt64)
(45) ADD
(46) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int64]
(47) ADD
(48) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(49) ADD
(50) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Int64]
(51) LOAD FLOAT VALUE (Float64)  [Double Value: 0.5]
(52) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type Double]
(53) LOAD FLOAT VALUE (Float32)  [Float Value: 0.5]
(54) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type Single]
(55) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Int64]
(56) CONVERT (Float64)
(57) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type Double]
(58) ADD
(59) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type Single]
(60) CONVERT (Float64)
(61) ADD
(62) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type Double]
(63) LOAD STRING  [String Value: SomeString]
(64) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type String]
(65) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(66) LOAD INT VALUE (Int8)  [SByte Value: 11]
(67) LOAD INT LITERAL (6)
(68) CALL METHOD  [DefaultInterpolatedStringHandler..ctor]
(69) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(70) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.String prefix]
(71) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(72) NO-OP
(73) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(74) LOAD STRING  [String Value: :  ]
(75) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(76) NO-OP
(77) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(78) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type String]
(79) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(80) NO-OP
(81) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(82) LOAD STRING  [String Value:  ]
(83) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(84) NO-OP
(85) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(86) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type Double]
(87) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(88) NO-OP
(89) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(90) LOAD STRING  [String Value:   - ]
(91) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(92) NO-OP
(93) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(94) LOAD TOKEN  [TypeReference: ExampleMethods]
(95) CALL METHOD  [Type.GetTypeFromHandle]
(96) CALL VIRTUAL  [Type.get_FullName]
(97) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(98) NO-OP
(99) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(100) LOAD STRING  [String Value: .]
(101) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(102) NO-OP
(103) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(104) LOAD STRING  [String Value: LocalStringField]
(105) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(106) NO-OP
(107) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(108) LOAD STRING  [String Value: : ]
(109) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(110) NO-OP
(111) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(112) LOAD ARGUMENT (Index 0)  [this keyword]
(113) LOAD FIELD  [FieldReference: LocalStringField]
(114) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(115) NO-OP
(116) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 17)  [Of type DefaultInterpolatedStringHandler]
(117) CALL METHOD  [DefaultInterpolatedStringHandler.ToStringAndClear]
(118) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(119) LOAD LOCAL VARIABLE (Index 1)  [Of type Byte]
(120) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type Byte]
(121) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type Byte]
(122) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(123) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(124) LOAD INT LITERAL (1)
(125) SUBTRACT
(126) LOAD INT LITERAL (3)
(127) BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form)  [TargetInstruction: 133]
(128) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 129]
(129) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(130) LOAD INT LITERAL (5)
(131) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 138]
(132) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 143]
(133) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(134) LOAD STRING  [String Value:   - switch statement case 1/2/3/4]
(135) CALL METHOD  [String.Concat]
(136) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(137) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 148]
(138) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(139) LOAD STRING  [String Value:   - switch statement case 5]
(140) CALL METHOD  [String.Concat]
(141) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(142) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 148]
(143) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(144) LOAD STRING  [String Value:   - switch statement default case]
(145) CALL METHOD  [String.Concat]
(146) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(147) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 148]
(148) LOAD STRING  [String Value: test]
(149) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(150) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(151) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type String]
(152) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type String]
(153) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(154) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(155) CALL METHOD  [<PrivateImplementationDetails>.ComputeStringHash]
(156) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(157) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(158) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(159) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 183]
(160) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(161) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(162) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 171]
(163) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(164) LOAD INT VALUE (Int32)  [Int32 Value: -1792857488]
(165) BRANCH WHEN EQUAL  [TargetInstruction: 230]
(166) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 167]
(167) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(168) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(169) BRANCH WHEN EQUAL  [TargetInstruction: 225]
(170) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(171) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(172) LOAD INT VALUE (Int32)  [Int32 Value: -1759302250]
(173) BRANCH WHEN EQUAL  [TargetInstruction: 240]
(174) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 175]
(175) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(176) LOAD INT VALUE (Int32)  [Int32 Value: -1742524631]
(177) BRANCH WHEN EQUAL  [TargetInstruction: 235]
(178) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 179]
(179) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(180) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(181) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 210]
(182) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(183) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(184) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(185) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 198]
(186) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(187) LOAD INT VALUE (Int32)  [Int32 Value: -1692191774]
(188) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 220]
(189) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 190]
(190) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(191) LOAD INT VALUE (Int32)  [Int32 Value: -1675414155]
(192) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 215]
(193) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 194]
(194) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(195) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(196) BRANCH WHEN EQUAL  [TargetInstruction: 255]
(197) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(198) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(199) LOAD INT VALUE (Int32)  [Int32 Value: -1620742665]
(200) BRANCH WHEN EQUAL  [TargetInstruction: 260]
(201) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 202]
(202) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(203) LOAD INT VALUE (Int32)  [Int32 Value: -1591526060]
(204) BRANCH WHEN EQUAL  [TargetInstruction: 250]
(205) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 206]
(206) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type UInt32]
(207) LOAD INT VALUE (Int32)  [Int32 Value: -1574748441]
(208) BRANCH WHEN EQUAL  [TargetInstruction: 245]
(209) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(210) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(211) LOAD STRING  [String Value: test1]
(212) CALL METHOD  [String.op_Equality]
(213) BRANCH WHEN TRUE  [TargetInstruction: 265]
(214) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(215) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(216) LOAD STRING  [String Value: test2]
(217) CALL METHOD  [String.op_Equality]
(218) BRANCH WHEN TRUE  [TargetInstruction: 271]
(219) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(220) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(221) LOAD STRING  [String Value: test3]
(222) CALL METHOD  [String.op_Equality]
(223) BRANCH WHEN TRUE  [TargetInstruction: 277]
(224) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(225) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(226) LOAD STRING  [String Value: test4]
(227) CALL METHOD  [String.op_Equality]
(228) BRANCH WHEN TRUE  [TargetInstruction: 283]
(229) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(230) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(231) LOAD STRING  [String Value: test5]
(232) CALL METHOD  [String.op_Equality]
(233) BRANCH WHEN TRUE  [TargetInstruction: 289]
(234) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(235) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(236) LOAD STRING  [String Value: test6]
(237) CALL METHOD  [String.op_Equality]
(238) BRANCH WHEN TRUE  [TargetInstruction: 295]
(239) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(240) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(241) LOAD STRING  [String Value: test7]
(242) CALL METHOD  [String.op_Equality]
(243) BRANCH WHEN TRUE  [TargetInstruction: 301]
(244) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(245) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(246) LOAD STRING  [String Value: test8]
(247) CALL METHOD  [String.op_Equality]
(248) BRANCH WHEN TRUE  [TargetInstruction: 307]
(249) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(250) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(251) LOAD STRING  [String Value: test9]
(252) CALL METHOD  [String.op_Equality]
(253) BRANCH WHEN TRUE  [TargetInstruction: 313]
(254) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(255) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(256) LOAD STRING  [String Value: test10]
(257) CALL METHOD  [String.op_Equality]
(258) BRANCH WHEN TRUE  [TargetInstruction: 319]
(259) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(260) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(261) LOAD STRING  [String Value: test11]
(262) CALL METHOD  [String.op_Equality]
(263) BRANCH WHEN TRUE  [TargetInstruction: 325]
(264) BRANCH UNCONDITIONALLY  [TargetInstruction: 331]
(265) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(266) LOAD STRING  [String Value:   - string switch statement case ]
(267) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(268) CALL METHOD  [String.Concat]
(269) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(270) BRANCH UNCONDITIONALLY  [TargetInstruction: 336]
(271) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(272) LOAD STRING  [String Value:   - string switch statement case ]
(273) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(274) CALL METHOD  [String.Concat]
(275) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(276) BRANCH UNCONDITIONALLY  [TargetInstruction: 336]
(277) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(278) LOAD STRING  [String Value:   - string switch statement case ]
(279) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(280) CALL METHOD  [String.Concat]
(281) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(282) BRANCH UNCONDITIONALLY  [TargetInstruction: 336]
(283) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(284) LOAD STRING  [String Value:   - string switch statement case ]
(285) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(286) CALL METHOD  [String.Concat]
(287) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(288) BRANCH UNCONDITIONALLY  [TargetInstruction: 336]
(289) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(290) LOAD STRING  [String Value:   - string switch statement case ]
(291) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(292) CALL METHOD  [String.Concat]
(293) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(294) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(295) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(296) LOAD STRING  [String Value:   - string switch statement case ]
(297) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(298) CALL METHOD  [String.Concat]
(299) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(300) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(301) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(302) LOAD STRING  [String Value:   - string switch statement case ]
(303) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(304) CALL METHOD  [String.Concat]
(305) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(306) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(307) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(308) LOAD STRING  [String Value:   - string switch statement case ]
(309) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(310) CALL METHOD  [String.Concat]
(311) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(312) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(313) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(314) LOAD STRING  [String Value:   - string switch statement case ]
(315) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(316) CALL METHOD  [String.Concat]
(317) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(318) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(319) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(320) LOAD STRING  [String Value:   - string switch statement case ]
(321) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(322) CALL METHOD  [String.Concat]
(323) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(324) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(325) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(326) LOAD STRING  [String Value:   - string switch statement case ]
(327) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(328) CALL METHOD  [String.Concat]
(329) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(330) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(331) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(332) LOAD STRING  [String Value:   - string switch statement default case]
(333) CALL METHOD  [String.Concat]
(334) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(335) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 336]
(336) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(337) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 23)  [Of type String]
(338) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 339]
(339) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 23)  [Of type String]
(340) RETURN");
#elif !IS_RELEASE_TESTING_BUILD
		instructionTypes.Should().Contain(typeof(BranchTargetInstruction));
		//instructionTypes.Should().Contain(typeof(ByteInstruction));
		instructionTypes.Should().Contain(typeof(DoubleInstruction));
		instructionTypes.Should().Contain(typeof(FieldReferenceInstruction));
		instructionTypes.Should().Contain(typeof(FloatInstruction));
		instructionTypes.Should().Contain(typeof(Int32Instruction));
		instructionTypes.Should().Contain(typeof(Int64Instruction));
		instructionTypes.Should().Contain(typeof(LocalVariableInstruction));
		instructionTypes.Should().Contain(typeof(MethodReferenceInstruction));
		instructionTypes.Should().Contain(typeof(ParameterReferenceInstruction));
		//instructionTypes.Should().Contain(typeof(SignatureInstruction));
		instructionTypes.Should().Contain(typeof(SignedByteInstruction));
		instructionTypes.Should().Contain(typeof(SimpleInstruction));
		instructionTypes.Should().Contain(typeof(StringInstruction));
		//instructionTypes.Should().Contain(typeof(SwitchInstruction));
		instructionTypes.Should().Contain(typeof(ThisKeywordInstruction));
		instructionTypes.Should().Contain(typeof(TypeReferenceInstruction));

		// DEBUG (Non Optimizied) Build
		instructions.Count.Should().Be(320);
		instructionsDescription.Should().Be(
@"(0) NO-OP
(1) LOAD INT LITERAL (5)
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) LOAD INT VALUE (Int8)  [SByte Value: 12]
(4) SET LOCAL VARIABLE (Index 1)  [Of type Byte]
(5) LOAD INT VALUE (Int8)  [SByte Value: -12]
(6) SET LOCAL VARIABLE (Index 2)  [Of type SByte]
(7) LOAD INT VALUE (Int32)  [Int32 Value: -300]
(8) SET LOCAL VARIABLE (Index 3)  [Of type Int16]
(9) LOAD INT VALUE (Int32)  [Int32 Value: 65000]
(10) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt16]
(11) LOAD INT VALUE (Int32)  [Int32 Value: -70000]
(12) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(13) LOAD INT VALUE (Int32)  [Int32 Value: 70000]
(14) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt32]
(15) LOAD INT VALUE (Int64)  [Int64 Value: -3000000000]
(16) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int64]
(17) LOAD INT VALUE (Int64)  [Int64 Value: -8446744073709551616]
(18) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(19) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(20) LOAD INT VALUE (Int64)  [Int64 Value: 9223372036854775807]
(21) SUBTRACT
(22) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(23) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(25) CALL METHOD  [ExampleMethods.AddTwoValues]
(26) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(27) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(28) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(29) MULTIPLY
(30) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(31) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(32) LOAD LOCAL VARIABLE (Index 1)  [Of type Byte]
(33) ADD
(34) LOAD LOCAL VARIABLE (Index 2)  [Of type SByte]
(35) ADD
(36) LOAD LOCAL VARIABLE (Index 3)  [Of type Int16]
(37) ADD
(38) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt16]
(39) ADD
(40) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(41) ADD
(42) CONVERT (Int64)
(43) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt32]
(44) CONVERT (UInt64)
(45) ADD
(46) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int64]
(47) ADD
(48) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(49) ADD
(50) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Int64]
(51) LOAD FLOAT VALUE (Float64)  [Double Value: 0.5]
(52) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type Double]
(53) LOAD FLOAT VALUE (Float32)  [Float Value: 0.5]
(54) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type Single]
(55) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Int64]
(56) CONVERT (Float64)
(57) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type Double]
(58) ADD
(59) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type Single]
(60) CONVERT (Float64)
(61) ADD
(62) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type Double]
(63) LOAD STRING  [String Value: SomeString]
(64) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type String]
(65) LOAD STRING  [String Value: {0}:  {1} {2}  - {3}.{4}: {5}]
(66) LOAD INT LITERAL (6)
(67) NEW ARRAY  [TypeReference: Object]
(68) DUPLICATE
(69) LOAD INT LITERAL (0)
(70) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.String prefix]
(71) STORE ARRAY ELEMENT (Object Reference)
(72) DUPLICATE
(73) LOAD INT LITERAL (1)
(74) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type String]
(75) STORE ARRAY ELEMENT (Object Reference)
(76) DUPLICATE
(77) LOAD INT LITERAL (2)
(78) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type Double]
(79) BOX VALUE  [TypeReference: Double]
(80) STORE ARRAY ELEMENT (Object Reference)
(81) DUPLICATE
(82) LOAD INT LITERAL (3)
(83) LOAD TOKEN  [TypeReference: ExampleMethods]
(84) CALL METHOD  [Type.GetTypeFromHandle]
(85) CALL VIRTUAL  [Type.get_FullName]
(86) STORE ARRAY ELEMENT (Object Reference)
(87) DUPLICATE
(88) LOAD INT LITERAL (4)
(89) LOAD STRING  [String Value: LocalStringField]
(90) STORE ARRAY ELEMENT (Object Reference)
(91) DUPLICATE
(92) LOAD INT LITERAL (5)
(93) LOAD ARGUMENT (Index 0)  [this keyword]
(94) LOAD FIELD  [FieldReference: LocalStringField]
(95) STORE ARRAY ELEMENT (Object Reference)
(96) CALL METHOD  [String.Format]
(97) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(98) LOAD LOCAL VARIABLE (Index 1)  [Of type Byte]
(99) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(100) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(101) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 17)  [Of type Byte]
(102) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 17)  [Of type Byte]
(103) LOAD INT LITERAL (1)
(104) SUBTRACT
(105) LOAD INT LITERAL (3)
(106) BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form)  [TargetInstruction: 112]
(107) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 108]
(108) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 17)  [Of type Byte]
(109) LOAD INT LITERAL (5)
(110) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 117]
(111) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 122]
(112) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(113) LOAD STRING  [String Value:   - switch statement case 1/2/3/4]
(114) CALL METHOD  [String.Concat]
(115) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(116) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 127]
(117) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(118) LOAD STRING  [String Value:   - switch statement case 5]
(119) CALL METHOD  [String.Concat]
(120) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(121) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 127]
(122) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(123) LOAD STRING  [String Value:   - switch statement default case]
(124) CALL METHOD  [String.Concat]
(125) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(126) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 127]
(127) LOAD STRING  [String Value: test]
(128) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(129) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(130) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(131) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(132) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(133) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(134) CALL METHOD  [<PrivateImplementationDetails>.ComputeStringHash]
(135) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(136) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(137) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(138) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 162]
(139) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(140) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(141) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 150]
(142) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(143) LOAD INT VALUE (Int32)  [Int32 Value: -1792857488]
(144) BRANCH WHEN EQUAL  [TargetInstruction: 209]
(145) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 146]
(146) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(147) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(148) BRANCH WHEN EQUAL  [TargetInstruction: 204]
(149) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(150) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(151) LOAD INT VALUE (Int32)  [Int32 Value: -1759302250]
(152) BRANCH WHEN EQUAL  [TargetInstruction: 219]
(153) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 154]
(154) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(155) LOAD INT VALUE (Int32)  [Int32 Value: -1742524631]
(156) BRANCH WHEN EQUAL  [TargetInstruction: 214]
(157) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 158]
(158) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(159) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(160) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 189]
(161) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(162) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(163) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(164) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 177]
(165) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(166) LOAD INT VALUE (Int32)  [Int32 Value: -1692191774]
(167) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 199]
(168) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 169]
(169) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(170) LOAD INT VALUE (Int32)  [Int32 Value: -1675414155]
(171) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 194]
(172) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 173]
(173) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(174) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(175) BRANCH WHEN EQUAL  [TargetInstruction: 234]
(176) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(177) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(178) LOAD INT VALUE (Int32)  [Int32 Value: -1620742665]
(179) BRANCH WHEN EQUAL  [TargetInstruction: 239]
(180) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 181]
(181) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(182) LOAD INT VALUE (Int32)  [Int32 Value: -1591526060]
(183) BRANCH WHEN EQUAL  [TargetInstruction: 229]
(184) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 185]
(185) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(186) LOAD INT VALUE (Int32)  [Int32 Value: -1574748441]
(187) BRANCH WHEN EQUAL  [TargetInstruction: 224]
(188) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(189) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(190) LOAD STRING  [String Value: test1]
(191) CALL METHOD  [String.op_Equality]
(192) BRANCH WHEN TRUE  [TargetInstruction: 244]
(193) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(194) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(195) LOAD STRING  [String Value: test2]
(196) CALL METHOD  [String.op_Equality]
(197) BRANCH WHEN TRUE  [TargetInstruction: 250]
(198) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(199) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(200) LOAD STRING  [String Value: test3]
(201) CALL METHOD  [String.op_Equality]
(202) BRANCH WHEN TRUE  [TargetInstruction: 256]
(203) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(204) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(205) LOAD STRING  [String Value: test4]
(206) CALL METHOD  [String.op_Equality]
(207) BRANCH WHEN TRUE  [TargetInstruction: 262]
(208) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(209) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(210) LOAD STRING  [String Value: test5]
(211) CALL METHOD  [String.op_Equality]
(212) BRANCH WHEN TRUE  [TargetInstruction: 268]
(213) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(214) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(215) LOAD STRING  [String Value: test6]
(216) CALL METHOD  [String.op_Equality]
(217) BRANCH WHEN TRUE  [TargetInstruction: 274]
(218) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(219) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(220) LOAD STRING  [String Value: test7]
(221) CALL METHOD  [String.op_Equality]
(222) BRANCH WHEN TRUE  [TargetInstruction: 280]
(223) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(224) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(225) LOAD STRING  [String Value: test8]
(226) CALL METHOD  [String.op_Equality]
(227) BRANCH WHEN TRUE  [TargetInstruction: 286]
(228) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(229) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(230) LOAD STRING  [String Value: test9]
(231) CALL METHOD  [String.op_Equality]
(232) BRANCH WHEN TRUE  [TargetInstruction: 292]
(233) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(234) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(235) LOAD STRING  [String Value: test10]
(236) CALL METHOD  [String.op_Equality]
(237) BRANCH WHEN TRUE  [TargetInstruction: 298]
(238) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(239) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(240) LOAD STRING  [String Value: test11]
(241) CALL METHOD  [String.op_Equality]
(242) BRANCH WHEN TRUE  [TargetInstruction: 304]
(243) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(244) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(245) LOAD STRING  [String Value:   - string switch statement case ]
(246) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(247) CALL METHOD  [String.Concat]
(248) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(249) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(250) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(251) LOAD STRING  [String Value:   - string switch statement case ]
(252) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(253) CALL METHOD  [String.Concat]
(254) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(255) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(256) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(257) LOAD STRING  [String Value:   - string switch statement case ]
(258) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(259) CALL METHOD  [String.Concat]
(260) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(261) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(262) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(263) LOAD STRING  [String Value:   - string switch statement case ]
(264) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(265) CALL METHOD  [String.Concat]
(266) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(267) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(268) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(269) LOAD STRING  [String Value:   - string switch statement case ]
(270) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(271) CALL METHOD  [String.Concat]
(272) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(273) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(274) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(275) LOAD STRING  [String Value:   - string switch statement case ]
(276) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(277) CALL METHOD  [String.Concat]
(278) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(279) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(280) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(281) LOAD STRING  [String Value:   - string switch statement case ]
(282) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(283) CALL METHOD  [String.Concat]
(284) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(285) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(286) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(287) LOAD STRING  [String Value:   - string switch statement case ]
(288) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(289) CALL METHOD  [String.Concat]
(290) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(291) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(292) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(293) LOAD STRING  [String Value:   - string switch statement case ]
(294) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(295) CALL METHOD  [String.Concat]
(296) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(297) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(298) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(299) LOAD STRING  [String Value:   - string switch statement case ]
(300) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(301) CALL METHOD  [String.Concat]
(302) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(303) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(304) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(305) LOAD STRING  [String Value:   - string switch statement case ]
(306) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(307) CALL METHOD  [String.Concat]
(308) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(309) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(310) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(311) LOAD STRING  [String Value:   - string switch statement default case]
(312) CALL METHOD  [String.Concat]
(313) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(314) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(315) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(316) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type String]
(317) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 318]
(318) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type String]
(319) RETURN");
#elif NET6_0
		instructionTypes.Should().Contain(typeof(BranchTargetInstruction));
		//instructionTypes.Should().Contain(typeof(ByteInstruction));
		instructionTypes.Should().Contain(typeof(DoubleInstruction));
		instructionTypes.Should().Contain(typeof(FieldReferenceInstruction));
		instructionTypes.Should().Contain(typeof(FloatInstruction));
		instructionTypes.Should().Contain(typeof(Int32Instruction));
		instructionTypes.Should().Contain(typeof(Int64Instruction));
		instructionTypes.Should().Contain(typeof(LocalVariableInstruction));
		instructionTypes.Should().Contain(typeof(MethodReferenceInstruction));
		instructionTypes.Should().Contain(typeof(ParameterReferenceInstruction));
		//instructionTypes.Should().Contain(typeof(SignatureInstruction));
		instructionTypes.Should().Contain(typeof(SimpleInstruction));
		instructionTypes.Should().Contain(typeof(StringInstruction));
		//instructionTypes.Should().Contain(typeof(SwitchInstruction));
		instructionTypes.Should().Contain(typeof(ThisKeywordInstruction));
		instructionTypes.Should().Contain(typeof(TypeReferenceInstruction));

		// RELEASE (Optimizied) Build
		instructionsDescription.Should().Be(
@"(0) LOAD INT LITERAL (5)
(1) LOAD INT VALUE (Int8)  [SByte Value: 12]
(2) SET LOCAL VARIABLE (Index 0)  [Of type Byte]
(3) LOAD INT VALUE (Int8)  [SByte Value: -12]
(4) SET LOCAL VARIABLE (Index 1)  [Of type SByte]
(5) LOAD INT VALUE (Int32)  [Int32 Value: -300]
(6) SET LOCAL VARIABLE (Index 2)  [Of type Int16]
(7) LOAD INT VALUE (Int32)  [Int32 Value: 65000]
(8) SET LOCAL VARIABLE (Index 3)  [Of type UInt16]
(9) LOAD INT VALUE (Int32)  [Int32 Value: -70000]
(10) LOAD INT VALUE (Int32)  [Int32 Value: 70000]
(11) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt32]
(12) LOAD INT VALUE (Int64)  [Int64 Value: -3000000000]
(13) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int64]
(14) LOAD INT VALUE (Int64)  [Int64 Value: -8446744073709551616]
(15) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(16) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(17) LOAD INT VALUE (Int64)  [Int64 Value: 9223372036854775807]
(18) SUBTRACT
(19) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(20) DUPLICATE
(21) CALL METHOD  [ExampleMethods.AddTwoValues]
(22) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(23) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(25) MULTIPLY
(26) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(27) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(28) ADD
(29) LOAD LOCAL VARIABLE (Index 1)  [Of type SByte]
(30) ADD
(31) LOAD LOCAL VARIABLE (Index 2)  [Of type Int16]
(32) ADD
(33) LOAD LOCAL VARIABLE (Index 3)  [Of type UInt16]
(34) ADD
(35) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(36) ADD
(37) CONVERT (Int64)
(38) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt32]
(39) CONVERT (UInt64)
(40) ADD
(41) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int64]
(42) ADD
(43) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(44) ADD
(45) LOAD FLOAT VALUE (Float64)  [Double Value: 0.5]
(46) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type Double]
(47) LOAD FLOAT VALUE (Float32)  [Float Value: 0.5]
(48) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Single]
(49) CONVERT (Float64)
(50) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type Double]
(51) ADD
(52) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Single]
(53) CONVERT (Float64)
(54) ADD
(55) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Double]
(56) LOAD STRING  [String Value: SomeString]
(57) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type String]
(58) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(59) LOAD INT VALUE (Int8)  [SByte Value: 11]
(60) LOAD INT LITERAL (6)
(61) CALL METHOD  [DefaultInterpolatedStringHandler..ctor]
(62) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(63) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.String prefix]
(64) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(65) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(66) LOAD STRING  [String Value: :  ]
(67) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(68) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(69) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type String]
(70) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(71) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(72) LOAD STRING  [String Value:  ]
(73) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(74) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(75) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Double]
(76) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(77) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(78) LOAD STRING  [String Value:   - ]
(79) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(80) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(81) LOAD TOKEN  [TypeReference: ExampleMethods]
(82) CALL METHOD  [Type.GetTypeFromHandle]
(83) CALL VIRTUAL  [Type.get_FullName]
(84) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(85) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(86) LOAD STRING  [String Value: .]
(87) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(88) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(89) LOAD STRING  [String Value: LocalStringField]
(90) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(91) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(92) LOAD STRING  [String Value: : ]
(93) CALL METHOD  [DefaultInterpolatedStringHandler.AppendLiteral]
(94) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(95) LOAD ARGUMENT (Index 0)  [this keyword]
(96) LOAD FIELD  [FieldReference: LocalStringField]
(97) CALL METHOD  [DefaultInterpolatedStringHandler.AppendFormatted]
(98) LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index)  (Index 14)  [Of type DefaultInterpolatedStringHandler]
(99) CALL METHOD  [DefaultInterpolatedStringHandler.ToStringAndClear]
(100) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(101) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(102) LOAD INT LITERAL (1)
(103) SUBTRACT
(104) LOAD INT LITERAL (3)
(105) BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form)  [TargetInstruction: 110]
(106) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(107) LOAD INT LITERAL (5)
(108) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 115]
(109) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 120]
(110) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(111) LOAD STRING  [String Value:   - switch statement case 1/2/3/4]
(112) CALL METHOD  [String.Concat]
(113) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(114) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 124]
(115) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(116) LOAD STRING  [String Value:   - switch statement case 5]
(117) CALL METHOD  [String.Concat]
(118) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(119) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 124]
(120) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(121) LOAD STRING  [String Value:   - switch statement default case]
(122) CALL METHOD  [String.Concat]
(123) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(124) LOAD STRING  [String Value: test]
(125) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(126) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(127) BRANCH WHEN FALSE  [TargetInstruction: 279]
(128) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(129) CALL METHOD  [String.get_Length]
(130) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Int32]
(131) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Int32]
(132) LOAD INT LITERAL (5)
(133) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 138]
(134) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Int32]
(135) LOAD INT LITERAL (6)
(136) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 147]
(137) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(138) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(139) LOAD INT LITERAL (4)
(140) CALL METHOD  [String.get_Chars]
(141) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type Char]
(142) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type Char]
(143) LOAD INT VALUE (Int8)  [SByte Value: 49]
(144) SUBTRACT
(145) SWITCH  [TargetInstructions: 158, 163, 168, 173, 178, 183, 188, 193, 198]  [TargetOffsets: 481, 503, 525, 547, 569, 591, 613, 635, 657]
(146) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(147) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(148) LOAD INT LITERAL (5)
(149) CALL METHOD  [String.get_Chars]
(150) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type Char]
(151) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type Char]
(152) LOAD INT VALUE (Int8)  [SByte Value: 48]
(153) BRANCH WHEN EQUAL  [TargetInstruction: 203]
(154) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type Char]
(155) LOAD INT VALUE (Int8)  [SByte Value: 49]
(156) BRANCH WHEN EQUAL  [TargetInstruction: 208]
(157) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(158) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(159) LOAD STRING  [String Value: test1]
(160) CALL METHOD  [String.op_Equality]
(161) BRANCH WHEN TRUE  [TargetInstruction: 213]
(162) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(163) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(164) LOAD STRING  [String Value: test2]
(165) CALL METHOD  [String.op_Equality]
(166) BRANCH WHEN TRUE  [TargetInstruction: 219]
(167) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(168) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(169) LOAD STRING  [String Value: test3]
(170) CALL METHOD  [String.op_Equality]
(171) BRANCH WHEN TRUE  [TargetInstruction: 225]
(172) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(173) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(174) LOAD STRING  [String Value: test4]
(175) CALL METHOD  [String.op_Equality]
(176) BRANCH WHEN TRUE  [TargetInstruction: 231]
(177) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(178) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(179) LOAD STRING  [String Value: test5]
(180) CALL METHOD  [String.op_Equality]
(181) BRANCH WHEN TRUE  [TargetInstruction: 237]
(182) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(183) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(184) LOAD STRING  [String Value: test6]
(185) CALL METHOD  [String.op_Equality]
(186) BRANCH WHEN TRUE  [TargetInstruction: 243]
(187) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(188) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(189) LOAD STRING  [String Value: test7]
(190) CALL METHOD  [String.op_Equality]
(191) BRANCH WHEN TRUE  [TargetInstruction: 249]
(192) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(193) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(194) LOAD STRING  [String Value: test8]
(195) CALL METHOD  [String.op_Equality]
(196) BRANCH WHEN TRUE  [TargetInstruction: 255]
(197) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(198) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(199) LOAD STRING  [String Value: test9]
(200) CALL METHOD  [String.op_Equality]
(201) BRANCH WHEN TRUE  [TargetInstruction: 261]
(202) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(203) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(204) LOAD STRING  [String Value: test10]
(205) CALL METHOD  [String.op_Equality]
(206) BRANCH WHEN TRUE  [TargetInstruction: 267]
(207) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(208) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(209) LOAD STRING  [String Value: test11]
(210) CALL METHOD  [String.op_Equality]
(211) BRANCH WHEN TRUE  [TargetInstruction: 273]
(212) BRANCH UNCONDITIONALLY  [TargetInstruction: 279]
(213) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(214) LOAD STRING  [String Value:   - string switch statement case ]
(215) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(216) CALL METHOD  [String.Concat]
(217) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(218) BRANCH UNCONDITIONALLY  [TargetInstruction: 283]
(219) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(220) LOAD STRING  [String Value:   - string switch statement case ]
(221) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(222) CALL METHOD  [String.Concat]
(223) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(224) BRANCH UNCONDITIONALLY  [TargetInstruction: 283]
(225) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(226) LOAD STRING  [String Value:   - string switch statement case ]
(227) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(228) CALL METHOD  [String.Concat]
(229) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(230) BRANCH UNCONDITIONALLY  [TargetInstruction: 283]
(231) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(232) LOAD STRING  [String Value:   - string switch statement case ]
(233) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(234) CALL METHOD  [String.Concat]
(235) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(236) BRANCH UNCONDITIONALLY  [TargetInstruction: 283]
(237) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(238) LOAD STRING  [String Value:   - string switch statement case ]
(239) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(240) CALL METHOD  [String.Concat]
(241) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(242) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(243) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(244) LOAD STRING  [String Value:   - string switch statement case ]
(245) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(246) CALL METHOD  [String.Concat]
(247) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(248) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(249) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(250) LOAD STRING  [String Value:   - string switch statement case ]
(251) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(252) CALL METHOD  [String.Concat]
(253) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(254) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(255) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(256) LOAD STRING  [String Value:   - string switch statement case ]
(257) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(258) CALL METHOD  [String.Concat]
(259) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(260) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(261) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(262) LOAD STRING  [String Value:   - string switch statement case ]
(263) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(264) CALL METHOD  [String.Concat]
(265) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(266) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(267) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(268) LOAD STRING  [String Value:   - string switch statement case ]
(269) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(270) CALL METHOD  [String.Concat]
(271) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(272) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(273) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(274) LOAD STRING  [String Value:   - string switch statement case ]
(275) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(276) CALL METHOD  [String.Concat]
(277) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(278) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 283]
(279) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(280) LOAD STRING  [String Value:   - string switch statement default case]
(281) CALL METHOD  [String.Concat]
(282) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(283) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(284) RETURN");
#else
		instructionTypes.Should().Contain(typeof(BranchTargetInstruction));
		//instructionTypes.Should().Contain(typeof(ByteInstruction));
		instructionTypes.Should().Contain(typeof(DoubleInstruction));
		instructionTypes.Should().Contain(typeof(FieldReferenceInstruction));
		instructionTypes.Should().Contain(typeof(FloatInstruction));
		instructionTypes.Should().Contain(typeof(Int32Instruction));
		instructionTypes.Should().Contain(typeof(Int64Instruction));
		instructionTypes.Should().Contain(typeof(LocalVariableInstruction));
		instructionTypes.Should().Contain(typeof(MethodReferenceInstruction));
		instructionTypes.Should().Contain(typeof(ParameterReferenceInstruction));
		//instructionTypes.Should().Contain(typeof(SignatureInstruction));
		instructionTypes.Should().Contain(typeof(SimpleInstruction));
		instructionTypes.Should().Contain(typeof(StringInstruction));
		//instructionTypes.Should().Contain(typeof(SwitchInstruction));
		instructionTypes.Should().Contain(typeof(ThisKeywordInstruction));
		instructionTypes.Should().Contain(typeof(TypeReferenceInstruction));

		// RELEASE (Optimizied) Build
		instructionsDescription.Should().Be(
@"(0) LOAD INT LITERAL (5)
(1) LOAD INT VALUE (Int8)  [SByte Value: 12]
(2) SET LOCAL VARIABLE (Index 0)  [Of type Byte]
(3) LOAD INT VALUE (Int8)  [SByte Value: -12]
(4) SET LOCAL VARIABLE (Index 1)  [Of type SByte]
(5) LOAD INT VALUE (Int32)  [Int32 Value: -300]
(6) SET LOCAL VARIABLE (Index 2)  [Of type Int16]
(7) LOAD INT VALUE (Int32)  [Int32 Value: 65000]
(8) SET LOCAL VARIABLE (Index 3)  [Of type UInt16]
(9) LOAD INT VALUE (Int32)  [Int32 Value: -70000]
(10) LOAD INT VALUE (Int32)  [Int32 Value: 70000]
(11) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt32]
(12) LOAD INT VALUE (Int64)  [Int64 Value: -3000000000]
(13) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int64]
(14) LOAD INT VALUE (Int64)  [Int64 Value: -8446744073709551616]
(15) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(16) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(17) LOAD INT VALUE (Int64)  [Int64 Value: 9223372036854775807]
(18) SUBTRACT
(19) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(20) DUPLICATE
(21) CALL METHOD  [ExampleMethods.AddTwoValues]
(22) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(23) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(25) MULTIPLY
(26) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(27) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(28) ADD
(29) LOAD LOCAL VARIABLE (Index 1)  [Of type SByte]
(30) ADD
(31) LOAD LOCAL VARIABLE (Index 2)  [Of type Int16]
(32) ADD
(33) LOAD LOCAL VARIABLE (Index 3)  [Of type UInt16]
(34) ADD
(35) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(36) ADD
(37) CONVERT (Int64)
(38) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt32]
(39) CONVERT (UInt64)
(40) ADD
(41) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int64]
(42) ADD
(43) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(44) ADD
(45) LOAD FLOAT VALUE (Float64)  [Double Value: 0.5]
(46) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type Double]
(47) LOAD FLOAT VALUE (Float32)  [Float Value: 0.5]
(48) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Single]
(49) CONVERT (Float64)
(50) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type Double]
(51) ADD
(52) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Single]
(53) CONVERT (Float64)
(54) ADD
(55) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Double]
(56) LOAD STRING  [String Value: SomeString]
(57) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type String]
(58) LOAD STRING  [String Value: {0}:  {1} {2}  - {3}.{4}: {5}]
(59) LOAD INT LITERAL (6)
(60) NEW ARRAY  [TypeReference: Object]
(61) DUPLICATE
(62) LOAD INT LITERAL (0)
(63) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.String prefix]
(64) STORE ARRAY ELEMENT (Object Reference)
(65) DUPLICATE
(66) LOAD INT LITERAL (1)
(67) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type String]
(68) STORE ARRAY ELEMENT (Object Reference)
(69) DUPLICATE
(70) LOAD INT LITERAL (2)
(71) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Double]
(72) BOX VALUE  [TypeReference: Double]
(73) STORE ARRAY ELEMENT (Object Reference)
(74) DUPLICATE
(75) LOAD INT LITERAL (3)
(76) LOAD TOKEN  [TypeReference: ExampleMethods]
(77) CALL METHOD  [Type.GetTypeFromHandle]
(78) CALL VIRTUAL  [Type.get_FullName]
(79) STORE ARRAY ELEMENT (Object Reference)
(80) DUPLICATE
(81) LOAD INT LITERAL (4)
(82) LOAD STRING  [String Value: LocalStringField]
(83) STORE ARRAY ELEMENT (Object Reference)
(84) DUPLICATE
(85) LOAD INT LITERAL (5)
(86) LOAD ARGUMENT (Index 0)  [this keyword]
(87) LOAD FIELD  [FieldReference: LocalStringField]
(88) STORE ARRAY ELEMENT (Object Reference)
(89) CALL METHOD  [String.Format]
(90) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(91) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(92) LOAD INT LITERAL (1)
(93) SUBTRACT
(94) LOAD INT LITERAL (3)
(95) BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form)  [TargetInstruction: 100]
(96) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(97) LOAD INT LITERAL (5)
(98) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 105]
(99) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 110]
(100) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(101) LOAD STRING  [String Value:   - switch statement case 1/2/3/4]
(102) CALL METHOD  [String.Concat]
(103) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(104) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 114]
(105) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(106) LOAD STRING  [String Value:   - switch statement case 5]
(107) CALL METHOD  [String.Concat]
(108) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(109) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 114]
(110) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(111) LOAD STRING  [String Value:   - switch statement default case]
(112) CALL METHOD  [String.Concat]
(113) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(114) LOAD STRING  [String Value: test]
(115) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(116) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(117) BRANCH WHEN FALSE  [TargetInstruction: 269]
(118) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(119) CALL METHOD  [String.get_Length]
(120) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type Int32]
(121) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type Int32]
(122) LOAD INT LITERAL (5)
(123) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 128]
(124) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type Int32]
(125) LOAD INT LITERAL (6)
(126) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 137]
(127) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(128) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(129) LOAD INT LITERAL (4)
(130) CALL METHOD  [String.get_Chars]
(131) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Char]
(132) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Char]
(133) LOAD INT VALUE (Int8)  [SByte Value: 49]
(134) SUBTRACT
(135) SWITCH  [TargetInstructions: 148, 153, 158, 163, 168, 173, 178, 183, 188]  [TargetOffsets: 401, 423, 445, 467, 489, 511, 533, 555, 577]
(136) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(137) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(138) LOAD INT LITERAL (5)
(139) CALL METHOD  [String.get_Chars]
(140) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Char]
(141) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Char]
(142) LOAD INT VALUE (Int8)  [SByte Value: 48]
(143) BRANCH WHEN EQUAL  [TargetInstruction: 193]
(144) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type Char]
(145) LOAD INT VALUE (Int8)  [SByte Value: 49]
(146) BRANCH WHEN EQUAL  [TargetInstruction: 198]
(147) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(148) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(149) LOAD STRING  [String Value: test1]
(150) CALL METHOD  [String.op_Equality]
(151) BRANCH WHEN TRUE  [TargetInstruction: 203]
(152) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(153) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(154) LOAD STRING  [String Value: test2]
(155) CALL METHOD  [String.op_Equality]
(156) BRANCH WHEN TRUE  [TargetInstruction: 209]
(157) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(158) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(159) LOAD STRING  [String Value: test3]
(160) CALL METHOD  [String.op_Equality]
(161) BRANCH WHEN TRUE  [TargetInstruction: 215]
(162) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(163) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(164) LOAD STRING  [String Value: test4]
(165) CALL METHOD  [String.op_Equality]
(166) BRANCH WHEN TRUE  [TargetInstruction: 221]
(167) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(168) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(169) LOAD STRING  [String Value: test5]
(170) CALL METHOD  [String.op_Equality]
(171) BRANCH WHEN TRUE  [TargetInstruction: 227]
(172) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(173) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(174) LOAD STRING  [String Value: test6]
(175) CALL METHOD  [String.op_Equality]
(176) BRANCH WHEN TRUE  [TargetInstruction: 233]
(177) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(178) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(179) LOAD STRING  [String Value: test7]
(180) CALL METHOD  [String.op_Equality]
(181) BRANCH WHEN TRUE  [TargetInstruction: 239]
(182) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(183) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(184) LOAD STRING  [String Value: test8]
(185) CALL METHOD  [String.op_Equality]
(186) BRANCH WHEN TRUE  [TargetInstruction: 245]
(187) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(188) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(189) LOAD STRING  [String Value: test9]
(190) CALL METHOD  [String.op_Equality]
(191) BRANCH WHEN TRUE  [TargetInstruction: 251]
(192) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(193) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(194) LOAD STRING  [String Value: test10]
(195) CALL METHOD  [String.op_Equality]
(196) BRANCH WHEN TRUE  [TargetInstruction: 257]
(197) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(198) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(199) LOAD STRING  [String Value: test11]
(200) CALL METHOD  [String.op_Equality]
(201) BRANCH WHEN TRUE  [TargetInstruction: 263]
(202) BRANCH UNCONDITIONALLY  [TargetInstruction: 269]
(203) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(204) LOAD STRING  [String Value:   - string switch statement case ]
(205) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(206) CALL METHOD  [String.Concat]
(207) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(208) BRANCH UNCONDITIONALLY  [TargetInstruction: 273]
(209) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(210) LOAD STRING  [String Value:   - string switch statement case ]
(211) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(212) CALL METHOD  [String.Concat]
(213) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(214) BRANCH UNCONDITIONALLY  [TargetInstruction: 273]
(215) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(216) LOAD STRING  [String Value:   - string switch statement case ]
(217) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(218) CALL METHOD  [String.Concat]
(219) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(220) BRANCH UNCONDITIONALLY  [TargetInstruction: 273]
(221) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(222) LOAD STRING  [String Value:   - string switch statement case ]
(223) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(224) CALL METHOD  [String.Concat]
(225) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(226) BRANCH UNCONDITIONALLY  [TargetInstruction: 273]
(227) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(228) LOAD STRING  [String Value:   - string switch statement case ]
(229) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(230) CALL METHOD  [String.Concat]
(231) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(232) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(233) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(234) LOAD STRING  [String Value:   - string switch statement case ]
(235) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(236) CALL METHOD  [String.Concat]
(237) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(238) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(239) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(240) LOAD STRING  [String Value:   - string switch statement case ]
(241) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(242) CALL METHOD  [String.Concat]
(243) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(244) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(245) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(246) LOAD STRING  [String Value:   - string switch statement case ]
(247) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(248) CALL METHOD  [String.Concat]
(249) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(250) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(251) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(252) LOAD STRING  [String Value:   - string switch statement case ]
(253) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(254) CALL METHOD  [String.Concat]
(255) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(256) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(257) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(258) LOAD STRING  [String Value:   - string switch statement case ]
(259) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(260) CALL METHOD  [String.Concat]
(261) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(262) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(263) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(264) LOAD STRING  [String Value:   - string switch statement case ]
(265) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(266) CALL METHOD  [String.Concat]
(267) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(268) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 273]
(269) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(270) LOAD STRING  [String Value:   - string switch statement default case]
(271) CALL METHOD  [String.Concat]
(272) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(273) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(274) RETURN");
#endif
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result5()
	{
		var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.MethodWithGotoLabels), BindingFlags.Public | BindingFlags.Instance);
		testMethodInfo.Should().NotBeNull();

		var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(26);
#else
		instructions.Count.Should().Be(46);
#endif

		var instructionDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);

#if IS_RELEASE_TESTING_BUILD
		instructionDescription.Should().Be(
@"(0) LOAD INT LITERAL (0)
(1) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(2) LOAD STRING  [String Value: First Label]
(3) CALL METHOD  [Console.WriteLine]
(4) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(5) LOAD INT LITERAL (1)
(6) ADD
(7) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(8) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(9) LOAD INT LITERAL (3)
(10) BRANCH WHEN LESS THAN (Short Form)  [TargetInstruction: 2]
(11) LOAD STRING  [String Value: Second Label]
(12) CALL METHOD  [Console.WriteLine]
(13) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(14) LOAD INT LITERAL (1)
(15) ADD
(16) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(17) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(18) LOAD INT LITERAL (6)
(19) BRANCH WHEN LESS THAN (Short Form)  [TargetInstruction: 2]
(20) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(21) LOAD INT VALUE (Int8)  [SByte Value: 9]
(22) BRANCH WHEN LESS THAN (Short Form)  [TargetInstruction: 11]
(23) LOAD STRING  [String Value: Last Label]
(24) CALL METHOD  [Console.WriteLine]
(25) RETURN");
#else

		instructionDescription.Should().Be(
@"(0) NO-OP
(1) LOAD INT LITERAL (0)
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) NO-OP
(4) LOAD STRING  [String Value: First Label]
(5) CALL METHOD  [Console.WriteLine]
(6) NO-OP
(7) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(8) LOAD INT LITERAL (1)
(9) ADD
(10) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(11) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(12) LOAD INT LITERAL (3)
(13) COMPARE (LessThan)
(14) SET LOCAL VARIABLE (Index 1)  [Of type Boolean]
(15) LOAD LOCAL VARIABLE (Index 1)  [Of type Boolean]
(16) BRANCH WHEN FALSE (Short Form)  [TargetInstruction: 18]
(17) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 3]
(18) NO-OP
(19) LOAD STRING  [String Value: Second Label]
(20) CALL METHOD  [Console.WriteLine]
(21) NO-OP
(22) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(23) LOAD INT LITERAL (1)
(24) ADD
(25) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(26) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(27) LOAD INT LITERAL (6)
(28) COMPARE (LessThan)
(29) SET LOCAL VARIABLE (Index 2)  [Of type Boolean]
(30) LOAD LOCAL VARIABLE (Index 2)  [Of type Boolean]
(31) BRANCH WHEN FALSE (Short Form)  [TargetInstruction: 33]
(32) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 3]
(33) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(34) LOAD INT VALUE (Int8)  [SByte Value: 9]
(35) COMPARE (LessThan)
(36) SET LOCAL VARIABLE (Index 3)  [Of type Boolean]
(37) LOAD LOCAL VARIABLE (Index 3)  [Of type Boolean]
(38) BRANCH WHEN FALSE (Short Form)  [TargetInstruction: 40]
(39) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 18]
(40) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 41]
(41) NO-OP
(42) LOAD STRING  [String Value: Last Label]
(43) CALL METHOD  [Console.WriteLine]
(44) NO-OP
(45) RETURN");
#endif
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result6()
	{
		_dynamicJumpTableMethod.Should().NotBeNull();
		var instructions = new MethodBodyParser(_dynamicJumpTableMethod).ParseInstructions();
		instructions.Count.Should().Be(15);

		var instructionsDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);
		instructionsDescription.Should().Be(
@"(0) LOAD ARGUMENT (Index 0)  [Parameter #0]  [ParameterReference: System.Int32 ]
(1) SWITCH  [TargetInstructions: 3, 5, 7, 9, 11]  [TargetOffsets: 28, 35, 42, 49, 56]
(2) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 13]
(3) LOAD STRING  [String Value: are no bananas]
(4) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(5) LOAD STRING  [String Value: is one banana]
(6) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(7) LOAD STRING  [String Value: are two bananas]
(8) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(9) LOAD STRING  [String Value: are three bananas]
(10) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(11) LOAD STRING  [String Value: are four bananas]
(12) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(13) LOAD STRING  [String Value: are many bananas]
(14) RETURN");
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result7()
	{
		var methodToParse = typeof(NativeInteropExampleMethods).GetMethod(nameof(NativeInteropExampleMethods.MethodThatUsesNativeInteropCall), BindingFlags.Public | BindingFlags.Static);
		methodToParse.Should().NotBeNull();

		var instructions = new MethodBodyParser(methodToParse!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(19);
#else
		instructions.Count.Should().Be(23);
#endif
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result8()
	{
		var methodToParse = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14_Using_Delegate_Function), BindingFlags.Public | BindingFlags.Static);
		methodToParse.Should().NotBeNull();

		var instructions = new MethodBodyParser(methodToParse!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		var instructionsDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);
		Console.WriteLine(instructionsDescription);

		instructions.Count.Should().Be(13);
#else
		instructions.Count.Should().Be(19);
		/*
		Count = 19

		{(0) NO-OP}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.FieldReferenceInstruction}
		{(2) DUPLICATE}
		{(3) BRANCH WHEN TRUE (Short Form)  [TargetInstruction: 10]}
		{(4) POP STACK}
		{(5) LOAD NULL REFERENCE}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.MethodReferenceInstruction}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.MethodReferenceInstruction}
		{(8) DUPLICATE}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.FieldReferenceInstruction}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.LocalVariableInstruction}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.LocalVariableInstruction}
		{(12) LOAD INT LITERAL (7)}
		{(13) LOAD INT VALUE (Int8)  [SByte Value: 14]}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.MethodReferenceInstruction}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.LocalVariableInstruction}
		{(16) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 17]}
		{Rhinobyte.Extensions.Reflection.IntermediateLanguage.LocalVariableInstruction}
		{(18) RETURN}

		*/
#endif
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result9()
	{
		var dynamicType = DynamicTypeBuilder.BuildTypeWithInlineSignatureMethod();
		var dynamicMethod = dynamicType!.GetMethod("InlineSignatureMethod", BindingFlags.Public | BindingFlags.Static);

		var instructions = new MethodBodyParser(dynamicMethod!).ParseInstructions();
		instructions.Count.Should().Be(2);
		instructions.Any(instruction => instruction is SignatureInstruction).Should().BeTrue();
	}

	[TestMethod]
	public void ParseInstructions_returns_the_expected_result_with_for_loop_and_continue_statement()
	{
		var methodToTest = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.MethodWithForLoopAndContinueStatement), BindingFlags.Public | BindingFlags.Static);

		var instructions = new MethodBodyParser(methodToTest!).ParseInstructions();
#if IS_RELEASE_TESTING_BUILD
		instructions.Count.Should().Be(95);
#else
		instructions.Count.Should().Be(119);
#endif

		//var description = new DefaultInstructionFormatter().DescribeInstructions(instructions);
	}



	/******     TEST SETUP     *****************************
	 *******************************************************/
	private static Type _dynamicTypeWithJumpTableMethod = null!; // Nullability hacks, InitializeTestClass will always set these
	private static MethodInfo _dynamicJumpTableMethod = null!;

	[ClassInitialize]
	public static void InitializeTestClass(TestContext testContext)
	{
		_dynamicTypeWithJumpTableMethod = DynamicTypeBuilder.BuildTypeWithJumpTableMethod()!;
		_dynamicJumpTableMethod = _dynamicTypeWithJumpTableMethod!.GetMethod("JumpTableMethod", BindingFlags.Public | BindingFlags.Static)!;
	}
}
