using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
	public class DependencyInjectionConstructorAttribute : Attribute
	{
	}
}
