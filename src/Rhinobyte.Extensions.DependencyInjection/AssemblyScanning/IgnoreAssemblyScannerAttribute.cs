using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false)]
	public class IgnoreAssemblyScannerAttribute : Attribute
	{
	}
}
