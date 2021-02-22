using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineBranchTargetInstruction : InstructionBase
	{
		public InstructionBase TargetInstruction { get; internal set; }
		public int TargetOffset { get; }

		internal InlineBranchTargetInstruction(OpCode opcode, int targetOffset)
			: base(opcode)
		{
			TargetInstruction = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction
			TargetOffset = targetOffset;
		}

		public InlineBranchTargetInstruction(OpCode opcode, InstructionBase targetInstruction, int targetOffset)
			: base(opcode)
		{
			TargetInstruction = targetInstruction;
			TargetOffset = targetOffset;
		}
	}
}
