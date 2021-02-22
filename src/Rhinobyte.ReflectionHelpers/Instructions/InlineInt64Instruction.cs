using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineInt64Instruction : InstructionBase
	{
		public long Value { get; }

		public InlineInt64Instruction(int offset, OpCode opcode, long value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
