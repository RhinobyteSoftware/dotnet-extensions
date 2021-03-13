using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public class DefaultInstructionFormatter : IInstructionFormatter
	{
		public virtual string DescribeInstruction(InstructionBase instruction)
			=> instruction?.ToString() ?? string.Empty;

		public virtual string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe)
			=> string.Join(Environment.NewLine, instructionsToDescribe.Select(instruction => DescribeInstruction(instruction)));
	}
}
