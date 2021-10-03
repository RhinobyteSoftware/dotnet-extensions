using System;

namespace Rhinobyte.Extensions.TestTools
{
	/// <summary>
	/// Attribute used to decorate non test methods within a test class. 
	/// </summary>
	/// <remarks>
	/// Used to explicitly mark methods in [TestClass] types that are not intended to have a [TestMethod] decorator.
	/// <para>This way I can use reflection to catch it if a method intended to be a test case has the [TestMethod] attribute left off by accident.</para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class NotATestMethodAttribute : Attribute
	{
	}
}
