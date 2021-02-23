using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class LocalVariableInstruction : InstructionBase
	{
		public LocalVariableInfo LocalVariable { get; }

		public LocalVariableInstruction(int offset, OpCode opcode, LocalVariableInfo localVariable)
			: base(offset, opcode)
		{
			LocalVariable = localVariable;
		}
	}
}
