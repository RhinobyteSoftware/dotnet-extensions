using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ByteInstruction : InstructionBase
	{
		public ByteInstruction(int offset, OpCode opcode, byte value)
			: base(offset, opcode, opcode.Size + 1)
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="byte"/> value of the instruction.
		/// </summary>
		public byte Value { get; }
	}
}
