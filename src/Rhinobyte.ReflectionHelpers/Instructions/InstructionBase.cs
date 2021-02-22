using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public abstract class InstructionBase
	{
		public OpCode OpCode { get; }

		protected InstructionBase(OpCode opcode)
		{
			OpCode = opcode;
		}
	}
}
