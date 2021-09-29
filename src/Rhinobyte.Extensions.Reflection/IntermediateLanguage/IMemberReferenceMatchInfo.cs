using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// A contract for a type that can be used to match a target member reference against member references in IL byte code.
	/// </summary>
	public interface IMemberReferenceMatchInfo
	{
		bool DoesInstructionReferenceMatch(InstructionBase ilInstruction);
		bool DoesInstructionReferenceMatch(MemberInfo? instructionMemberReference);
	}
}
