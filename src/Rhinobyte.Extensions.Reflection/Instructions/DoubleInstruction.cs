using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Instructions
{
	/// <summary>
	/// Instruction class with an associated <see cref="double"/> value operand.
	/// </summary>
	public sealed class DoubleInstruction : InstructionBase
	{
		internal DoubleInstruction(int index, int offset, OpCode opcode, double value)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="double"/> value of the instruction.
		/// </summary>
		public double Value { get; }

		public override string ToString()
			=> $"{base.ToString()}  [Double Value: {Value}]";
	}
}
