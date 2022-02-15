using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage;

[TestClass]
public class LocalVariableInstructionTests
{
	[TestMethod]
	public void ToString_gracefully_handles_nulls()
	{
		var systemUnderTest = new LocalVariableInstruction(0, 0, OpCodes.Nop, new MockLocalVariableInfo(false, 1, null));
		systemUnderTest.ToString().Should().NotBeNullOrWhiteSpace();

		systemUnderTest = new LocalVariableInstruction(0, 0, OpCodes.Nop, new MockLocalVariableInfo(false, 1, typeof(string)));
		systemUnderTest.ToString().Should().NotBeNullOrWhiteSpace();

		systemUnderTest = new LocalVariableInstruction(0, 0, OpCodes.Nop, new MockLocalVariableInfo(false, 1, new MockTypeInfo(null, null, null)));
		systemUnderTest.ToString().Should().NotBeNullOrWhiteSpace();
	}
}
