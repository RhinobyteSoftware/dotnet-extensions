using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="string"/> value.
	/// </summary>
	public sealed class StringInstruction : InstructionBase
	{
		internal StringInstruction(int index, int offset, OpCode opcode, string value)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="string"/> value of the instruction.
		/// </summary>
		public string Value { get; }

		/// <inheritdoc/>
		public override string ToString()
			=> $"{base.ToString()}  [String Value: {Value}]";
	}
}
