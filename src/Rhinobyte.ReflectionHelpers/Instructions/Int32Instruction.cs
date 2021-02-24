using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class Int32Instruction : InstructionBase
	{
		public Int32Instruction(int offset, OpCode opcode, int value)
			: base(offset, opcode, opcode.Size + 4)
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="int"/> value of the instruction.
		/// </summary>
		public int Value { get; }
	}
}
