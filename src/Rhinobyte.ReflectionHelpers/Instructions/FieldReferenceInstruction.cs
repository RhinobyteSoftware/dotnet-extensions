using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class FieldReferenceInstruction : InstructionBase
	{
		public FieldInfo FieldReference { get; }

		public FieldReferenceInstruction(int offset, OpCode opcode, FieldInfo fieldReference)
			: base(offset, opcode)
		{
			FieldReference = fieldReference;
		}
	}
}
