using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Parser class that processes the IL bytes from a <see cref="MethodBody"/>.
	/// <para>
	/// The parser can be used to detect matches for <see cref="MemberInfo"/> references within the IL bytes or to return a collection of
	/// strongly typed <see cref="InstructionBase"/> objects that represent the method's IL instructions.
	/// </para>
	/// </summary>
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

			_module = method.Module ?? throw new ArgumentException($"{nameof(method)}.{nameof(method.Module)} property is null");

			_declaringTypeGenericArguments = method.DeclaringType?.GetGenericArguments();
			_isStaticMethod = method.IsStatic;
			_localVariables = methodBody.LocalVariables;
			_methodGenericArguments = method.GetGenericArguments();
			_parameters = method.GetParameters();
		}

		/// <summary>
		/// Returns true if a reference to each of the <paramref name="memberReferencesToLookFor"/> members is found. Returns false otherwise.
		/// </summary>
		internal bool ContainsReferencesToAll(
			IEnumerable<MemberInfo> memberReferencesToLookFor,
			bool matchAgainstBaseClassMembers,
			bool matchAgainstDeclaringTypeMember)
		{
			var referencesMatchInfo = new List<MemberReferenceMatchInfo>();
			foreach (var memberInfo in memberReferencesToLookFor)
				referencesMatchInfo.Add(new MemberReferenceMatchInfo(memberInfo, matchAgainstBaseClassMembers, matchAgainstDeclaringTypeMember));

			return ContainsReferencesToAll(referencesMatchInfo);
		}

		/// <summary>
		/// Returns true if a reference to each of the <paramref name="memberReferencesToLookFor"/> members is found. Returns false otherwise.
		/// </summary>
		internal bool ContainsReferencesToAll(IEnumerable<IMemberReferenceMatchInfo> memberReferencesToLookFor)
		{
			_bytePosition = 0;

			var referencesNotFoundYet = new List<IMemberReferenceMatchInfo>(memberReferencesToLookFor);

			while (_bytePosition < _ilBytes.Length)
			{
				var opcodeByte = ReadByte();
				var currentOpcode = opcodeByte != 254 // OpCodes.Prefix1
					? OpCodeHelper.SingleByteOpCodeLookup[opcodeByte]
					: OpCodeHelper.TwoByteOpCodeLookup[ReadByte()];

#pragma warning disable IDE0010 // Add missing cases.  Reason: Intentionally want the default case to handle most operand types for this method
				switch (currentOpcode.OperandType)
#pragma warning restore IDE0010 // Add missing cases
				{
					case OperandType.InlineField:
					case OperandType.InlineMethod:
					case OperandType.InlineType:
					case OperandType.InlineTok:
						var instructionMemberReference = _module.ResolveMember(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments);

						var nextReferenceIndex = 0;
						while (nextReferenceIndex < referencesNotFoundYet.Count)
						{
							var nextReferenceToLookFor = referencesNotFoundYet[nextReferenceIndex];
							if (nextReferenceToLookFor.DoesInstructionReferenceMatch(instructionMemberReference))
								referencesNotFoundYet.RemoveAt(nextReferenceIndex);
							else
								++nextReferenceIndex;
						}

						if (referencesNotFoundYet.Count == 0)
							return true;

						break;

					case OperandType.InlineSwitch:
						// For inline switch we have to read the actual size of the targets array for the jump table to know
						// how many operand bytes to skip
						var numberOfTargets = ReadInt32();
						_bytePosition += (4 * numberOfTargets);
						break;

					default:
						_bytePosition += OpCodeHelper.GetOperandSize(currentOpcode.OperandType);
						break;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if a reference to the <paramref name="memberReferenceToLookFor"/> is found. Returns false otherwise.
		/// </summary>
		internal bool ContainsReferenceTo(
			MemberInfo memberReferenceToLookFor,
			bool matchAgainstBaseClassMembers,
			bool matchAgainstDeclaringTypeMember)
			=> ContainsReferenceTo(new MemberReferenceMatchInfo(memberReferenceToLookFor, matchAgainstBaseClassMembers, matchAgainstDeclaringTypeMember));

		/// <summary>
		/// Returns true if a reference to the <paramref name="memberReferenceToLookFor"/> is found. Returns false otherwise.
		/// </summary>
		internal bool ContainsReferenceTo(IMemberReferenceMatchInfo memberReferenceToLookFor)
		{
			_bytePosition = 0;

			while (_bytePosition < _ilBytes.Length)
			{
				var opcodeByte = ReadByte();
				var currentOpcode = opcodeByte != 254 // OpCodes.Prefix1
					? OpCodeHelper.SingleByteOpCodeLookup[opcodeByte]
					: OpCodeHelper.TwoByteOpCodeLookup[ReadByte()];

#pragma warning disable IDE0010 // Add missing cases.  Reason: Intentionally want the default case to handle most operand types for this method
				switch (currentOpcode.OperandType)
#pragma warning restore IDE0010 // Add missing cases
				{
					case OperandType.InlineField:
					case OperandType.InlineMethod:
					case OperandType.InlineType:
					case OperandType.InlineTok:
						var instructionMemberReference = _module.ResolveMember(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments);
						if (memberReferenceToLookFor.DoesInstructionReferenceMatch(instructionMemberReference))
						{
							return true;
						}
						break;

					case OperandType.InlineSwitch:
						// For inline switch we have to read the actual size of the targets array for the jump table to know
						// how many operand bytes to skip
						var numberOfTargets = ReadInt32();
						_bytePosition += (4 * numberOfTargets);
						break;

					default:
						_bytePosition += OpCodeHelper.GetOperandSize(currentOpcode.OperandType);
						break;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if a reference to any of the <paramref name="memberReferencesToLookFor"/> is found. Returns false otherwise.
		/// </summary>
		internal bool ContainsReferenceToAny(
			IEnumerable<MemberInfo> memberReferencesToLookFor,
			bool matchAgainstBaseClassMembers,
			bool matchAgainstDeclaringTypeMember)
		{
			var referencesMatchInfo = new List<MemberReferenceMatchInfo>();
			foreach (var memberInfo in memberReferencesToLookFor)
				referencesMatchInfo.Add(new MemberReferenceMatchInfo(memberInfo, matchAgainstBaseClassMembers, matchAgainstDeclaringTypeMember));

			return ContainsReferenceToAny(referencesMatchInfo);
		}

		/// <summary>
		/// Returns true if a reference to any of the <paramref name="memberReferencesToLookFor"/> is found. Returns false otherwise.
		/// </summary>
		internal bool ContainsReferenceToAny(IEnumerable<IMemberReferenceMatchInfo> memberReferencesToLookFor)
		{
			_bytePosition = 0;

			while (_bytePosition < _ilBytes.Length)
			{
				var opcodeByte = ReadByte();
				var currentOpcode = opcodeByte != 254 // OpCodes.Prefix1
					? OpCodeHelper.SingleByteOpCodeLookup[opcodeByte]
					: OpCodeHelper.TwoByteOpCodeLookup[ReadByte()];

#pragma warning disable IDE0010 // Add missing cases.  Reason: Intentionally want the default case to handle most operand types for this method
				switch (currentOpcode.OperandType)
#pragma warning restore IDE0010 // Add missing cases
				{
					case OperandType.InlineField:
					case OperandType.InlineMethod:
					case OperandType.InlineType:
					case OperandType.InlineTok:
						var instructionMemberReference = _module.ResolveMember(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments);
						foreach (var referenceToLookFor in memberReferencesToLookFor)
						{
							if (referenceToLookFor.DoesInstructionReferenceMatch(instructionMemberReference))
								return true;
						}
						break;

					case OperandType.InlineSwitch:
						// For inline switch we have to read the actual size of the targets array for the jump table to know
						// how many operand bytes to skip
						var numberOfTargets = ReadInt32();
						_bytePosition += (4 * numberOfTargets);
						break;

					default:
						_bytePosition += OpCodeHelper.GetOperandSize(currentOpcode.OperandType);
						break;
				}
			}

			return false;
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

		internal IReadOnlyCollection<InstructionBase> ParseInstructions()
		{
			_bytePosition = 0;
			var instructions = new List<InstructionBase>();
			var branchInstructionsNeedingLinked = new List<InstructionBase>();

			InstructionBase? previousInstruction = null;

			var instructionIndex = 0;
			var instructionOffset = _bytePosition;
			while (_bytePosition < _ilBytes.Length)
			{
				instructionOffset = _bytePosition;
				var opcodeByte = ReadByte();
				var currentOpcode = opcodeByte != 254 // OpCodes.Prefix1
					? OpCodeHelper.SingleByteOpCodeLookup[opcodeByte]
					: OpCodeHelper.TwoByteOpCodeLookup[ReadByte()];


				InstructionBase currentInstruction;
				switch (currentOpcode.OperandType)
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
					{
						// IMPORTANT: Call ReadSignedByte() / ReadInt32() first to advance the _bytePosition before appending _bytePosition for the calculated target offset
						var targetOffset = currentOpcode.OperandType == OperandType.ShortInlineBrTarget
							? ReadSignedByte() + _bytePosition
							: ReadInt32() + _bytePosition;

						currentInstruction = new BranchTargetInstruction(instructionIndex, instructionOffset, currentOpcode, targetOffset);

						if (targetOffset < instructionOffset)
						{
							((BranchTargetInstruction)currentInstruction).TargetInstruction = FindInstructionByOffset(instructionOffset, instructions, currentInstruction, targetOffset);
							break;
						}

						// Add to the "branchInstructionsNeedingLinked" collection so we can set the target instruction later
						branchInstructionsNeedingLinked.Add(currentInstruction);
						break;
					}

					case OperandType.InlineField:
						currentInstruction = new FieldReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, _module.ResolveField(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments));
						break;

					case OperandType.InlineI:
						currentInstruction = new Int32Instruction(instructionIndex, instructionOffset, currentOpcode, ReadInt32());
						break;

					case OperandType.InlineI8:
						currentInstruction = new Int64Instruction(instructionIndex, instructionOffset, currentOpcode, ReadInt64());
						break;

					case OperandType.InlineMethod:
						currentInstruction = new MethodReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, _module.ResolveMethod(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments));
						break;

					case OperandType.InlineNone:
						switch (currentOpcode.Value)
						{
							case 2: // Ldarg_0
									// For non-static methods Ldarg_0 refers to the 'this' keyword, Ldarg_1 == _parameters[0], and so on ...
								currentInstruction = !_isStaticMethod
									? new ThisKeywordInstruction(instructionIndex, instructionOffset, currentOpcode, _method)
									: new ParameterReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, 0, _parameters[0]);
								break;

							case 3: // Ldarg_1
							{
								var parameterIndex = _isStaticMethod ? 1 : 0;
								currentInstruction = new ParameterReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, parameterIndex, _parameters[parameterIndex]);
								break;
							}

							case 4: // Ldarg_2
							{
								var parameterIndex = _isStaticMethod ? 2 : 1;
								currentInstruction = new ParameterReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, parameterIndex, _parameters[parameterIndex]);
								break;
							}

							case 5: // Ldarg_3
							{
								var parameterIndex = _isStaticMethod ? 3 : 2;
								currentInstruction = new ParameterReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, parameterIndex, _parameters[parameterIndex]);
								break;
							}

							case 6: // Ldloc_0
							case 10: // Stloc_0
								currentInstruction = new LocalVariableInstruction(instructionIndex, instructionOffset, currentOpcode, _localVariables[0]);
								break;

							case 7: // Ldloc_1
							case 11: // Stloc_1
								currentInstruction = new LocalVariableInstruction(instructionIndex, instructionOffset, currentOpcode, _localVariables[1]);
								break;

							case 8: // Ldloc_2
							case 12: // Stloc_2
								currentInstruction = new LocalVariableInstruction(instructionIndex, instructionOffset, currentOpcode, _localVariables[2]);
								break;

							case 9: // Ldloc_3
							case 13: // Stloc_3
								currentInstruction = new LocalVariableInstruction(instructionIndex, instructionOffset, currentOpcode, _localVariables[3]);
								break;

							default:
								currentInstruction = new SimpleInstruction(instructionIndex, instructionOffset, currentOpcode);
								break;
						}
						break;

					case OperandType.InlineR:
						currentInstruction = new DoubleInstruction(instructionIndex, instructionOffset, currentOpcode, ReadDouble());
						break;

					case OperandType.InlineSig:
						currentInstruction = new SignatureInstruction(instructionIndex, instructionOffset, currentOpcode, _module.ResolveSignature(ReadInt32()));
						break;

					case OperandType.InlineString:
						currentInstruction = new StringInstruction(instructionIndex, instructionOffset, currentOpcode, _module.ResolveString(ReadInt32()));
						break;

					case OperandType.InlineSwitch:
						var numberOfTargets = ReadInt32();
						var baseOffset = _bytePosition + (4 * numberOfTargets);
						var targetOffsets = new int[numberOfTargets];
						for (var targetIndex = 0; targetIndex < numberOfTargets; ++targetIndex)
						{
							targetOffsets[targetIndex] = ReadInt32() + baseOffset;
						}

						currentInstruction = new SwitchInstruction(instructionIndex, instructionOffset, currentOpcode, targetOffsets);

						// Add to the "branchInstructionsNeedingLinked" collection so we can set the target instructions later
						branchInstructionsNeedingLinked.Add(currentInstruction);
						break;

					case OperandType.InlineTok:
						var memberReference = _module.ResolveMember(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments);
						switch (memberReference)
						{
							case FieldInfo fieldReference:
								currentInstruction = new FieldReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, fieldReference);
								break;

							case MethodBase methodReference:
								currentInstruction = new MethodReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, methodReference);
								break;

							case Type typeReference:
								currentInstruction = new TypeReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, typeReference);
								break;

							default:
								// Not sure if this is possible or if I should make this case throw...
								currentInstruction = new UnknownMemberReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, memberReference);
								break;
						}
						break;

					case OperandType.InlineType:
						currentInstruction = new TypeReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, _module.ResolveType(ReadInt32(), _declaringTypeGenericArguments, _methodGenericArguments));
						break;

					case OperandType.InlineVar:
					case OperandType.ShortInlineVar:
					{
						var variableIndex = currentOpcode.OperandType == OperandType.ShortInlineVar
							? ReadByte()
							: ReadInt16();

						if (OpCodeHelper.LocalVariableOpcodeValues.Contains(currentOpcode.Value))
						{
							currentInstruction = new LocalVariableInstruction(instructionIndex, instructionOffset, currentOpcode, _localVariables[variableIndex]);
							break;
						}

						if (!_isStaticMethod)
						{
							if (variableIndex == 0)
							{
								currentInstruction = new ThisKeywordInstruction(instructionIndex, instructionOffset, currentOpcode, _method);
								break;
							}

							variableIndex -= 1; // Subtract one for non static methods to account for [0] being the this keyword
						}

						currentInstruction = new ParameterReferenceInstruction(instructionIndex, instructionOffset, currentOpcode, variableIndex, _parameters[variableIndex]);
						break;
					}

					case OperandType.ShortInlineI:
						if (currentOpcode == OpCodes.Ldc_I4_S)
						{
							currentInstruction = new SignedByteInstruction(instructionIndex, instructionOffset, currentOpcode, (sbyte)ReadByte());
							break;
						}

						currentInstruction = new ByteInstruction(instructionIndex, instructionOffset, currentOpcode, ReadByte());
						break;

					case OperandType.ShortInlineR:
						currentInstruction = new FloatInstruction(instructionIndex, instructionOffset, currentOpcode, ReadSingle());
						break;

#pragma warning disable CS0618 // Type or member is obsolete. Reason: Ensuring all switch cases are handled.
					case OperandType.InlinePhi:
#pragma warning restore CS0618 // Type or member is obsolete
					default:
						throw new NotSupportedException($"{nameof(MethodBodyParser)}.{nameof(ParseInstructions)}() is not supported for an {nameof(OperandType)} value of {currentOpcode.OperandType}");
				}

				if (previousInstruction != null)
				{
					previousInstruction.NextInstruction = currentInstruction;
				}

				currentInstruction.PreviousInstruction = previousInstruction;

				instructions.Add(currentInstruction);
				previousInstruction = currentInstruction;

				++instructionIndex;
			}

			foreach (var instructionToUpdate in branchInstructionsNeedingLinked)
			{
#pragma warning disable IDE0010 // Add missing cases.  Reason: Intentionally limited to the specified cases
				switch (instructionToUpdate.OpCode.OperandType)
#pragma warning restore IDE0010 // Add missing cases
				{
					case OperandType.InlineBrTarget:
					case OperandType.ShortInlineBrTarget:
						var branchTargetInstruction = (BranchTargetInstruction)instructionToUpdate;
						branchTargetInstruction.TargetInstruction = FindInstructionByOffset(instructionOffset, instructions, branchTargetInstruction, branchTargetInstruction.TargetOffset);
						break;

					case OperandType.InlineSwitch:
						var switchInstruction = (SwitchInstruction)instructionToUpdate;
						var targetInstructions = new List<InstructionBase>();
						foreach (var targetOffset in switchInstruction.TargetOffsets)
						{
							targetInstructions.Add(FindInstructionByOffset(instructionOffset, instructions, switchInstruction, targetOffset));
						}
						switchInstruction.TargetInstructions = targetInstructions;
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

		internal sbyte ReadSignedByte()
			=> (sbyte)ReadByte();

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
