using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class with an associated <see cref="long"/> value operand.
	/// </summary>
	public sealed class Int64Instruction : InstructionBase
	{
		internal Int64Instruction(int offset, OpCode opcode, long value)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="long"/> value of the instruction.
		/// </summary>
		public long Value { get; }

		public override string ToString()
			=> $"{base.ToString()}  [Int64 Value: {Value}]";
	}
}
