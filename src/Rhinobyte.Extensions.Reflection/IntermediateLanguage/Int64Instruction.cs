using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// Instruction class with an associated <see cref="long"/> value operand.
/// </summary>
public sealed class Int64Instruction : InstructionBase
{
	internal Int64Instruction(int index, int offset, OpCode opcode, long value)
		: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
	{
		Value = value;
	}

	/// <summary>
	/// The <see cref="long"/> value of the instruction.
	/// </summary>
	public long Value { get; }

	/// <inheritdoc/>
	public override string ToString()
		=> $"{base.ToString()}  [Int64 Value: {Value}]";
}
