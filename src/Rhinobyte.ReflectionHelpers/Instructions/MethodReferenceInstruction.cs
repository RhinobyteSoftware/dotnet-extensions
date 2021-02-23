using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class MethodReferenceInstruction : InstructionBase
	{
		public MethodBase MethodReference { get; }

		public MethodReferenceInstruction(int offset, OpCode opcode, MethodBase methodReference)
			: base(offset, opcode)
		{
			MethodReference = methodReference;
		}
	}
}
