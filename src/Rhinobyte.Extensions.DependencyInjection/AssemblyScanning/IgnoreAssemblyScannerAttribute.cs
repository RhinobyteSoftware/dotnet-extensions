using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
	public class IgnoreAssemblyScannerAttribute : Attribute
	{
	}
}
