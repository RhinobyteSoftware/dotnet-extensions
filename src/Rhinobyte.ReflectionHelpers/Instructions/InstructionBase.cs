using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Abstract base class representing an individual intermediate language (IL) instruction.
	/// </summary>
	public abstract class InstructionBase
	{
		protected InstructionBase(int index, int offset, OpCode opcode, int size)
		{
			Index = index;
			Offset = offset;
			OpCode = opcode;
			Size = size;
		}

		/// <summary>
		/// The instruction's index within the list of method body instructions.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// The next instruction in the intermediate language (IL) bytes.
		/// </summary>
		public InstructionBase? NextInstruction { get; internal set; }

		/// <summary>
		/// The relative offset of this instruction within the intermediate language (IL) bytes.
		/// </summary>
		public int Offset { get; }

		/// <summary>
		/// The <see cref="OpCode"/> for this instruction.
		/// </summary>
		public OpCode OpCode { get; }

		/// <summary>
		/// The previous instruction in the intermediate language (IL) bytes.
		/// </summary>
		public InstructionBase? PreviousInstruction { get; internal set; }

		/// <summary>
		/// The size in bytes of the intermediate language (IL) instruction.
		/// </summary>
		public virtual int Size { get; }

		public virtual string FullDescription() =>
$@"{ToString()}
[{GetType().Name}   Offset: {Offset}   Size: {Size}]
[OpCode: {OpCodeHelper.NameLookup[OpCode.Value]} ({OpCode.Value})   OperandType: {OpCode.OperandType}   OpCodeType: {OpCode.OpCodeType}   Size: {OpCode.Size}   StackBehaviourPop: {OpCode.StackBehaviourPop}   StackBehaviourPush: {OpCode.StackBehaviourPush}]";

		public override string ToString()
		{
			if (OpCodeHelper.ShortDescriptionLookup.TryGetValue(OpCode.Value, out var description))
			{
				return $"({Index}) {description}";
			}

			return $"({Index}) {base.ToString()}";
		}
	}
}
