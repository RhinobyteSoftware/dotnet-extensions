using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class StringInstruction : InstructionBase
	{
		public StringInstruction(int offset, OpCode opcode, string value)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="string"/> value of the instruction.
		/// </summary>
		public string Value { get; }
	}
}
