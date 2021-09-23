using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage
{
	[TestClass]
	public class OpCodeHelperUnitTests
	{
		[TestMethod]
		public void LocalVariableOpcodeValues_should_match_the_values_found_using_reflection()
		{
			var variableOpcodes = new List<OpCode>();
			foreach (var opcodeField in OpCodeTestHelper.OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				if (opcode.OperandType == OperandType.InlineVar || opcode.OperandType == OperandType.ShortInlineVar)
				{
					variableOpcodes.Add(opcode);
				}
			}

			var localVariableOpcodes = variableOpcodes.Where(opcode => opcode.Name?.Contains("loc") == true).Select(opcode => opcode.Value).ToArray();

			OpCodeHelper.LocalVariableOpcodeValues.Should().BeEquivalentTo(localVariableOpcodes);
		}

		[TestMethod]
		public void LongDescriptionLookup_should_contain_entries_for_all_of_the_opcodes_found_using_reflection()
		{
			foreach (var opcodeField in OpCodeTestHelper.OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				OpCodeHelper.LongDescriptionLookup.ContainsKey(opcode.Value).Should().BeTrue();
			}
		}

		[TestMethod]
		public void NameLookup_should_contain_entries_for_all_of_the_opcodes_found_using_reflection()
		{
			foreach (var opcodeField in OpCodeTestHelper.OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				OpCodeHelper.NameLookup[opcode.Value].Should().Be(opcodeField.Name);
			}
		}

		[TestMethod]
		public void ShortDescriptionLookup_should_contain_entries_for_all_of_the_opcodes_found_using_reflection()
		{
			foreach (var opcodeField in OpCodeTestHelper.OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				OpCodeHelper.ShortDescriptionLookup.ContainsKey(opcode.Value).Should().BeTrue();
			}
		}

		[TestMethod]
		public void SingleByteOpCodeLookup_should_match_the_values_found_using_reflection()
		{
			// Build the array of single byte opcodes using reflection
			var singleByteOpcodes = new OpCode[256];
			foreach (var opcodeField in OpCodeTestHelper.OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				if (opcode.Size == 1)
				{
					singleByteOpcodes[opcode.Value] = opcode;
				}
			}

			OpCodeHelper.SingleByteOpCodeLookup.Should().BeEquivalentTo(singleByteOpcodes);
		}

		[TestMethod]
		public void TwoByteOpCodeLookup_should_match_the_values_found_using_reflection()
		{
			// Build the array of two byte opcodes using reflection
			var twoByteOpcodes = new OpCode[31];
			foreach (var opcodeField in OpCodeTestHelper.OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				if (opcode.Size == 1)
				{
					continue;
				}

				twoByteOpcodes[opcode.Value & 0xff] = opcode;
			}

			OpCodeHelper.TwoByteOpCodeLookup.Should().BeEquivalentTo(twoByteOpcodes);
		}
	}
}
