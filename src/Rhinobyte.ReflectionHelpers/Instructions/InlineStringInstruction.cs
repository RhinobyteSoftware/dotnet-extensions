using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineStringInstruction : InstructionBase
	{
		public string Value { get; }

		public InlineStringInstruction(int offset, OpCode opcode, string value)
			: base(offset, opcode)
		{
			Value = value;
		}
	}
}
