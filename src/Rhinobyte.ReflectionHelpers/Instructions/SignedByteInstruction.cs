using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class SignedByteInstruction : InstructionBase
	{
		public sbyte Value { get; }

		public SignedByteInstruction(int offset, OpCode opcode, sbyte value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
