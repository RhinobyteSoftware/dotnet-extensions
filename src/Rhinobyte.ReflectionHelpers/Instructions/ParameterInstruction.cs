using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ParameterReferenceInstruction : InstructionBase
	{
		public ParameterReferenceInstruction(int offset, OpCode opcode, ParameterInfo parameterReference)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			ParameterReference = parameterReference;
		}

		/// <summary>
		/// The <see cref="ParameterInfo"/> reference of the instruction.
		/// </summary>
		public ParameterInfo ParameterReference { get; }
	}
}
