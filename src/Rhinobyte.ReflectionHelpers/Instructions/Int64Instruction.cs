﻿using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class Int64Instruction : InstructionBase
	{
		public Int64Instruction(int offset, OpCode opcode, long value)
			: base(offset, opcode, opcode.Size + 8)
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="long"/> value of the instruction.
		/// </summary>
		public long Value { get; }
	}
}
