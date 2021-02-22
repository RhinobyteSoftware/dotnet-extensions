using Rhinobyte.ReflectionHelpers.Instructions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers
{
	internal class MethodBodyParser
	{
		private int _bytePosition;
		private readonly byte[] _ilBytes;

		internal MethodBodyParser(MethodInfo method)
		{
			_ = method ?? throw new ArgumentNullException(nameof(method));

			var methodBody = method.GetMethodBody()
				?? throw new ArgumentException($"{nameof(method)}.{nameof(MethodBase.GetMethodBody)}() returned null for the method: {method.Name}");

			_ilBytes = methodBody.GetILAsByteArray()
				?? throw new ArgumentException($"{nameof(MethodBody)}.{nameof(MethodBody.GetILAsByteArray)}() returned null for the method: {method.Name}");
		}

		internal IReadOnlyCollection<InstructionBase> ParseInstructions()
		{
			_bytePosition = 0;
			var instructions = new List<InstructionBase>();

			while (_bytePosition < _ilBytes.Length)
			{
				var opcodeByte = ReadByte();
				var currentOpcode = opcodeByte != 254 // OpCodes.Prefix1
					? OpCodeHelper.SingleByteOpCodeLookup[opcodeByte]
					: OpCodeHelper.TwoByteOpCodeLookup[ReadByte()];

				switch (currentOpcode.OperandType)
				{
					case OperandType.InlineBrTarget:
						instructions.Add(new InlineBranchTargetInstruction(currentOpcode, ReadInt32() + _bytePosition));
						break;

					case OperandType.InlineField:
						// Construct field instruction
						break;

					case OperandType.InlineMethod:
						// Construct method instruction
						break;

					case OperandType.InlineTok:
						// Construct reference token instruction
						break;

					case OperandType.InlineType:
						// Construct type instruction
						break;

					case OperandType.InlineI:
						// Construct int32 instruction
						break;

					case OperandType.InlineI8:
						// Construct int64 instruction
						break;

					case OperandType.InlineNone:
						break;

					case OperandType.InlineR:
						// Construct 64-bit floating point instruction
						break;

					case OperandType.InlineSig:
						// Construct signature instruction
						break;

					case OperandType.InlineString:
						// Construct string instruction
						break;

					case OperandType.InlineSwitch:
						// Construct switch instruction
						break;

					case OperandType.InlineVar:
						// Construct variable/argument instruction
						break;

					case OperandType.ShortInlineBrTarget:
						// Construct 8bit branch target instruction
						break;

					case OperandType.ShortInlineI:
						// Construct 8bit integer instruction
						break;

					case OperandType.ShortInlineR:
						// Construct 32-bit floating point instruction
						break;

					case OperandType.ShortInlineVar:
						// Construct 8bit variable/argument instruction
						break;

					default:
						throw new NotSupportedException($"{nameof(MethodBodyParser)}.{nameof(ParseInstructions)}() is not supported for an {nameof(OperandType)} value of {currentOpcode.OperandType}");
				}
			}

			foreach (var instruction in instructions)
			{
				switch (instruction.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
						// TODO: Lookup the instruction for the TargetOffset and set the TargetInstruction property
						break;
				}
			}

			return instructions;
		}

		/// <summary>
		/// Convenience method to read the next byte and advance the _bytePosition.
		/// </summary>
		internal byte ReadByte()
		{
			if (_bytePosition >= _ilBytes.Length)
			{
				throw new InvalidOperationException("End of byte array reached");
			}

			return _ilBytes[_bytePosition++];
		}

		/// <summary>
		/// Convenience method to read the next four bytes as an Int32 value and to advance the _bytePosition
		/// </summary>
		internal int ReadInt32()
		{
			if (_bytePosition + 4 > _ilBytes.Length)
			{
				throw new InvalidOperationException("End of byte array reached");
			}

			var intValue = _ilBytes[_bytePosition]
				| (_ilBytes[_bytePosition + 1] << 8)
				| (_ilBytes[_bytePosition + 2] << 16)
				| (_ilBytes[_bytePosition + 3] << 24);

			_bytePosition += 4;
			return intValue;
		}

	}
}
