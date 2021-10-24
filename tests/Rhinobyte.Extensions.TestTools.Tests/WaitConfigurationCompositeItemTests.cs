using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rhinobyte.Extensions.TestTools.Tests
{
	[TestClass]
	public class WaitConfigurationCompositeItemTests
	{
		[TestMethod]
		public void AssignMissingValues_returns_false_for_a_null_itemToCopyFrom_argument()
		{
			new WaitConfigurationCompositeItem().AssignMissingValues(itemToCopyFrom: null!, "SomeKey").Should().BeFalse();
		}
	}
}
