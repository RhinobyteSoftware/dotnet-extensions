using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// A formatter that generates a string description for one or more <see cref="InstructionBase"/> objects
	/// </summary>
	public interface IInstructionFormatter
	{
		string DescribeInstruction(InstructionBase instruction);
		string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe);
	}
}
