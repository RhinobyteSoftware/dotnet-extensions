using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// A formatter that generates a string description for one or more <see cref="InstructionBase"/> objects
	/// </summary>
	public interface IInstructionFormatter
	{
		/// <summary>
		/// Return a string description for the specified <paramref name="instruction"/>
		/// </summary>
		string DescribeInstruction(InstructionBase instruction);
		/// <summary>
		/// Return a string description for the specified collection of <paramref name="instructionsToDescribe"/>
		/// </summary>
		string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe);
	}
}
