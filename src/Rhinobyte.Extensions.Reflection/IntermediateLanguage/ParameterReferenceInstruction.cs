using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Instruction class with an associated <see cref="ParameterInfo"/> index.
	/// </summary>
	public sealed class ParameterReferenceInstruction : InstructionBase
	{
		internal ParameterReferenceInstruction(int index, int offset, OpCode opcode, int parameterIndex, ParameterInfo parameterReference)
			: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			ParameterIndex = parameterIndex;
			ParameterReference = parameterReference;
		}

		/// <summary>
		/// The index of the parameter.
		/// </summary>
		public int ParameterIndex { get; }

		/// <summary>
		/// The <see cref="ParameterInfo"/> reference of the instruction.
		/// </summary>
		public ParameterInfo ParameterReference { get; }

		public override string ToString()
		{
			if (ParameterReference == null)
			{
				return $"{base.ToString()}  [Parameter #{ParameterIndex}]  [ParameterReference: null]";
			}

			return $"{base.ToString()}  [Parameter #{ParameterIndex}]  [ParameterReference: {ParameterReference.ParameterType?.FullName ?? ParameterReference.ParameterType?.Name ?? "(Unknown Parameter Type)"} {ParameterReference.Name}{(ParameterReference.IsOptional ? " (Optional)" : null)}]";
		}
	}
}
