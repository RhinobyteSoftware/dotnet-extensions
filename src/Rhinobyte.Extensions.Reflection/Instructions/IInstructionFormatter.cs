using System.Collections.Generic;

namespace Rhinobyte.Extensions.Reflection.Instructions
{
	public interface IInstructionFormatter
	{
		string DescribeInstruction(InstructionBase instruction);
		string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe);
	}
}
