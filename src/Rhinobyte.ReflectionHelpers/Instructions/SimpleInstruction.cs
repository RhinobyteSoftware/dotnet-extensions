using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class for simple intermediate language (IL) instructions that do not require operands in the IL bytes.
	/// </summary>
	public sealed class SimpleInstruction : InstructionBase
	{
		internal SimpleInstruction(int offset, OpCode opcode)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{

		}
	}
}
