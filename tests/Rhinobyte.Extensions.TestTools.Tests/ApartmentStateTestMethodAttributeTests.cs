using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.TestTools.Tests;
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
[TestClass]
public class ApartmentStateTestMethodAttributeTests
{
	[TestMethod]
	public void Constructors_do_not_throw_for_invalid_arguments()
	{
		// Attribute constructors should not throw to prevent unexpected exceptions by calls to GetCustomAttribute(s)
		new ApartmentStateTestMethodAttribute(ApartmentState.Unknown).Should().NotBeNull();
		new ApartmentStateTestMethodAttribute(ApartmentState.STA).Should().NotBeNull();
		new ApartmentStateTestMethodAttribute(ApartmentState.MTA).Should().NotBeNull();
		new ApartmentStateTestMethodAttribute(displayName: null, ApartmentState.Unknown).Should().NotBeNull();
		new ApartmentStateTestMethodAttribute(ApartmentState.Unknown, testMethodAttribute: null!).Should().NotBeNull();
	}

	[DataTestMethod]
	[DataRow(ApartmentState.STA)]
	[DataRow(ApartmentState.MTA)]
	public void Execute_gracefully_handles_a_null_array_result(ApartmentState apartmentStateToTest)
	{
		var expectedTestResult = new TestResult { };
		var mockTestMethod = new Mock<ITestMethod>();
		mockTestMethod.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(expectedTestResult);

		var wrappedAttributeThatReturnsNullArrayResult = new StubTestMethodAttribute() { ShouldReturnNullArrayResult = true };
		var systemUnderTest = new ApartmentStateTestMethodAttribute(apartmentStateToTest, wrappedAttributeThatReturnsNullArrayResult);
		var testResults = systemUnderTest.Execute(mockTestMethod.Object);

		if (wrappedAttributeThatReturnsNullArrayResult.ExecutedThreadId != Environment.CurrentManagedThreadId)
		{
			testResults.Should().NotBeNull();
			testResults.Should().BeEmpty();
		}
	}

	[TestMethod]
	public void Execute_invokes_the_test_method_directly_when_not_wrapping_another_attribute()
	{
		var expectedTestResult = new TestResult { };
		var mockTestMethod = new Mock<ITestMethod>();
		mockTestMethod.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(expectedTestResult);

		var systemUnderTest = new ApartmentStateTestMethodAttribute(ApartmentState.MTA);
		var testResults = systemUnderTest.Execute(mockTestMethod.Object);
		testResults.Should().HaveCount(1).And.Contain(expectedTestResult);
	}

	[DataTestMethod]
	[DataRow(ApartmentState.STA)]
	[DataRow(ApartmentState.MTA)]
	public void Execute_runs_the_test_method_with_the_configured_ApartmentState(ApartmentState apartmentStateToTest)
	{
		var expectedTestResult = new TestResult { };
		var mockTestMethod = new Mock<ITestMethod>();
		mockTestMethod.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(expectedTestResult);

		var stubTestMethodAttribute = new StubTestMethodAttribute();
		var systemUnderTest = new ApartmentStateTestMethodAttribute(apartmentStateToTest, stubTestMethodAttribute);

		var testResults = systemUnderTest.Execute(mockTestMethod.Object);
		testResults.Should().HaveCount(1).And.Contain(expectedTestResult);
		stubTestMethodAttribute.Should().Match<StubTestMethodAttribute>(stubValues => stubValues.WasExecuteCalled && stubValues.ExecutedApartmentState == apartmentStateToTest);
	}

	[TestMethod]
	public void Execute_throws_when_configured_with_an_invalid_ApartmentState_value()
	{
		var mockTestMethod = new Mock<ITestMethod>();
		Invoking(() => new ApartmentStateTestMethodAttribute(ApartmentState.Unknown).Execute(mockTestMethod.Object))
			.Should()
			.Throw<InvalidOperationException>()
			.WithMessage($@"{nameof(ApartmentStateTestMethodAttribute)} is configured to use an invalid {nameof(ApartmentState)} value of ""Unknown""*");

		var stubTestMethodAttribute = new StubTestMethodAttribute();
		Invoking(() => new ApartmentStateTestMethodAttribute(ApartmentState.Unknown, stubTestMethodAttribute).Execute(mockTestMethod.Object))
			.Should()
			.Throw<InvalidOperationException>()
			.WithMessage($@"{nameof(ApartmentStateTestMethodAttribute)} is configured to use an invalid {nameof(ApartmentState)} value of ""Unknown""*");
	}

	[DataTestMethod]
	[DataRow(ApartmentState.STA)]
	[DataRow(ApartmentState.MTA)]
	public void Execute_uses_the_current_thread_if_the_configured_ApartmentState_matches1(ApartmentState apartmentStateToTest)
	{
		var expectedTestResult = new TestResult { };
		var mockTestMethod = new Mock<ITestMethod>();
		mockTestMethod.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(expectedTestResult);

		var thisThreadApartmentState = Thread.CurrentThread.GetApartmentState();
		var thisThreadId = Environment.CurrentManagedThreadId;
		var shouldUseCurrentThread = thisThreadApartmentState == apartmentStateToTest;

		var stubTestMethodAttribute = new StubTestMethodAttribute();
		var systemUnderTest = new ApartmentStateTestMethodAttribute(apartmentStateToTest, stubTestMethodAttribute);
		var testResults = systemUnderTest.Execute(mockTestMethod.Object);
		testResults.Should().HaveCount(1).And.Contain(expectedTestResult);

		stubTestMethodAttribute.ExecutedApartmentState.Should().Be(apartmentStateToTest);

		if (shouldUseCurrentThread)
			stubTestMethodAttribute.ExecutedThreadId.Should().Be(thisThreadId);
		else
			stubTestMethodAttribute.ExecutedThreadId.Should().NotBe(thisThreadId);
	}

	[TestMethod]
	public void Execute_uses_the_current_thread_if_the_configured_ApartmentState_matches2()
	{
		var stubTestMethodAttributeForSta = new StubTestMethodAttribute();
		var stubTestMethodAttributeForMta = new StubTestMethodAttribute();
#pragma warning disable IDE0039 // Use local function
		ThreadStart threadStartDelegate = () =>
#pragma warning restore IDE0039 // Use local function
		{
			var expectedTestResult = new TestResult { };
			var mockTestMethod = new Mock<ITestMethod>();
			mockTestMethod.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(expectedTestResult);

			var systemUnderTestSta = new ApartmentStateTestMethodAttribute(ApartmentState.STA, stubTestMethodAttributeForSta);
			var testResults = systemUnderTestSta.Execute(mockTestMethod.Object);
			testResults.Should().HaveCount(1).And.Contain(expectedTestResult);

			var systemUnderTestMta = new ApartmentStateTestMethodAttribute(ApartmentState.MTA, stubTestMethodAttributeForMta);
			testResults = systemUnderTestMta.Execute(mockTestMethod.Object);
			testResults.Should().HaveCount(1).And.Contain(expectedTestResult);
		};

		var thisThreadApartmentState = Thread.CurrentThread.GetApartmentState();
		if (thisThreadApartmentState == ApartmentState.STA)
		{
			// Current thread state is STA, just run the method and verify
			var thisThreadId = Environment.CurrentManagedThreadId;
			threadStartDelegate();
			stubTestMethodAttributeForSta.ExecutedApartmentState.Should().Be(ApartmentState.STA);
			stubTestMethodAttributeForSta.ExecutedThreadId.Should().Be(thisThreadId);

			stubTestMethodAttributeForMta.ExecutedApartmentState.Should().Be(ApartmentState.MTA);
			stubTestMethodAttributeForMta.ExecutedThreadId.Should().NotBe(thisThreadId);
			return;
		}

		// Current thread is MTA, create our own STA thread to test with
		var staThread = new Thread(threadStartDelegate);
		staThread.SetApartmentState(ApartmentState.STA);
		staThread.Start();
		staThread.Join();

		stubTestMethodAttributeForSta.ExecutedApartmentState.Should().Be(ApartmentState.STA);
		stubTestMethodAttributeForSta.ExecutedThreadId.Should().Be(staThread.ManagedThreadId);

		stubTestMethodAttributeForMta.ExecutedApartmentState.Should().Be(ApartmentState.MTA);
		stubTestMethodAttributeForMta.ExecutedThreadId.Should().NotBe(staThread.ManagedThreadId);
	}

	[TestMethod]
	public void Execute_uses_the_current_thread_if_the_configured_ApartmentState_matches3()
	{
		var stubTestMethodAttributeForSta = new StubTestMethodAttribute();
		var stubTestMethodAttributeForMta = new StubTestMethodAttribute();
#pragma warning disable IDE0039 // Use local function
		ThreadStart threadStartDelegate = () =>
#pragma warning restore IDE0039 // Use local function
		{
			var expectedTestResult = new TestResult { };
			var mockTestMethod = new Mock<ITestMethod>();
			mockTestMethod.Setup(x => x.Invoke(It.IsAny<object[]>())).Returns(expectedTestResult);

			var systemUnderTestSta = new ApartmentStateTestMethodAttribute(ApartmentState.STA, stubTestMethodAttributeForSta);
			var testResults = systemUnderTestSta.Execute(mockTestMethod.Object);
			testResults.Should().HaveCount(1).And.Contain(expectedTestResult);

			var systemUnderTestMta = new ApartmentStateTestMethodAttribute(ApartmentState.MTA, stubTestMethodAttributeForMta);
			testResults = systemUnderTestMta.Execute(mockTestMethod.Object);
			testResults.Should().HaveCount(1).And.Contain(expectedTestResult);
		};

		var thisThreadApartmentState = Thread.CurrentThread.GetApartmentState();
		if (thisThreadApartmentState == ApartmentState.MTA)
		{
			// Current thread state is MTA, just run the method and verify
			var thisThreadId = Environment.CurrentManagedThreadId;
			threadStartDelegate();
			stubTestMethodAttributeForSta.ExecutedApartmentState.Should().Be(ApartmentState.STA);
			stubTestMethodAttributeForSta.ExecutedThreadId.Should().NotBe(thisThreadId);

			stubTestMethodAttributeForMta.ExecutedApartmentState.Should().Be(ApartmentState.MTA);
			stubTestMethodAttributeForMta.ExecutedThreadId.Should().Be(thisThreadId);
			return;
		}

		// Current thread is STA, create our own MTA thread to test with
		var mtaThread = new Thread(threadStartDelegate);
		mtaThread.SetApartmentState(ApartmentState.MTA);
		mtaThread.Start();
		mtaThread.Join();

		stubTestMethodAttributeForSta.ExecutedApartmentState.Should().Be(ApartmentState.STA);
		stubTestMethodAttributeForSta.ExecutedThreadId.Should().NotBe(mtaThread.ManagedThreadId);

		stubTestMethodAttributeForMta.ExecutedApartmentState.Should().Be(ApartmentState.MTA);
		stubTestMethodAttributeForMta.ExecutedThreadId.Should().Be(mtaThread.ManagedThreadId);
	}

	[ApartmentStateTestMethod(ApartmentState.STA)]
	public void TestMethod_decorated_with_the_ApartmentStateTestMethodAttribute_should_run_with_STA_apartment_state()
	{
		Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);
	}

	[TestMethod]
	public void TestMethod_decorated_with_normal_TestMethodAttribute_should_run_with_MTA_apartment_state()
	{
		Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.MTA);
	}



	/******     TEST SETUP     ******************************
	 ********************************************************/
	public sealed class StubTestMethodAttribute : TestMethodAttribute
	{
		public ApartmentState ExecutedApartmentState { get; private set; }
		public int ExecutedThreadId { get; private set; }
		public bool ShouldReturnNullArrayResult { get; set; }
		public bool WasExecuteCalled { get; private set; }

		public override TestResult[] Execute(ITestMethod testMethod)
		{
			WasExecuteCalled = true;
			ExecutedApartmentState = Thread.CurrentThread.GetApartmentState();
			ExecutedThreadId = Environment.CurrentManagedThreadId;

			if (ShouldReturnNullArrayResult)
				return null!;

			return new[] { testMethod.Invoke(null) };
		}
	}
}
