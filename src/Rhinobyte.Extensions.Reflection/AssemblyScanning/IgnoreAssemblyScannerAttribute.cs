using System;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning
{
	/// <summary>
	/// Ignore attribute that can be used to decorate individual types or to decorate an entire assembly.
	/// <para>When <see cref="AssemblyScanner"/> contains the <see cref="IgnoredAttributeFilter"/> the filter will ignore items decoreated with this attribute.</para>
	/// <para>The <see cref="AssemblyScanner.CreateDefault"/> call will automatically include the <see cref="IgnoredAttributeFilter"/> in both the <see cref="AssemblyScanner.ScannedAssemblyFilters"/> and <see cref="AssemblyScanner.ScannedTypeFilters"/> collections.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
	public sealed class IgnoreAssemblyScannerAttribute : Attribute
	{
	}
}
