using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class with an associated <see cref="MethodBase"/> reference token operand.
	/// </summary>
	public sealed class MethodReferenceInstruction : InstructionBase
	{
		internal MethodReferenceInstruction(int offset, OpCode opcode, MethodBase methodReference)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			MethodReference = methodReference;
		}


		/// <summary>
		/// The <see cref="MethodBase"/> reference of the instruction.
		/// </summary>
		public MethodBase MethodReference { get; }

		public override string ToString()
		{
			if (MethodReference == null)
			{
				return $"{base.ToString()}  [MethodReference: null]";
			}

			var methodName = MethodReference.DeclaringType == null
				? $"{MethodReference.Name} (UnknownType)"
				: $"{MethodReference.DeclaringType.Name}.{MethodReference.Name}";

			return $"{base.ToString()}  [MethodReference: {methodName}]";
		}
	}
}
