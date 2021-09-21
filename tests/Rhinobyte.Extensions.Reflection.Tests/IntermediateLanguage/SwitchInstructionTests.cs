using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System;
using System.Reflection.Emit;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage
{
	[TestClass]
	public class SwitchInstructionTests
	{
		[TestMethod]
		public void Constructor_should_throw_argument_null_exception_for_null_target_offsets()
		{
			Invoking(() => new SwitchInstruction(0, 0, OpCodes.Nop, null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage(@"Value cannot be null*targetOffsets*");
		}
	}
}
