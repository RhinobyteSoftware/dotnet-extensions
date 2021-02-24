using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ThisKeywordInstruction : InstructionBase
	{
		public ThisKeywordInstruction(bool isShortFormOperand, int offset, OpCode opcode, MethodBase method)
			: base(offset, opcode, opcode.Size + (isShortFormOperand ? 1 : 4))
		{
			IsShortFormOperand = isShortFormOperand;
			Method = method;
		}

		/// <summary>
		/// Whether or not the opcode uses a short form (8bit) operand or a full 32bit operand.
		/// </summary>
		public bool IsShortFormOperand { get; }

		/// <summary>
		/// The <see cref="MethodBase"/> of the "this" keyword.
		/// </summary>
		public MethodBase Method { get; }
	}
}
