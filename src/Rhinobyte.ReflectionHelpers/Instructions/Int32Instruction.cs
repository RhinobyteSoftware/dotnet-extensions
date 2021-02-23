using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class Int32Instruction : InstructionBase
	{
		public int Value { get; }

		public Int32Instruction(int offset, OpCode opcode, int value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
