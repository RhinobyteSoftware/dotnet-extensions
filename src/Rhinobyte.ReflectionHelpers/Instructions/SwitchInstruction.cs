using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class SwitchInstruction : InstructionBase
	{
		internal SwitchInstruction(int offset, OpCode opcode, IReadOnlyCollection<int> targetOffsets)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType) + (targetOffsets.Count * 4))
		{
			TargetInstructions = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction set
			TargetOffsets = targetOffsets;
		}

		public SwitchInstruction(int offset, OpCode opcode, IReadOnlyCollection<InstructionBase> targetInstructions, IReadOnlyCollection<int> targetOffsets)
			: base(
				offset,
				opcode,
				opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType) + (4 * targetOffsets?.Count ?? throw new ArgumentNullException(nameof(targetOffsets)))
			)
		{
			TargetInstructions = targetInstructions ?? throw new ArgumentNullException(nameof(targetInstructions));
			TargetOffsets = targetOffsets!;
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
