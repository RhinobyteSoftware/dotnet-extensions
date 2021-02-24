using System.Collections.Generic;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class for switch opcodes that build a jump table. Includes arrays for the target offsets / target instructions of the jump table.
	/// </summary>
	public sealed class SwitchInstruction : InstructionBase
	{
		internal SwitchInstruction(int offset, OpCode opcode, IReadOnlyCollection<int> targetOffsets)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType) + (targetOffsets.Count * 4))
		{
			TargetInstructions = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction set
			TargetOffsets = targetOffsets;
		}

		/// <summary>
		/// The collection of <see cref="InstructionBase"/> targets of the switch statement's jump table.
		/// </summary>
		public IReadOnlyCollection<InstructionBase> TargetInstructions { get; internal set; }

		/// <summary>
		/// The instruction target offsets of the switch statement's jump table.
		/// </summary>
		public IReadOnlyCollection<int> TargetOffsets { get; }

		public override string ToString()
			=> $"{base.ToString()}  [TargetOffsets: {string.Join(", ", TargetOffsets)}]";
	}
}
