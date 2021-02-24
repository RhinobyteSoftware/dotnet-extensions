using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Instruction class associated with the "this" keyword which is represented by the method argument index 0 for non static methods.
	/// </summary>
	public sealed class ThisKeywordInstruction : InstructionBase
	{
		internal ThisKeywordInstruction(int offset, OpCode opcode, MethodBase method)
			: base(offset, opcode, opcode.Size + OpCodeHelper.GetOperandSize(opcode.OperandType))
		{
			Method = method;
		}

		/// <summary>
		/// The <see cref="MethodBase"/> of the "this" keyword.
		/// </summary>
		public MethodBase Method { get; }

		public override string ToString()
			=> $"{base.ToString()}  [this keyword]";
	}
}
