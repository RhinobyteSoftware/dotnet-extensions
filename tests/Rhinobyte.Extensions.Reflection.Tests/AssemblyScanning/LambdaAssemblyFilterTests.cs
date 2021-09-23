using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.AssemblyScanning
{
	[TestClass]
	public class LambdaAssemblyFilterTests
	{
		[TestMethod]
		public void Constructor_throws_ArgumentNullException_for_a_null_filter_function()
		{
			Invoking(() => new LambdaAssemblyFilter(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*filter*");
		}
	}
}
