using System.Collections.Generic;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers
{
	/// <summary>
	/// Static helper class for fast lookup of <see cref="OpCode"/> instances and related information.
	/// </summary>
	public static class OpCodeHelper
	{
		// TODO: Populate dictionary with keys/values
		public static IReadOnlyDictionary<byte, OpCode> OpCodeLookup = new Dictionary<byte, OpCode>();
	}
}
