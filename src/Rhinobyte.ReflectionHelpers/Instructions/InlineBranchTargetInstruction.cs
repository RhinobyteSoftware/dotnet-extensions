using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class InlineBranchTargetInstruction : InstructionBase
	{
		/// <summary>
		/// Whether or not the <see cref="OpCode.OperandType"/> is the <see cref="OperandType.ShortInlineBrTarget"/> for an 8bit operand or the <see cref="OperandType.InlineBrTarget"/> for a full 32bit operand.
		/// </summary>
		public bool IsShortInstruction { get; }
		public InstructionBase TargetInstruction { get; internal set; }
		public int TargetOffset { get; }

		internal InlineBranchTargetInstruction(bool isShortInstruction, int offset, OpCode opcode, int targetOffset)
			: base(offset, opcode)
		{
			IsShortInstruction = isShortInstruction;
			TargetInstruction = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction
			TargetOffset = targetOffset;
		}

		public InlineBranchTargetInstruction(bool isShortInstruction, int offset, OpCode opcode, InstructionBase targetInstruction, int targetOffset)
			: base(offset, opcode)
		{
			IsShortInstruction = isShortInstruction;
			TargetInstruction = targetInstruction;
			TargetOffset = targetOffset;
		}
	}
}
