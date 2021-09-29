using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	public interface IMemberReferenceMatchInfo
	{
		bool DoesInstructionReferenceMatch(InstructionBase ilInstruction);
		bool DoesInstructionReferenceMatch(MemberInfo? instructionMemberReference);
	}
}
