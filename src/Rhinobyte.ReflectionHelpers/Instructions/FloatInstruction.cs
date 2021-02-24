using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class with an associated <see cref="float"/> value operand.
	/// </summary>
	public sealed class FloatInstruction : InstructionBase
	{
		internal FloatInstruction(int offset, OpCode opcode, float value)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="float"/> value of the instruction.
		/// </summary>
		public float Value { get; }

		public override string ToString()
			=> $"{base.ToString()}  [Float Value: {Value}]";
	}
}
