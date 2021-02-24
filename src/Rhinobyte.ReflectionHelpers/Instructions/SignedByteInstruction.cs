using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class SignedByteInstruction : InstructionBase
	{
		public SignedByteInstruction(int offset, OpCode opcode, sbyte value)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="sbyte"/> value of the instruction.
		/// </summary>
		public sbyte Value { get; }
	}
}
