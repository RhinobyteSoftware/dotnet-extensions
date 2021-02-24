using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ThisKeywordInstruction : InstructionBase
	{
		public ThisKeywordInstruction(int offset, OpCode opcode, MethodBase method)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Method = method;
		}

		/// <summary>
		/// The <see cref="MethodBase"/> of the "this" keyword.
		/// </summary>
		public MethodBase Method { get; }
	}
}
