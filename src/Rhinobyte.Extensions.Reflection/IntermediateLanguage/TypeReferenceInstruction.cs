using System;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="Type"/> reference token operand.
	/// </summary>
	public sealed class TypeReferenceInstruction : InstructionBase
	{
		internal TypeReferenceInstruction(int index, int offset, OpCode opcode, Type typeReference)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			TypeReference = typeReference;
		}

		/// <summary>
		/// The <see cref="Type"/> reference of the instruction.
		/// </summary>
		public Type TypeReference { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			if (TypeReference is null)
			{
				return $"{base.ToString()}  [TypeReference: null]";
			}

			return $"{base.ToString()}  [TypeReference: {TypeReference.Name}]";
		}
	}
}
