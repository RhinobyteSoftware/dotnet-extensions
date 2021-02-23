using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class BasicInstruction : InstructionBase
	{
		public BasicInstruction(int offset, OpCode opcode)
			: base(offset, opcode)
		{

		}

		// TODO: Override ToString() with a clean + descriptive summary of the instruction type
	}
}
