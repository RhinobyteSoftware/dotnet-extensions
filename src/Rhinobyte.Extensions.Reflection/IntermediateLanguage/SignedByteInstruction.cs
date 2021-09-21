using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	public sealed class SignedByteInstruction : InstructionBase
	{
		internal SignedByteInstruction(int index, int offset, OpCode opcode, sbyte value)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="sbyte"/> value of the instruction.
		/// </summary>
		public sbyte Value { get; }

		public override string ToString()
			=> $"{base.ToString()}  [SByte Value: {Value}]";
	}
}
