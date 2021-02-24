using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class Int32Instruction : InstructionBase
	{
		public Int32Instruction(int offset, OpCode opcode, int value)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="int"/> value of the instruction.
		/// </summary>
		public int Value { get; }

		public override string ToString()
			=> $"{base.ToString()}  [Int32 Value: {Value}]";
	}
}
