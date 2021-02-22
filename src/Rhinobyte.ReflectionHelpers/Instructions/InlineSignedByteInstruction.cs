using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineSignedByteInstruction : InstructionBase
	{
		public sbyte Value { get; }

		public InlineSignedByteInstruction(int offset, OpCode opcode, sbyte value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
