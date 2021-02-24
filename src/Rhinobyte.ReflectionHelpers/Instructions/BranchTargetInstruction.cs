using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class BranchTargetInstruction : InstructionBase
	{
		internal BranchTargetInstruction(int offset, OpCode opcode, int targetOffset)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			TargetInstruction = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction
			TargetOffset = targetOffset;
		}

		public BranchTargetInstruction(int offset, OpCode opcode, InstructionBase targetInstruction, int targetOffset)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			TargetInstruction = targetInstruction;
			TargetOffset = targetOffset;
		}

		/// <summary>
		/// The target instruction of this branch instruction.
		/// </summary>
		public InstructionBase TargetInstruction { get; internal set; }

		/// <summary>
		/// The offset of the target instruction.
		/// </summary>
		public int TargetOffset { get; }

		public override string ToString()
			=> $"{base.ToString()}  [TargetOffset: {TargetOffset}]";
	}
}
