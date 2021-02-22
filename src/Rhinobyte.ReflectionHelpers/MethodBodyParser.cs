using Rhinobyte.ReflectionHelpers.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers
{
	internal class MethodBodyParser
	{
		private int _bytePosition;
		private readonly byte[] _ilBytes;
		private readonly Module _module;

		internal MethodBodyParser(MethodInfo method)
		{
			_ = method ?? throw new ArgumentNullException(nameof(method));

			var methodBody = method.GetMethodBody()
				?? throw new ArgumentException($"{nameof(method)}.{nameof(MethodBase.GetMethodBody)}() returned null for the method: {method.Name}");

			_ilBytes = methodBody.GetILAsByteArray()
				?? throw new ArgumentException($"{nameof(MethodBody)}.{nameof(MethodBody.GetILAsByteArray)}() returned null for the method: {method.Name}");

			_module = method.Module ?? throw new ArgumentNullException($"{nameof(method)}.{nameof(method.Module)} is null");
		}

		internal IReadOnlyCollection<InstructionBase> ParseInstructions()
		{
			_bytePosition = 0;
			var instructions = new List<InstructionBase>();

			while (_bytePosition < _ilBytes.Length)
			{
				var instructionOffset = _bytePosition;
				var opcodeByte = ReadByte();
				var currentOpcode = opcodeByte != 254 // OpCodes.Prefix1
					? OpCodeHelper.SingleByteOpCodeLookup[opcodeByte]
					: OpCodeHelper.TwoByteOpCodeLookup[ReadByte()];

				switch (currentOpcode.OperandType)
				{
					case OperandType.InlineBrTarget:
						// Set the targetOffset = current _bytePosition + int32 operand following the opcode... we'll iterate over the
						// instructions later and set the TargetInstruction property once we have all the instruction offsets calculated
						instructions.Add(new InlineBranchTargetInstruction(false, instructionOffset, currentOpcode, _bytePosition + ReadInt32()));
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
						instructions.Add(new InlineInt32Instruction(instructionOffset, currentOpcode, ReadInt32()));
						break;

					case OperandType.InlineI8:
						instructions.Add(new InlineInt64Instruction(instructionOffset, currentOpcode, ReadInt64()));
						break;

					case OperandType.InlineNone:
						break;

					case OperandType.InlineR:
						instructions.Add(new InlineDoubleInstruction(instructionOffset, currentOpcode, ReadDouble()));
						break;

					case OperandType.InlineSig:
						instructions.Add(new InlineSignatureInstruction(instructionOffset, currentOpcode, _module.ResolveSignature(ReadInt32())));
						break;

					case OperandType.InlineString:
						instructions.Add(new InlineStringInstruction(instructionOffset, currentOpcode, _module.ResolveString(ReadInt32())));
						break;

					case OperandType.InlineSwitch:
						// Construct switch instruction
						break;

					case OperandType.InlineVar:
						// Construct variable/argument instruction
						break;

					case OperandType.ShortInlineBrTarget:
						// Set the targetOffset = current _bytePosition + int8 operand following the opcode... we'll iterate over the
						// instructions later and set the TargetInstruction property once we have all the instruction offsets calculated
						instructions.Add(new InlineBranchTargetInstruction(true, instructionOffset, currentOpcode, _bytePosition + ((sbyte)ReadByte())));
						break;

					case OperandType.ShortInlineI:
						if (currentOpcode == OpCodes.Ldc_I4_S)
						{
							instructions.Add(new InlineSignedByteInstruction(instructionOffset, currentOpcode, (sbyte)ReadByte()));
							break;
						}

						instructions.Add(new InlineByteInstruction(instructionOffset, currentOpcode, ReadByte()));
						break;

					case OperandType.ShortInlineR:
						instructions.Add(new InlineFloatInstruction(instructionOffset, currentOpcode, ReadSingle()));
						break;

					case OperandType.ShortInlineVar:
						// Construct 8bit variable/argument instruction
						break;

					default:
						throw new NotSupportedException($"{nameof(MethodBodyParser)}.{nameof(ParseInstructions)}() is not supported for an {nameof(OperandType)} value of {currentOpcode.OperandType}");
				}
			}

			foreach (var instructionToUpdate in instructions)
			{
				switch (instructionToUpdate.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
						var branchTargetInstruction = (InlineBranchTargetInstruction)instructionToUpdate;

						// TODO: Optimize this... could track the offset -> instruction mappings using a dictionary or could use a binary search on the instructions list in stead of a brute force iteration of the list
						var targetInstruction = instructions.FirstOrDefault(nextInstruction => nextInstruction.Offset == branchTargetInstruction.TargetOffset);
						if (targetInstruction == null)
						{
							throw new InvalidOperationException($"Failed to locate the targetInstruction for target offset {branchTargetInstruction.TargetOffset}. [BranchTargetInstructionOffset: {instructionToUpdate.Offset}]");
						}

						branchTargetInstruction.TargetInstruction = targetInstruction;
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
		/// Convenience method to read the next eight bytes as a double value and to advance the _bytePosition
		/// </summary>
		internal double ReadDouble()
		{
			var doubleValue = BitConverter.ToDouble(_ilBytes, _bytePosition);
			_bytePosition += 8;
			return doubleValue;
		}

		/// <summary>
		/// Convenience method to read the next four bytes as an Int32 value and to advance the _bytePosition
		/// </summary>
		internal int ReadInt32()
		{
			var intValue = BitConverter.ToInt32(_ilBytes, _bytePosition);
			_bytePosition += 4;
			return intValue;
		}

		/// <summary>
		/// Convenience method to read the next eight bytes as an Int64 value and to advance the _bytePosition
		/// </summary>
		internal long ReadInt64()
		{
			var longValue = BitConverter.ToInt64(_ilBytes, _bytePosition);
			_bytePosition += 8;
			return longValue;
		}

		/// <summary>
		/// Convenience method to read the next four bytes as a float value and to advance the _bytePosition
		/// </summary>
		internal float ReadSingle()
		{
			var floatValue = BitConverter.ToSingle(_ilBytes, _bytePosition);
			_bytePosition += 4;
			return floatValue;
		}

	}
}
