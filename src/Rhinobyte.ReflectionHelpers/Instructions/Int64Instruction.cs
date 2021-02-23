using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class Int64Instruction : InstructionBase
	{
		public long Value { get; }

		public Int64Instruction(int offset, OpCode opcode, long value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
