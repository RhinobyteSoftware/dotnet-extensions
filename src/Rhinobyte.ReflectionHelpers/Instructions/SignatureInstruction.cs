using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class SignatureInstruction : InstructionBase
	{
		private readonly byte[] _signatureBlob;

		public SignatureInstruction(int offset, OpCode opcode, byte[] signatureBlob)
			: base(offset, opcode)
		{
			_signatureBlob = signatureBlob;
		}

		/// <summary>
		/// Returns a copy of the singature blob byte[]
		/// </summary>
		public byte[] GetSignatureBlob()
		{
			// Need to return a clone of the array so that consumers of this library cannot change its contents
			return (byte[])_signatureBlob.Clone();
		}
	}
}
