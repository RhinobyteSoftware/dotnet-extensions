using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineDoubleInstruction : InstructionBase
	{
		public double Value { get; }

		public InlineDoubleInstruction(int offset, OpCode opcode, double value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
