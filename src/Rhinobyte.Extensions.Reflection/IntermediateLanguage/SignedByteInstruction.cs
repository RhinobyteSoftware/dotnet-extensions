using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// An instruction with a corresponding signed bye value
/// </summary>
/// <remarks>The <see cref="SignedByteInstruction.Value"/> has property type <see cref="short" /> to ensure common language specification compliance</remarks>
public sealed class SignedByteInstruction : InstructionBase
{
	internal SignedByteInstruction(int index, int offset, OpCode opcode, sbyte value)
		: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
	{
		Value = value;
	}

	/// <summary>
	/// The <see cref="sbyte"/> value of the instruction.
	/// </summary>
	/// <remarks>Property type is <see cref="short"/> to ensure common language specification compliance</remarks>
	public short Value { get; }

	/// <inheritdoc/>
	public override string ToString()
		=> $"{base.ToString()}  [SByte Value: {Value}]";
}
