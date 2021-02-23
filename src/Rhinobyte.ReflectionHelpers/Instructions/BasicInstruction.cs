using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class BasicInstruction : InstructionBase
	{
		public BasicInstruction(int offset, OpCode opcode)
			: base(offset, opcode)
		{

		}

		public override string ToString()
		{
			// TODO: Replace TryGetValue with a plain index lookup once all of the DescriptionLookup values have been filled out
			if (OpCodeHelper.DescriptionLookup.TryGetValue(OpCode.Value, out var description))
			{
				return description;
			}

			return base.ToString();
		}
	}
}
