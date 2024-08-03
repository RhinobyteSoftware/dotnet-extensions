using System;
using System.Globalization;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

public class MockMethodBase : MethodBase
{
	public MethodAttributes _attributes;
	private MethodBody? _methodBody;
	private Module? _module;
	private Type? _reflectedType;

	public MockMethodBase(
		Type? declaringType,
		string name)
	{
		DeclaringType = declaringType!;
		Name = name;
	}

	public MockMethodBase(
		string name)
		: this(typeof(MockMethodBase), name)
	{

	}



	public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

	public override MethodAttributes Attributes => _attributes;

	public override MemberTypes MemberType => MemberTypes.Method;

	public override Module Module => _module!;

	public override string Name { get; }

	public override Type DeclaringType { get; }

	public override Type ReflectedType => _reflectedType!;


	public override object[] GetCustomAttributes(bool inherit) => [];
	public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];

	public override Type[] GetGenericArguments() => [];

	public override MethodBody? GetMethodBody() => _methodBody;

	public override MethodImplAttributes GetMethodImplementationFlags() => MethodImplAttributes.Runtime;
	public override ParameterInfo[] GetParameters() => [];
	public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture) => throw new NotImplementedException();
	public override bool IsDefined(Type attributeType, bool inherit) => false;

	public void SetAttributes(MethodAttributes attributes) => _attributes = attributes;
	public void SetMethodBody(MethodBody? methodBody) => _methodBody = methodBody;
	public void SetModule(Module? module) => _module = module;
	public void SetReflectedType(Type? reflectedType) => _reflectedType = reflectedType;
}

public class MockMethodBody : MethodBody
{

}
