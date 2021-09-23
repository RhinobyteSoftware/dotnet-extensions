using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public static class OpCodeTestHelper
	{
		public static readonly FieldInfo[] OpcodeStaticFields;
		public static readonly IReadOnlyDictionary<OperandType, List<OpCode>> OpcodeLookupByOperandType;

		static OpCodeTestHelper()
		{
			OpcodeStaticFields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			var lookupDictionary = new Dictionary<OperandType, List<OpCode>>();
			foreach (var opcodeField in OpcodeStaticFields)
			{
				var opcode = (OpCode)opcodeField.GetValue(null)!;
				if (!lookupDictionary.TryGetValue(opcode.OperandType, out var opcodesList))
				{
					opcodesList = new List<OpCode>();
					lookupDictionary.Add(opcode.OperandType, opcodesList);
				}

				opcodesList.Add(opcode);
			}

			OpcodeLookupByOperandType = lookupDictionary;
		}


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
	}
}
