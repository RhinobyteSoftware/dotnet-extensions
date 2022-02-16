using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// Instruction class with an associated <see cref="float"/> value operand.
/// </summary>
public sealed class FloatInstruction : InstructionBase
{
	internal FloatInstruction(int index, int offset, OpCode opcode, float value)
		: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
	{
		Value = value;
	}

	/// <summary>
	/// The <see cref="float"/> value of the instruction.
	/// </summary>
	public float Value { get; }

	/// <inheritdoc/>
	public override string ToString()
		=> $"{base.ToString()}  [Float Value: {Value}]";
}
