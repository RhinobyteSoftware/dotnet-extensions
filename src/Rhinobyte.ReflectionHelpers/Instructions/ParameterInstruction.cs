using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ParameterReferenceInstruction : InstructionBase
	{
		public ParameterInfo ParameterReference { get; }

		public ParameterReferenceInstruction(int offset, OpCode opcode, ParameterInfo parameterReference)
			: base(offset, opcode)
		{
			ParameterReference = parameterReference;
		}
	}
}
