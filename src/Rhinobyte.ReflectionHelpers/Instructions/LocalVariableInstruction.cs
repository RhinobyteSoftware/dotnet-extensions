using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class LocalVariableInstruction : InstructionBase
	{
		public LocalVariableInstruction(int offset, OpCode opcode, LocalVariableInfo localVariable)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			LocalVariable = localVariable;
		}

		/// <summary>
		/// The <see cref="LocalVariableInfo"/> local variable of the instruction.
		/// </summary>
		public LocalVariableInfo LocalVariable { get; }
	}
}
