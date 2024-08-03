using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;

namespace Rhinobyte.Extensions.TestTools;

/// <summary>
/// <see cref="TestMethodAttribute"/> implementation that will check the current thread's apartment state. If the current apartment state
/// does not match the configured <see cref="TestApartmentState"/> the test method will be executed on a new thread with the specified
/// apartment state.
/// </summary>
#pragma warning disable CA1813 // Avoid unsealed attributes -  Reason: Subclassing allowed to support extending the test attribute behavior
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class ApartmentStateTestMethodAttribute : TestMethodAttribute
#pragma warning restore CA1813 // Avoid unsealed attributes
{
	/// <summary>
	/// Construct a new instance of the attribute with the specified thread <see cref="ApartmentState"/> to use.
	/// </summary>
	public ApartmentStateTestMethodAttribute(ApartmentState testApartmentState)
	{
		TestApartmentState = testApartmentState;
	}

	/// <summary>
	/// Construct a new instance of the attribute with the specified <paramref name="displayName"/> and thread <see cref="ApartmentState"/> to use.
	/// </summary>
	public ApartmentStateTestMethodAttribute(string? displayName, ApartmentState testApartmentState)
		: base(displayName)
	{
		TestApartmentState = testApartmentState;
	}

	/// <summary>
	/// Wrap the provided <paramref name="testMethodAttribute"/> in a new instance of this attribute with the specified thread <see cref="ApartmentState"/> to use.
	/// </summary>
	/// <remarks>
	/// This constructor overload can be used by subclasses of <see cref="TestClassAttribute"/> to wrap other test method attributes.
	/// <para>For example: <see cref="ApartmentStateTestClassAttribute"/></para>
	/// </remarks>
	public ApartmentStateTestMethodAttribute(ApartmentState testApartmentState, TestMethodAttribute? testMethodAttribute)
		: this(testMethodAttribute?.DisplayName, testApartmentState)
	{
		TestMethodAttribute = testMethodAttribute;
	}

	/// <summary>
	/// The thread <see cref="ApartmentState"/> that the test method should run under.
	/// </summary>
	public ApartmentState TestApartmentState { get; }

	/// <summary>
	/// A <see cref="TestMethodAttribute"/> that is being wrapped by this attribute. When this property is non-null
	/// the <see cref="Execute(ITestMethod)"/> implementation will call through to the wrapped attribute's execute method
	/// in lieu of invoking the test method directly.
	/// </summary>
	public TestMethodAttribute? TestMethodAttribute { get; }


	/// <summary>
	/// Execute the <paramref name="testMethod"/>. If the current thread's apartment state does not match
	/// the configured <see cref="TestApartmentState"/> then a new thread is created to invoke the test method
	/// with the specific apartment state.
	/// </summary>
	/// <param name="testMethod">The test method to execute</param>
	/// <returns>The <see cref="TestResult"/> returned by the test method execution</returns>
#pragma warning disable CA1062 // Validate arguments of public methods
	public override TestResult[] Execute(ITestMethod testMethod)
	{
		if (TestApartmentState == ApartmentState.Unknown)
			throw new InvalidOperationException($@"{nameof(ApartmentStateTestMethodAttribute)} is configured to use an invalid {nameof(ApartmentState)} value of ""{TestApartmentState}""{Environment.NewLine}  TestClass: {testMethod.TestClassName}{Environment.NewLine}  TestMethod: {testMethod.TestMethodName}");

		if (Thread.CurrentThread.GetApartmentState() == TestApartmentState)
			return Invoke(testMethod);

		// TODO: Find a clean way to pool / re-use STA threads without requiring a synchronization context ?

		TestResult[]? result = null;
		var thread = new Thread(() => result = Invoke(testMethod));
		thread.SetApartmentState(TestApartmentState);
		thread.Start();
		thread.Join();
#pragma warning disable CA1508 // Avoid dead conditional code  -
		return result ?? [];
#pragma warning restore CA1508 // Avoid dead conditional code
	}
#pragma warning restore CA1062 // Validate arguments of public methods

	private TestResult[] Invoke(ITestMethod testMethod)
	{
		if (TestMethodAttribute != null)
			return TestMethodAttribute.Execute(testMethod);

		return [testMethod.Invoke(null)];
	}
}
