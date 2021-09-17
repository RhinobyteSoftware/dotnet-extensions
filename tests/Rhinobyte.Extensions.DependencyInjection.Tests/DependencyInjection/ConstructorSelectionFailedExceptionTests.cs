using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rhinobyte.Extensions.DependencyInjection.Tests.DependencyInjection
{
	[TestClass]
	public class ConstructorSelectionFailedExceptionTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void Should_be_serializable()
		{
			typeof(ConstructorSelectionFailedException).IsDefined(typeof(System.SerializableAttribute), false).Should().BeTrue();
		}
	}
}
