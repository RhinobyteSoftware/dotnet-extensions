using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class MethodReferenceInstruction : InstructionBase
	{
		public MethodReferenceInstruction(int offset, OpCode opcode, MethodBase methodReference)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			MethodReference = methodReference;
		}


		/// <summary>
		/// The <see cref="MethodBase"/> reference of the instruction.
		/// </summary>
		public MethodBase MethodReference { get; }
	}
}
