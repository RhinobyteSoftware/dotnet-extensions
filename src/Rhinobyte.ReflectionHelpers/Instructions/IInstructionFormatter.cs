using System.Collections.Generic;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public interface IInstructionFormatter
	{
		string DescribeInstruction(InstructionBase instruction);
		string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe);
	}
}
