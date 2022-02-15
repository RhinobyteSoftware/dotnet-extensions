using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class ConstructorSelectionFailedExceptionTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void Constructors_behave_as_expected()
	{
		var constructorSelectionFailedException = new ConstructorSelectionFailedException();
		var defaultMessage = new InvalidOperationException().Message;
		constructorSelectionFailedException.Message.Should().Be(defaultMessage);
		constructorSelectionFailedException.InnerException.Should().BeNull();

		constructorSelectionFailedException = new ConstructorSelectionFailedException("Some message");
		constructorSelectionFailedException.Message.Should().Be("Some message");
		constructorSelectionFailedException.InnerException.Should().BeNull();

		constructorSelectionFailedException = new ConstructorSelectionFailedException("Some message2", new ArgumentException("Test"));
		constructorSelectionFailedException.Message.Should().Be("Some message2");
		constructorSelectionFailedException.InnerException.Should().NotBeNull().And.BeOfType<ArgumentException>();
	}

	[TestMethod]
	public void Should_be_serializable()
	{
		typeof(ConstructorSelectionFailedException).IsDefined(typeof(System.SerializableAttribute), false).Should().BeTrue();
	}
}
