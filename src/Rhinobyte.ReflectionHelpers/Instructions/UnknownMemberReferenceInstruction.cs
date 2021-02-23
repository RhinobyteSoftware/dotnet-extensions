using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class UnknownMemberReferenceInstruction : InstructionBase
	{
		public MemberInfo MemberReference { get; }

		public UnknownMemberReferenceInstruction(int offset, OpCode opcode, MemberInfo memberReference)
			: base(offset, opcode)
		{
			MemberReference = memberReference;
		}
	}
}
