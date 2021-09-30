using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="FieldInfo"/> reference token operand.
	/// </summary>
	public sealed class FieldReferenceInstruction : InstructionBase
	{
		internal FieldReferenceInstruction(int index, int offset, OpCode opcode, FieldInfo? fieldReference)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			FieldReference = fieldReference;
		}

		/// <summary>
		/// The <see cref="FieldInfo"/> reference of the instruction.
		/// </summary>
		public FieldInfo? FieldReference { get; }

		/// <inheritdoc/>
		public override string ToString()
			=> $"{base.ToString()}  [FieldReference: {FieldReference?.Name}]";
	}
}
