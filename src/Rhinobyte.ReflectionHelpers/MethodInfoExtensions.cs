using Rhinobyte.ReflectionHelpers.Instructions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinobyte.ReflectionHelpers
{
	public static class MethodInfoExtensions
	{
		public static IReadOnlyCollection<InstructionBase> ParseInstructions(this MethodBase methodInfo)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			return new MethodBodyParser(methodInfo).ParseInstructions();
		}
	}
}
