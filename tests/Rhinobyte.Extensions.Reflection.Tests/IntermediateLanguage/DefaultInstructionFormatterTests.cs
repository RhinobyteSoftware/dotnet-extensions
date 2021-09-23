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
			var fakeInstructions = new List<InstructionBase>();

			var offset = 0;
			fakeInstructions.Add(new BranchTargetInstruction(0, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.ShortInlineBrTarget].First(), 1));
			offset += fakeInstructions.Last().Size;

			var branchTargetInstruction2 = new BranchTargetInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.ShortInlineBrTarget].First(), 1);
			fakeInstructions.Add(branchTargetInstruction2);
			offset += branchTargetInstruction2.Size;

			var byteInstruction = new ByteInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.ShortInlineI].First(), 12);
			fakeInstructions.Add(byteInstruction);
			offset += byteInstruction.Size;
			branchTargetInstruction2.TargetInstruction = byteInstruction;

			fakeInstructions.Add(new FieldReferenceInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.InlineField].First(), null));
			offset += fakeInstructions.Last().Size;

			var allFields = typeof(ExampleMethods).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			fakeInstructions.Add(new FieldReferenceInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.InlineField].First(), allFields.First()));
			offset += fakeInstructions.Last().Size;

			var fakeOpCode = OpCodeTestHelper.GetFakeOpcodeThatDoesntExist();
			OpCodeHelper.ShortDescriptionLookup.ContainsKey(fakeOpCode.Value).Should().BeFalse();
			fakeInstructions.Add(new SimpleInstruction(fakeInstructions.Count, offset, fakeOpCode));
			offset += fakeInstructions.Last().Size;

			fakeInstructions.Add(new SignatureInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.InlineSig].First(), new byte[0]));
			offset += fakeInstructions.Last().Size;

			fakeInstructions.Add(new SwitchInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.InlineSwitch].First(), Array.Empty<int>()));
			offset += fakeInstructions.Last().Size;

			fakeInstructions.Add(new UnknownMemberReferenceInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.InlineTok].First(), null));
			offset += fakeInstructions.Last().Size;

			var method = typeof(DefaultInstructionFormatterTests).GetMethod(nameof(DescribeInstructions_returns_the_expected_result), BindingFlags.Public | BindingFlags.Instance);
			fakeInstructions.Add(new UnknownMemberReferenceInstruction(fakeInstructions.Count, offset, OpCodeTestHelper.OpcodeLookupByOperandType[OperandType.InlineTok].First(), method));
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
