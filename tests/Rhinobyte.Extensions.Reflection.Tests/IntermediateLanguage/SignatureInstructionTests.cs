using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage;

[TestClass]
public class SignatureInstructionTests
{
	[TestMethod]
	public void GetSignatureBlob_does_not_throw_for_a_null_byte_array()
	{
		var signatureInstruction = new SignatureInstruction(0, 0, OpCodes.Calli, null!);
		signatureInstruction.GetSignatureBlob().Should().BeEmpty();
		signatureInstruction.ToString().Should().NotBeNullOrEmpty();
	}

	[TestMethod]
	public void GetSignatureBlob_returns_a_cloned_byte_array_to_prevent_mutation()
	{
		var orignalSignatureBlob = new byte[12];
		var signatureInstruction = new SignatureInstruction(0, 0, OpCodes.Calli, orignalSignatureBlob);
		var getSignatureBlobResult = signatureInstruction.GetSignatureBlob();
		getSignatureBlobResult.Should().NotBeSameAs(orignalSignatureBlob);
	}
}
