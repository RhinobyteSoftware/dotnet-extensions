using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class LocalVariableInstruction : InstructionBase
	{
		public LocalVariableInstruction(bool isShortFormOperand, int offset, OpCode opcode, LocalVariableInfo localVariable)
			: base(offset, opcode, opcode.Size + (isShortFormOperand ? 1 : 4))
		{
			IsShortFormOperand = isShortFormOperand;
			LocalVariable = localVariable;
		}

		/// <summary>
		/// Whether or not the opcode uses a short form (8bit) operand or a full 32bit operand.
		/// </summary>
		public bool IsShortFormOperand { get; }

		/// <summary>
		/// The <see cref="LocalVariableInfo"/> local variable of the instruction.
		/// </summary>
		public LocalVariableInfo LocalVariable { get; }
	}
}
