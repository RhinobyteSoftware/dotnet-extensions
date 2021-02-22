using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public abstract class InstructionBase
	{
		/// <summary>
		/// The relative offset of this instruction within the instruction set bytes.
		/// </summary>
		public int Offset { get; }

		/// <summary>
		/// The <see cref="OpCode"/> for this instruction.
		/// </summary>
		public OpCode OpCode { get; }

		protected InstructionBase(int offset, OpCode opcode)
		{
			Offset = offset;
			OpCode = opcode;
		}
	}
}
