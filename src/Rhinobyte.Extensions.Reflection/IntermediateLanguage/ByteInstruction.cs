using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="byte"/> value operand.
	/// </summary>
	public sealed class ByteInstruction : InstructionBase
	{
		internal ByteInstruction(int index, int offset, OpCode opcode, byte value)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="byte"/> value of the instruction.
		/// </summary>
		public byte Value { get; }

		/// <inheritdoc/>
		public override string ToString()
			=> $"{base.ToString()}  [Byte Value: {Value}]";
	}
}
