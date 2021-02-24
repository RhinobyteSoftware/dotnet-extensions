using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class FloatInstruction : InstructionBase
	{
		public FloatInstruction(int offset, OpCode opcode, float value)
			: base(offset, opcode, opcode.Size + 4)
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="float"/> value of the instruction.
		/// </summary>
		public float Value { get; }
	}
}
