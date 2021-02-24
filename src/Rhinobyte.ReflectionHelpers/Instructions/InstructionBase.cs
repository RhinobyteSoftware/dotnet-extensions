using System;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	/// <summary>
	/// Abstract base class representing an individual intermediate language (IL) instruction.
	/// </summary>
	public abstract class InstructionBase
	{
		protected InstructionBase(int offset, OpCode opcode, int size)
		{
			Offset = offset;
			OpCode = opcode;
			Size = size;
		}


		/// <summary>
		/// The relative offset of this instruction within the intermediate language (IL) bytes.
		/// </summary>
		public int Offset { get; }

		/// <summary>
		/// The <see cref="OpCode"/> for this instruction.
		/// </summary>
		public OpCode OpCode { get; }

		/// <summary>
		/// The size in bytes of the intermediate language (IL) instruction.
		/// </summary>
		public virtual int Size { get; }

		public virtual string FullDescription() =>
$@"{ToString()}
[{GetType().Name}   Offset: {Offset}   Size: {Size}]
[OpCode: {OpCodeHelper.NameLookup[OpCode.Value]}   OperandType: {OpCode.OperandType}   OpCodeType: {OpCode.OpCodeType}   Size: {OpCode.Size}   StackBehaviourPop: {OpCode.StackBehaviourPop}   Opcode.StackBehaviourPush: {OpCode.StackBehaviourPush}]";

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
