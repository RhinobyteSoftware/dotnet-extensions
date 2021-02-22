using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineSignatureInstruction : InstructionBase
	{
		public byte[] SignatureBlob { get; }

		public InlineSignatureInstruction(int offset, OpCode opcode, byte[] signatureBlob)
			: base(offset, opcode)
		{
			SignatureBlob = signatureBlob;
		}
	}
}
