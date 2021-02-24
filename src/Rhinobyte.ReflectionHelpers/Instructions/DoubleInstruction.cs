﻿using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.Instructions
{
	public sealed class DoubleInstruction : InstructionBase
	{
		public DoubleInstruction(int offset, OpCode opcode, double value)
			: base(offset, opcode, opcode.Size + 8)
		{
			Value = value;
		}

		/// <summary>
		/// The <see cref="double"/> value of the instruction.
		/// </summary>
		public double Value { get; }
	}
}
