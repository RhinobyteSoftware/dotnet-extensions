using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage;

[TestClass]
public class ParameterReferenceInstructionTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void ToString_gracefully_handles_null()
	{
		var parameterReferenceInstruction = new ParameterReferenceInstruction(0, 0, OpCodes.Nop, 0, null!);
		parameterReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();

		var parameterReference = new ParameterInfoWithNulls(null!);
		parameterReferenceInstruction = new ParameterReferenceInstruction(0, 0, OpCodes.Nop, 0, parameterReference);
		parameterReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();

		var methodInfo = typeof(ExampleLibrary1.ClassWithGenericMethod).GetMethod(nameof(ExampleLibrary1.ClassWithGenericMethod.MethodWithGenericResult4), BindingFlags.Public | BindingFlags.Instance);
		var typeWithNullFullName = methodInfo!.ReturnType;
		typeWithNullFullName.Name.Should().NotBeNullOrWhiteSpace();
		typeWithNullFullName.FullName.Should().BeNull();
		parameterReference = new ParameterInfoWithNulls(typeWithNullFullName);
		parameterReferenceInstruction = new ParameterReferenceInstruction(0, 0, OpCodes.Nop, 0, parameterReference);
		parameterReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();
	}

	/******     TEST SETUP     *****************************
	 *******************************************************/
	public class ParameterInfoWithNulls : System.Reflection.ParameterInfo
	{
		public ParameterInfoWithNulls(Type parameterType)
		{
			ParameterType = parameterType;
		}

		public override Type ParameterType { get; }
	}
}
