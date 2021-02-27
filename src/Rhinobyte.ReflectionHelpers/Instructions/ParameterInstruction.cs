using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class with an associated <see cref="ParameterInfo"/> index.
	/// </summary>
	public sealed class ParameterReferenceInstruction : InstructionBase
	{
		internal ParameterReferenceInstruction(int index, int offset, OpCode opcode, ParameterInfo parameterReference)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			ParameterReference = parameterReference;
		}

		/// <summary>
		/// The <see cref="ParameterInfo"/> reference of the instruction.
		/// </summary>
		public ParameterInfo ParameterReference { get; }

		public override string ToString()
		{
			if (ParameterReference == null)
			{
				return $"{base.ToString()}  [ParameterReference: null]";
			}

			return $"{base.ToString()}  [ParameterReference: {ParameterReference.ParameterType?.FullName ?? "(Unknown Parameter Type)"} {ParameterReference.Name}{(ParameterReference.IsOptional ? " (Optional)" : null)}]";
		}
	}
}
