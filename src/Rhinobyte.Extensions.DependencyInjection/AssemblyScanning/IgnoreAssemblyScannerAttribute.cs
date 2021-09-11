using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
	public class IgnoreAssemblyScannerAttribute : Attribute
	{
	}
}
