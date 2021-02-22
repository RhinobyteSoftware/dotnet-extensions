using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineInt32Instruction : InstructionBase
	{
		public int Value { get; }

		public InlineInt32Instruction(int offset, OpCode opcode, int value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
