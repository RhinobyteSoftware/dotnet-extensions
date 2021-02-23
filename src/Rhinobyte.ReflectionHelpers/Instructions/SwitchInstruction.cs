using System.Collections.Generic;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class SwitchInstruction : InstructionBase
	{
		public IReadOnlyCollection<InstructionBase> TargetInstructions { get; internal set; }
		public IReadOnlyCollection<int> TargetOffsets { get; }

		internal SwitchInstruction(int offset, OpCode opcode, IReadOnlyCollection<int> targetOffsets)
			: base(offset, opcode)
		{
			TargetInstructions = null!; // Nullability hack, the parser will be responsible for ensuring this is always set to a non-null instruction set
			TargetOffsets = targetOffsets;
		}

		public SwitchInstruction(int offset, OpCode opcode, IReadOnlyCollection<InstructionBase> targetInstructions, IReadOnlyCollection<int> targetOffsets)
			: base(offset, opcode)
		{
			TargetInstructions = targetInstructions;
			TargetOffsets = targetOffsets;
		}
	}
}
