using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ByteInstruction : InstructionBase
	{
		public byte Value { get; }

		public ByteInstruction(int offset, OpCode opcode, byte value)
			: base(offset, opcode)
		{
			Value = value;
		}

	}
}
