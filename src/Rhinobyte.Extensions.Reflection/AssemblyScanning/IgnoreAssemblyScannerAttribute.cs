using System;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
	public class IgnoreAssemblyScannerAttribute : Attribute
	{
	}
}
