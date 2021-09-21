using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="LocalVariableInfo"/> index.
	/// </summary>
	public sealed class LocalVariableInstruction : InstructionBase
	{
		internal LocalVariableInstruction(int index, int offset, OpCode opcode, LocalVariableInfo localVariable)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			LocalVariable = localVariable;
		}

		/// <summary>
		/// The <see cref="LocalVariableInfo"/> local variable of the instruction.
		/// </summary>
		public LocalVariableInfo LocalVariable { get; }

		public override string ToString()
		{
			if (LocalVariable == null)
			{
				return $"{base.ToString()} [Missing LocalVariableInfo]";
			}

			var indexString = OpCodeHelper.GetOperandSize(OpCode.OperandType) == 0
				? string.Empty
				: $"  (Index {LocalVariable.LocalIndex})";

			return $"{base.ToString()}{indexString}  [Of type {LocalVariable.LocalType?.Name ?? "UnknownType"}]";
		}
	}
}
