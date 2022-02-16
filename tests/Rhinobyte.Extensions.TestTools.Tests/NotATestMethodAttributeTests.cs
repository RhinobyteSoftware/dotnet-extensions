using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rhinobyte.Extensions.TestTools.Tests;

[TestClass]
public class NotATestMethodAttributeTests
{
	[TestMethod]
	public void Constructor_does_not_throw()
	{
		new NotATestMethodAttribute().Should().NotBeNull();
	}
}
