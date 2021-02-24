using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class SignatureInstruction : InstructionBase
	{
		private readonly byte[] _signatureBlob;

		public SignatureInstruction(int offset, OpCode opcode, byte[] signatureBlob)
			: base(offset, opcode, opcode.Size + 4)
		{
			_signatureBlob = signatureBlob;
		}

		/// <summary>
		/// Returns a copy of the instruction's singature blob <see cref="byte"/>[].
		/// </summary>
		public byte[] GetSignatureBlob()
		{
			// Need to return a clone of the array so that consumers of this library cannot change its contents
			return (byte[])_signatureBlob.Clone();
		}
	}
}
