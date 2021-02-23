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
		private readonly Type[]? _declaringTypeGenericArguments;
		private readonly byte[] _ilBytes;
		private readonly bool _isStaticMethod;
		private readonly IList<LocalVariableInfo> _localVariables;
		private readonly MethodBase _method;
		private readonly Type[]? _methodGenericArguments;
		private readonly Module _module;
		private readonly ParameterInfo[] _parameters;

		internal MethodBodyParser(MethodBase method)
		{
			_method = method ?? throw new ArgumentNullException(nameof(method));

			var methodBody = method.GetMethodBody()
				?? throw new ArgumentException($"{nameof(method)}.{nameof(MethodBase.GetMethodBody)}() returned null for the method: {method.Name}");

			_ilBytes = methodBody.GetILAsByteArray()
				?? throw new ArgumentException($"{nameof(MethodBody)}.{nameof(MethodBody.GetILAsByteArray)}() returned null for the method: {method.Name}");

			_module = method.Module ?? throw new ArgumentNullException($"{nameof(method)}.{nameof(method.Module)} is null");

			_declaringTypeGenericArguments = method.DeclaringType?.GetGenericArguments();
			_isStaticMethod = method.IsStatic;
			_localVariables = methodBody.LocalVariables;
			_methodGenericArguments = method.GetGenericArguments();
			_parameters = method.GetParameters();
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
						instructions.Add(new BranchTargetInstruction(false, instructionOffset, currentOpcode, _bytePosition + ReadInt32()));
						break;

					case OperandType.InlineField:
						instructions.Add(new FieldReferenceInstruction(instructionOffset, currentOpcode, _module.ResolveField(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments)));
						break;

					case OperandType.InlineI:
						instructions.Add(new Int32Instruction(instructionOffset, currentOpcode, ReadInt32()));
						break;

					case OperandType.InlineI8:
						instructions.Add(new Int64Instruction(instructionOffset, currentOpcode, ReadInt64()));
						break;

					case OperandType.InlineMethod:
						instructions.Add(new MethodReferenceInstruction(instructionOffset, currentOpcode, _module.ResolveMethod(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments)));
						break;

					case OperandType.InlineNone:
						break;

					case OperandType.InlineR:
						instructions.Add(new DoubleInstruction(instructionOffset, currentOpcode, ReadDouble()));
						break;

					case OperandType.InlineSig:
						instructions.Add(new SignatureInstruction(instructionOffset, currentOpcode, _module.ResolveSignature(ReadInt32())));
						break;

					case OperandType.InlineString:
						instructions.Add(new StringInstruction(instructionOffset, currentOpcode, _module.ResolveString(ReadInt32())));
						break;

					case OperandType.InlineSwitch:
						var numberOfTargets = ReadInt32();
						var baseOffset = _bytePosition + (4 * numberOfTargets);
						var targetOffsets = new int[numberOfTargets];
						for (var targetIndex = 0; targetIndex < numberOfTargets; ++targetIndex)
						{
							targetOffsets[targetIndex] = baseOffset + ReadInt32();
						}

						// Set the targetOffsets array now... we'll iterate over the
						// instructions later and set the TargetInstructions property once we have
						// all the instruction offsets calculated
						instructions.Add(new SwitchInstruction(instructionOffset, currentOpcode, targetOffsets));
						break;

					case OperandType.InlineTok:
						var memberReference = _module.ResolveMember(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments);
						switch (memberReference)
						{
							case FieldInfo fieldReference:
								instructions.Add(new FieldReferenceInstruction(instructionOffset, currentOpcode, fieldReference));
								break;

							case MethodBase methodReference:
								instructions.Add(new MethodReferenceInstruction(instructionOffset, currentOpcode, methodReference));
								break;

							case Type typeReference:
								instructions.Add(new TypeReferenceInstruction(instructionOffset, currentOpcode, typeReference));
								break;

							default:
								// Not sure if this is possible or if I should make this case throw...
								instructions.Add(new UnknownMemberReferenceInstruction(instructionOffset, currentOpcode, memberReference));
								break;
						}
						break;

					case OperandType.InlineType:
						instructions.Add(new TypeReferenceInstruction(instructionOffset, currentOpcode, _module.ResolveType(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments)));
						break;

					case OperandType.InlineVar:
					case OperandType.ShortInlineVar:
						var variableIndex = currentOpcode.OperandType == OperandType.InlineVar
							? ReadInt16()
							: ReadByte();

						if (OpCodeHelper.LocalVariableOpcodeValues.Contains(currentOpcode.Value))
						{
							instructions.Add(new LocalVariableInstruction(instructionOffset, currentOpcode, _localVariables[variableIndex]));
							break;
						}

						if (!_isStaticMethod && variableIndex == 0)
						{
							instructions.Add(new ThisKeywordInstruction(instructionOffset, currentOpcode, _method));
							break;
						}

						instructions.Add(new ParameterReferenceInstruction(instructionOffset, currentOpcode, _parameters[variableIndex]));
						break;

					case OperandType.ShortInlineBrTarget:
						// Set the targetOffset = current _bytePosition + int8 operand following the opcode... we'll iterate over the
						// instructions later and set the TargetInstruction property once we have all the instruction offsets calculated
						instructions.Add(new BranchTargetInstruction(true, instructionOffset, currentOpcode, _bytePosition + ((sbyte)ReadByte())));
						break;

					case OperandType.ShortInlineI:
						if (currentOpcode == OpCodes.Ldc_I4_S)
						{
							instructions.Add(new SignedByteInstruction(instructionOffset, currentOpcode, (sbyte)ReadByte()));
							break;
						}

						instructions.Add(new ByteInstruction(instructionOffset, currentOpcode, ReadByte()));
						break;

					case OperandType.ShortInlineR:
						instructions.Add(new FloatInstruction(instructionOffset, currentOpcode, ReadSingle()));
						break;

					default:
						throw new NotSupportedException($"{nameof(MethodBodyParser)}.{nameof(ParseInstructions)}() is not supported for an {nameof(OperandType)} value of {currentOpcode.OperandType}");
				}
			}

			var highestOffset = instructions.Last().Offset;
			foreach (var instructionToUpdate in instructions)
			{
				switch (instructionToUpdate.OpCode.OperandType)
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
						var branchTargetInstruction = (BranchTargetInstruction)instructionToUpdate;
						branchTargetInstruction.TargetInstruction = FindInstructionByOffset(highestOffset, instructions, branchTargetInstruction, branchTargetInstruction.TargetOffset);
						break;

					case OperandType.InlineSwitch:
						var switchInstruction = (SwitchInstruction)instructionToUpdate;
						var targetInstructions = new List<InstructionBase>();
						foreach (var targetOffset in switchInstruction.TargetOffsets)
						{
							targetInstructions.Add(FindInstructionByOffset(highestOffset, instructions, switchInstruction, targetOffset));
						}
						switchInstruction.TargetInstructions = targetInstructions;
						break;
				}
			}

			return instructions;
		}

		internal static InstructionBase FindInstructionByOffset(
			int highestInstructionOffset,
			IReadOnlyList<InstructionBase> instructionsToSearch,
			InstructionBase sourceInstruction,
			int targetOffset)
		{
			if (targetOffset < 0 || targetOffset > highestInstructionOffset)
			{
				throw new InvalidOperationException($"Failed to locate the target instruction for target offset {targetOffset}. [SourceInstruction: {sourceInstruction}]");
			}

			// Perform a binary search for the target offset
			var minIndex = 0;
			var maxIndex = instructionsToSearch.Count - 1;
			while (minIndex <= maxIndex)
			{
				var indexToCheck = minIndex + ((maxIndex - minIndex) / 2);
				var instructionToCheck = instructionsToSearch[indexToCheck];
				var instructionOffset = instructionToCheck.Offset;

				if (instructionOffset == targetOffset)
				{
					return instructionToCheck;
				}

				if (instructionOffset > targetOffset)
				{
					maxIndex = indexToCheck - 1;
				}
				else
				{
					minIndex = indexToCheck + 1;
				}
			}

			throw new InvalidOperationException($"Failed to locate the target instruction for target offset {targetOffset}. [SourceInstruction: {sourceInstruction}]");
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
		/// Convenience method to read the next two bytes as an Int16 value and to advance the _bytePosition
		/// </summary>
		internal int ReadInt16()
		{
			var shortValue = BitConverter.ToInt16(_ilBytes, _bytePosition);
			_bytePosition += 2;
			return shortValue;
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
