using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// A contract for a type that can be used to match a target member reference against member references in IL byte code.
	/// </summary>
	public interface IMemberReferenceMatchInfo
	{
		/// <summary>
		/// Determine if the provided <paramref name="ilInstruction"/> contains a reference to the member info represented by this instance.
		/// </summary>
		bool DoesInstructionReferenceMatch(InstructionBase ilInstruction);
		/// <summary>
		/// Determine if the provided <paramref name="instructionMemberReference"/> matches the member info represented by this instance.
		/// </summary>
		bool DoesInstructionReferenceMatch(MemberInfo? instructionMemberReference);
	}
}
