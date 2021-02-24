using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class UnknownMemberReferenceInstruction : InstructionBase
	{
		public UnknownMemberReferenceInstruction(int offset, OpCode opcode, MemberInfo memberReference)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			MemberReference = memberReference;
		}

		/// <summary>
		/// The <see cref="MemberInfo"/> reference of the instruction.
		/// </summary>
		public MemberInfo MemberReference { get; }
	}
}
