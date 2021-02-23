using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class DoubleInstruction : InstructionBase
	{
		public double Value { get; }

		public DoubleInstruction(int offset, OpCode opcode, double value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
