using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class StringInstruction : InstructionBase
	{
		public string Value { get; }

		public StringInstruction(int offset, OpCode opcode, string value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
