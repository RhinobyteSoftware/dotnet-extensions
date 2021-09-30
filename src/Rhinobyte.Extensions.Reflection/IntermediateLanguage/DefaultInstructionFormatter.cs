using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Default implementation of <see cref="IInstructionFormatter"/> that uses the <see cref="InstructionBase.ToString"/> method.
	/// <para>When describing multiple instructions <see cref="Environment.NewLine"/> is used as the separator between individual instructions.</para>
	/// </summary>
	public class DefaultInstructionFormatter : IInstructionFormatter
	{
		/// <inheritdoc/>
		public virtual string DescribeInstruction(InstructionBase instruction)
			=> instruction?.ToString() ?? string.Empty;

		/// <inheritdoc/>
		public virtual string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe)
			=> string.Join(Environment.NewLine, instructionsToDescribe.Select(instruction => DescribeInstruction(instruction)));
	}
}
