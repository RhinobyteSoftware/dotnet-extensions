using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Instructions
{
	/// <summary>
	/// Instruction class for switch opcodes that build a jump table. Includes arrays for the target offsets / target instructions of the jump table.
	/// </summary>
	public sealed class SwitchInstruction : InstructionBase
	{
		internal SwitchInstruction(int index, int offset, OpCode opcode, IReadOnlyCollection<int> targetOffsets)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType) + (targetOffsets.Count * 4))
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
		{
			if (TargetInstructions == null)
			{
				return $"{base.ToString()}  [TargetInstructions: null]  [TargetOffsets: {string.Join(", ", TargetOffsets)}]";
			}

			return $"{base.ToString()}  [TargetInstructions: {string.Join(", ", TargetInstructions.Select(instruction => instruction.Index))}]  [TargetOffsets: {string.Join(", ", TargetOffsets)}]";
		}
	}
}
