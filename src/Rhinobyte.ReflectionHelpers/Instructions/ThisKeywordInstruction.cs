using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class ThisKeywordInstruction : InstructionBase
	{
		public MethodBase Method { get; }

		public ThisKeywordInstruction(int offset, OpCode opcode, MethodBase method)
			: base(offset, opcode)
		{
			Method = method;
		}
	}
}
