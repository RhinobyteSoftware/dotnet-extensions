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
				var opcode = OpCodeHelper.OpCodeLookup[opcodeByte];

				switch (opcode.OperandType)
				{
					// TODO: Based on opcode type read operand bytes, advance the _bytePosition,
					// and construct the instruction instance type

					default:
						throw new NotSupportedException($"{nameof(MethodBodyParser)}.{nameof(ParseInstructions)}() is not supported for an {nameof(OperandType)} value of {opcode.OperandType}");
				}
			}

			// TODO: Once we've read all the instructions, iterate over the instructions list and
			// connect instruction references..

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
	}
}
