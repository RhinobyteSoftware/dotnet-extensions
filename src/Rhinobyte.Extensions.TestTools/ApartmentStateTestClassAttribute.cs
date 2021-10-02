using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;

namespace Rhinobyte.Extensions.TestTools
{
	/// <summary>
	/// <see cref="TestClassAttribute"/> implementation that will ensure any test methods in the class use the specified thread <see cref="ApartmentState"/>.
	/// </summary>
#pragma warning disable CA1813 // Avoid unsealed attributes  - Reason: Subclassing allowed so other test attributes can extend the behavior.
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public class ApartmentStateTestClassAttribute : TestClassAttribute
#pragma warning restore CA1813 // Avoid unsealed attributes
	{
		/// <summary>
		/// Attribute constructor with a required thread <paramref name="testApartmentState"/> parameter.
		/// </summary>
		/// <param name="testApartmentState">The thread <see cref="ApartmentState"/> that the test methods should execute under</param>
		public ApartmentStateTestClassAttribute(ApartmentState testApartmentState)
		{
			TestApartmentState = testApartmentState;
		}

		/// <summary>
		/// The thread <see cref="ApartmentState"/> that test methods in this class should run under.
		/// </summary>
		public ApartmentState TestApartmentState { get; }

		/// <summary>
		/// Implementation of <see cref="TestClassAttribute.GetTestMethodAttribute(TestMethodAttribute)"/> which wraps any non
		/// <see cref="ApartmentStateTestMethodAttribute"/> instances in a new apartment state attribute using the configured
		/// <see cref="TestApartmentState" />.
		/// </summary>
		/// <param name="testMethodAttribute">The <see cref="TestMethodAttribute"/> to wrap if it is not already an instance of <see cref="ApartmentStateTestMethodAttribute"/></param>
		public override TestMethodAttribute GetTestMethodAttribute(TestMethodAttribute testMethodAttribute)
		{
			if (testMethodAttribute is ApartmentStateTestMethodAttribute)
				return testMethodAttribute;

			return new ApartmentStateTestMethodAttribute(TestApartmentState, base.GetTestMethodAttribute(testMethodAttribute));
		}
	}
}
