using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineFloatInstruction : InstructionBase
	{
		public float Value { get; }

		public InlineFloatInstruction(int offset, OpCode opcode, float value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
