using System.Globalization;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// Instruction class for instructions that transfer control to a target instruction at a specified relative target offset.
/// </summary>
public sealed class BranchTargetInstruction : InstructionBase
{
	internal BranchTargetInstruction(int index, int offset, OpCode opcode, int targetOffset)
		: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
	{
		TargetInstruction = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction
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

	/// <inheritdoc/>
	public override string ToString()
		=> $"{base.ToString()}  [TargetInstruction: {TargetInstruction?.Index.ToString(CultureInfo.CurrentCulture) ?? "null"}]";
}
