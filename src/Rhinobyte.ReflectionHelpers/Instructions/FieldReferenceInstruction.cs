using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class FieldReferenceInstruction : InstructionBase
	{
		public FieldReferenceInstruction(int offset, OpCode opcode, FieldInfo fieldReference)
			: base(offset, opcode, opcode.Size + 4)
		{
			FieldReference = fieldReference;
		}

		/// <summary>
		/// The <see cref="FieldInfo"/> reference of the instruction.
		/// </summary>
		public FieldInfo FieldReference { get; }
	}
}
