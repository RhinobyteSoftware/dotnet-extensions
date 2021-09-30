using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="MemberInfo"/> reference token operand. 
	/// <para>
	/// This is a failsafe type that shouldn't ever actually be used. The token should be resolved to a FieldInfo, MethodBase, of Type instance and an instruction type specific to the resolved member is what the
	/// parser should actually return.
	/// </para>
	/// </summary>
	public sealed class UnknownMemberReferenceInstruction : InstructionBase
	{
		internal UnknownMemberReferenceInstruction(int index, int offset, OpCode opcode, MemberInfo? memberReference)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			MemberReference = memberReference;
		}

		/// <summary>
		/// The <see cref="MemberInfo"/> reference of the instruction.
		/// </summary>
		public MemberInfo? MemberReference { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			if (MemberReference is null)
			{
				return $"{base.ToString()}  [MemberReference: null]";
			}

			return $"{base.ToString()}  [MemberReference: {MemberReference.MemberType}  {MemberReference.Name} ({MemberReference.DeclaringType?.Name ?? "UnknownDeclaringType"})]";
		}
	}
}
