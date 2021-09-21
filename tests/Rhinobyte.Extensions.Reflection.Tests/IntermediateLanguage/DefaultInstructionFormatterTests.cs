using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage
{
	[TestClass]
	public class DefaultInstructionFormatterTests
	{
		public static OpCode GetFakeOpcodeThatDoesntExist()
		{
			// Setup a fake opcode with an unknown value so we can test ToString() / opcode lookup behavior for it
			var opCodeConstructor = typeof(OpCode).GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Single();

			var flags = ((int)OperandType.InlineNone) |
				((int)FlowControl.Meta << 5) |
				((int)OpCodeType.Prefix << 9) |
				((int)StackBehaviour.Pop0 << 12) |
				((int)StackBehaviour.Push0 << 12) |
				(2 << 22) |
				(0 << 28);

			var newOpCodeThatDoesntExistYet = (OpCode)opCodeConstructor.Invoke(new object[] { 0xfe1f, flags });
			newOpCodeThatDoesntExistYet.Value.Should().Be(unchecked((short)0xfe1f));
			return newOpCodeThatDoesntExistYet;
		}

		[TestMethod]
		public void DescribeInstruction_returns_the_expected_result()
		{
			var defaultInstructionFormatter = new DefaultInstructionFormatter();
			defaultInstructionFormatter.DescribeInstruction(null!).Should().Be(string.Empty);

			InstructionBase fakeInstruction = new SimpleInstruction(0, 0, OpCodes.Nop);
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());

			fakeInstruction = new LocalVariableInstruction(0, 0, OpCodes.Ldloc, null!);
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());

			fakeInstruction = new MethodReferenceInstruction(0, 0, OpCodes.Call, null!);
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());

			fakeInstruction = new ParameterReferenceInstruction(0, 0, OpCodes.Ldarg_1, 0, null!);
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());

			var switchInstruction = new SwitchInstruction(0, 0, OpCodes.Switch, Array.Empty<int>());
			defaultInstructionFormatter.DescribeInstruction(switchInstruction).Should().Be(switchInstruction.ToString());

			switchInstruction = new SwitchInstruction(0, 0, OpCodes.Switch, new int[] { switchInstruction.Size + 4 });
			switchInstruction.TargetInstructions = new List<InstructionBase>() { fakeInstruction };
			defaultInstructionFormatter.DescribeInstruction(switchInstruction).Should().Be(switchInstruction.ToString());

			fakeInstruction = new TypeReferenceInstruction(0, 0, OpCodes.Refanytype, null!);
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());

			fakeInstruction = new TypeReferenceInstruction(0, 0, OpCodes.Refanytype, typeof(DefaultInstructionFormatterTests));
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());

			fakeInstruction = new UnknownMemberReferenceInstruction(0, 0, OpCodes.Ldflda, null);
			defaultInstructionFormatter.DescribeInstruction(fakeInstruction).Should().Be(fakeInstruction.ToString());
		}

		[TestMethod]
		public void DescribeInstructions_returns_the_expected_result()
		{
			var opcodeStaticFields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			var opcodeLookupByOperandType = new Dictionary<OperandType, List<OpCode>>();
			foreach (var opcodeField in opcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				if (!opcodeLookupByOperandType.TryGetValue(opcode.OperandType, out var opcodesList))
				{
					opcodesList = new List<OpCode>();
					opcodeLookupByOperandType.Add(opcode.OperandType, opcodesList);
				}

				opcodesList.Add(opcode);
			}

			var fakeInstructions = new List<InstructionBase>();

			var offset = 0;
			fakeInstructions.Add(new BranchTargetInstruction(0, offset, opcodeLookupByOperandType[OperandType.ShortInlineBrTarget].First(), 1));
			offset += fakeInstructions.Last().Size;

			var branchTargetInstruction2 = new BranchTargetInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.ShortInlineBrTarget].First(), 1);
			fakeInstructions.Add(branchTargetInstruction2);
			offset += branchTargetInstruction2.Size;

			var byteInstruction = new ByteInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.ShortInlineI].First(), 12);
			fakeInstructions.Add(byteInstruction);
			offset += byteInstruction.Size;
			branchTargetInstruction2.TargetInstruction = byteInstruction;

			fakeInstructions.Add(new FieldReferenceInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.InlineField].First(), null));
			offset += fakeInstructions.Last().Size;

			var allFields = typeof(ExampleMethods).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			fakeInstructions.Add(new FieldReferenceInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.InlineField].First(), allFields.First()));
			offset += fakeInstructions.Last().Size;

			var fakeOpCode = GetFakeOpcodeThatDoesntExist();
			OpCodeHelper.ShortDescriptionLookup.ContainsKey(fakeOpCode.Value).Should().BeFalse();
			fakeInstructions.Add(new SimpleInstruction(fakeInstructions.Count, offset, fakeOpCode));
			offset += fakeInstructions.Last().Size;

			fakeInstructions.Add(new SignatureInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.InlineSig].First(), new byte[0]));
			offset += fakeInstructions.Last().Size;

			fakeInstructions.Add(new SwitchInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.InlineSwitch].First(), Array.Empty<int>()));
			offset += fakeInstructions.Last().Size;

			fakeInstructions.Add(new UnknownMemberReferenceInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.InlineTok].First(), null));
			offset += fakeInstructions.Last().Size;

			var method = typeof(DefaultInstructionFormatterTests).GetMethod(nameof(DescribeInstructions_returns_the_expected_result), BindingFlags.Public | BindingFlags.Instance);
			fakeInstructions.Add(new UnknownMemberReferenceInstruction(fakeInstructions.Count, offset, opcodeLookupByOperandType[OperandType.InlineTok].First(), method));
			offset += fakeInstructions.Last().Size;

			var description = new DefaultInstructionFormatter().DescribeInstructions(fakeInstructions);
			description.Should().Be(
@"(0) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: null]
(1) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 2]
(2) LOAD INT VALUE (Int8)  [Byte Value: 12]
(3) LOAD FIELD  [FieldReference: ]
(4) LOAD FIELD  [FieldReference: <LocalIntegerProperty>k__BackingField]
(5) ?UnknownOpCode?
(6) CALL METHOD  [SignatureBlob: 0 bytes]
(7) SWITCH  [TargetInstructions: null]  [TargetOffsets: ]
(8) LOAD TOKEN  [MemberReference: null]
(9) LOAD TOKEN  [MemberReference: Method  DescribeInstructions_returns_the_expected_result (DefaultInstructionFormatterTests)]");
		}
	}
}
