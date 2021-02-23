using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class FloatInstruction : InstructionBase
	{
		public float Value { get; }

		public FloatInstruction(int offset, OpCode opcode, float value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
