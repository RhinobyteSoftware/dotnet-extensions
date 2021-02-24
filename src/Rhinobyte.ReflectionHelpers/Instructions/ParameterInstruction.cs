using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ParameterReferenceInstruction : InstructionBase
	{
		public ParameterReferenceInstruction(bool isShortFormOperand, int offset, OpCode opcode, ParameterInfo parameterReference)
			: base(offset, opcode, opcode.Size + (isShortFormOperand ? 1 : 4))
		{
			IsShortFormOperand = isShortFormOperand;
			ParameterReference = parameterReference;
		}

		/// <summary>
		/// Whether or not the opcode uses a short form (8bit) operand or a full 32bit operand.
		/// </summary>
		public bool IsShortFormOperand { get; }

		/// <summary>
		/// The <see cref="ParameterInfo"/> reference of the instruction.
		/// </summary>
		public ParameterInfo ParameterReference { get; }
	}
}
