using System;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class TypeReferenceInstruction : InstructionBase
	{
		public Type TypeReference { get; }

		public TypeReferenceInstruction(int offset, OpCode opcode, Type typeReference)
			: base(offset, opcode)
		{
			TypeReference = typeReference;
		}
	}
}
