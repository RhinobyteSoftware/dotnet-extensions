using System;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class TypeReferenceInstruction : InstructionBase
	{
		public TypeReferenceInstruction(int offset, OpCode opcode, Type typeReference)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			TypeReference = typeReference;
		}

		/// <summary>
		/// The <see cref="Type"/> reference of the instruction.
		/// </summary>
		public Type TypeReference { get; }

		public override string ToString()
		{
			if (TypeReference == null)
			{
				return $"{base.ToString()}  [TypeReference: null]";
			}

			return $"{base.ToString()}  [TypeReference: {TypeReference.Name}]";
		}
	}
}
