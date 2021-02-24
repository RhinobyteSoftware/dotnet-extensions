using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class BranchTargetInstruction : InstructionBase
	{
		internal BranchTargetInstruction(bool isShortFormOperand, int offset, OpCode opcode, int targetOffset)
			: base(offset, opcode, opcode.Size + (isShortFormOperand ? 1 : 4))
		{
			IsShortFormOperand = isShortFormOperand;
			TargetInstruction = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction
			TargetOffset = targetOffset;
		}

		public BranchTargetInstruction(bool isShortFormOperand, int offset, OpCode opcode, InstructionBase targetInstruction, int targetOffset)
			: base(offset, opcode, opcode.Size + (isShortFormOperand ? 1 : 4))
		{
			IsShortFormOperand = isShortFormOperand;
			TargetInstruction = targetInstruction;
			TargetOffset = targetOffset;
		}

		/// <summary>
		/// Whether or not the <see cref="OpCode.OperandType"/> is the <see cref="OperandType.ShortInlineBrTarget"/> for an 8bit operand or the <see cref="OperandType.InlineBrTarget"/> for a full 32bit operand.
		/// </summary>
		public bool IsShortFormOperand { get; }

		/// <summary>
		/// The target instruction of this branch instruction.
		/// </summary>
		public InstructionBase TargetInstruction { get; internal set; }

		/// <summary>
		/// The offset of the target instruction.
		/// </summary>
		public int TargetOffset { get; }
	}
}
