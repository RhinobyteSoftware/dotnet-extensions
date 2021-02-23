using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers.UnitTests
{
	[TestClass]
	public class OpCodeHelperUnitTests
	{
		[TestMethod]
		public void LocalVariableOpcodeValues_should_match_the_values_found_using_reflection()
		{
			var opcodeStaticFields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			var variableOpcodes = new List<OpCode>();
			foreach (var opcodeField in opcodeStaticFields)
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
		public void SingleByteOpCodeLookup_should_match_the_values_found_using_reflection()
		{
			// Build the array of single byte opcodes using reflection
			var singleByteOpcodes = new OpCode[256];
			var opcodeStaticFields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var opcodeField in opcodeStaticFields)
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
			var opcodeStaticFields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var opcodeField in opcodeStaticFields)
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
