using System;
using System.Globalization;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public class MockMethodBase : MethodBase
	{
		private MethodBody? _methodBody;
		private Module? _module;

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

		public override MethodAttributes Attributes => MethodAttributes.Public | MethodAttributes.Static;

		public override MemberTypes MemberType => MemberTypes.Method;

		public override Module Module => _module!;

		public override string Name { get; }

		public override Type DeclaringType { get; }

		public override Type ReflectedType => throw new NotImplementedException();


		public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();
		public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();

		public override Type[] GetGenericArguments() => Array.Empty<Type>();

		public override MethodBody? GetMethodBody() => _methodBody;

		public override MethodImplAttributes GetMethodImplementationFlags() => MethodImplAttributes.Runtime;
		public override ParameterInfo[] GetParameters() => Array.Empty<ParameterInfo>();
		public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture) => throw new NotImplementedException();
		public override bool IsDefined(Type attributeType, bool inherit) => false;

		public void SetMethodBody(MethodBody? methodBody) => _methodBody = methodBody;
		public void SetModule(Module? module) => _module = module;
	}

	public class MockMethodBody : MethodBody
	{

	}
}
