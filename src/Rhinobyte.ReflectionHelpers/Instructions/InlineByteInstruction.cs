using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineByteInstruction : InstructionBase
	{
		public byte Value { get; }

		public InlineByteInstruction(int offset, OpCode opcode, byte value)
			: base(offset, opcode)
		{
			Value = value;
		}

	}
}
