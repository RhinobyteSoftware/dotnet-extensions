using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// Instruction class with an associated <see cref="MethodBase"/> reference token operand.
/// </summary>
public sealed class MethodReferenceInstruction : InstructionBase
{
	internal MethodReferenceInstruction(int index, int offset, OpCode opcode, MethodBase? methodReference)
		: base(index, offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
	{
		MethodReference = methodReference;
	}


	/// <summary>
	/// The <see cref="MethodBase"/> reference of the instruction.
	/// </summary>
	public MethodBase? MethodReference { get; }

	/// <inheritdoc/>
	public override string ToString()
	{
		if (MethodReference is null)
		{
			return $"{base.ToString()}  [Missing MethodReference]";
		}

		var methodName = MethodReference.DeclaringType is null
			? $"{MethodReference.Name} (UnknownType)"
			: $"{MethodReference.DeclaringType.Name}.{MethodReference.Name}";

		return $"{base.ToString()}  [{methodName}]";
	}
}
