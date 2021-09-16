using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Attribute used to flag an explicit constructor to use for dependency injection.
	/// <para>See also <seealso cref="ConstructorSelectionType"/></para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
	public class DependencyInjectionConstructorAttribute : Attribute
	{
	}
}
