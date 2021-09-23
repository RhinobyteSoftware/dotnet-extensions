using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage
{
	[TestClass]
	public class MethodReferenceInstructionTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void ToString_handles_nulls_gracefully()
		{
			var methodReferenceInstruction = new MethodReferenceInstruction(0, 0, OpCodes.Call, null);
			methodReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();

			var mockMethodBase = new MockMethodBase(null, "MockMethod");
			methodReferenceInstruction = new MethodReferenceInstruction(0, 0, OpCodes.Call, mockMethodBase);
			methodReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();
		}
	}
}
