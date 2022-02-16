using System;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

/******     BASE TYPE     ******************************
********************************************************/
public class ExampleInheritedMemberBaseClass
{
	protected string? inheritedStringField;
	public int baseIntFieldThatWillBeHidden;
	public static readonly TimeSpan staticTimeSpanField = TimeSpan.FromMinutes(1);

	public int InheritedIntProperty { get; protected set; }

	public bool InheritedPublicPropertyWithNoSetter { get; }

	public bool PublicPropertyOnBaseThatWillBeHidden { get; set; }

	protected virtual short ProtectedVirtualShortProperty { get; set; }

	public virtual bool PublicVirtualBoolProperty { get; set; }

	public virtual bool PublicVirtualBoolReadOnlyProperty { get; }

	public bool _writeOnlyBackingField;
#pragma warning disable CA1044 // Properties should not be write only
	public virtual bool PublicVirtualBoolWriteOnlyProperty { set => _writeOnlyBackingField = value; }
#pragma warning restore CA1044 // Properties should not be write only

	protected internal void InheritedMethod()
	{
		Console.WriteLine("InheritedMethod() was called");
	}

	protected virtual void InheritedVirtualProtectedMethod()
	{
		Console.WriteLine("InheritedVirtualMethod() was called");
	}

	public void NonVirtualBaseMethod1() { }

	public void NonVirtualBaseMethod2(string someStringParameter) { }

	public virtual void PublicVirtualMethod() { }

#pragma warning disable CA1044 // Properties should not be write only
#pragma warning disable IDE0052 // Remove unread private members
	private string _backingField = string.Empty;
	public string WriteOnlyProperty { set => _backingField = value; }
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore CA1044 // Properties should not be write only

	public static bool StaticPropertyOnBaseClass { get; set; }

	public static bool StaticReadOnlyProperty => true;

#pragma warning disable IDE0052 // Remove unread private members
	private static bool _staticWriteOnlyBackingField;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning disable CA1044 // Properties should not be write only
	public static bool StaticWriteOnlyProperty { set => _staticWriteOnlyBackingField = value; }
#pragma warning restore CA1044 // Properties should not be write only
}


/******     CHILD TYPE     *****************************
********************************************************/
public class ExampleInheritedMemberSubClass : ExampleInheritedMemberBaseClass
{
	public new int baseIntFieldThatWillBeHidden;

	protected override short ProtectedVirtualShortProperty { get => 5; set => _ = value; }
	public new bool PublicPropertyOnBaseThatWillBeHidden { get; set; }
	public override bool PublicVirtualBoolProperty { get => true; set => _ = value; }
	public override bool PublicVirtualBoolReadOnlyProperty => false;
	public override bool PublicVirtualBoolWriteOnlyProperty { set => _ = value; }

	protected override void InheritedVirtualProtectedMethod()
	{
		base.InheritedVirtualProtectedMethod();
	}

	public void MethodThatCallsTheInheritedMethod()
	{
		InheritedMethod();
	}

	public string MethodThatReferencesTheInheritedField()
	{
		return inheritedStringField ?? string.Empty;
	}

	public override void PublicVirtualMethod() => base.PublicVirtualMethod();
}


/******     GRANDCHILD TYPE     ************************
********************************************************/
public class GrandChildMemberClass : ExampleInheritedMemberSubClass
{
	public new int baseIntFieldThatWillBeHidden;

	public override void PublicVirtualMethod()
	{
		return;
	}
}


/******     STATIC REFERENCES TYPE     *****************
********************************************************/
public static class ExampleInheritedMemberStaticClass
{
	public static bool MethodThatReferencesBasePropertyNotYetHidden()
	{
		var baseInstance = new ExampleInheritedMemberBaseClass();
		return baseInstance.PublicPropertyOnBaseThatWillBeHidden;
	}

	public static bool MethodThatReferencesSubclassPropertyThatHidesBaseClassProperty()
	{
		var subclassInstance = new ExampleInheritedMemberSubClass();
		return subclassInstance.PublicPropertyOnBaseThatWillBeHidden;
	}

	public static void MethodThatReferencesBaseVirtualProperties()
	{
		var baseInstance = new ExampleInheritedMemberBaseClass();
		var value1 = baseInstance.PublicVirtualBoolProperty || baseInstance.PublicVirtualBoolReadOnlyProperty;
		baseInstance.PublicVirtualBoolWriteOnlyProperty = value1;
	}

	public static void MethodThatReferencesOverrideVirtualProperties()
	{
		var inheritedInstance = new ExampleInheritedMemberSubClass();
		var value1 = inheritedInstance.PublicVirtualBoolProperty || inheritedInstance.PublicVirtualBoolReadOnlyProperty;
		inheritedInstance.PublicVirtualBoolWriteOnlyProperty = value1;
	}

	public static bool MethodThatReferencesTheInheritedPublicPropertyWithNoSetter()
	{
		var instance = new ExampleInheritedMemberSubClass();
		return instance.InheritedPublicPropertyWithNoSetter;
	}

	public static bool MethodThatReferencesTheBasePublicPropertyWithNoSetter()
	{
		var instance = new ExampleInheritedMemberBaseClass();
		return instance.InheritedPublicPropertyWithNoSetter;
	}

	public static void MethodThatReferencesTheInheritedPublicProperty()
	{
		var instance = new ExampleInheritedMemberSubClass();
		if (instance.InheritedIntProperty > 0)
			Console.WriteLine("InheritedIntProperty (inherited) is greater than zero");
	}

	public static void MethodThatReferencesTheBasePublicProperty()
	{
		var instance = new ExampleInheritedMemberBaseClass();
		if (instance.InheritedIntProperty > 0)
			Console.WriteLine("InheritedIntProperty (base) is greater than zero");
	}
}
