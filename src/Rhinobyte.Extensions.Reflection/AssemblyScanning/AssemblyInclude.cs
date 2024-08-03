using System;
using System.Diagnostics;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning;

/// <summary>
/// Representation of an assembly to include during assembly scanning.
/// </summary>
[DebuggerDisplay("Assembly = {AssemblyToInclude},  AreNonExportedTypesIncluded = {AreNonExportedTypesIncluded}")]
public struct AssemblyInclude : IEquatable<AssemblyInclude>
{
	/// <summary>
	/// Construct an AssemblyInclude instance
	/// </summary>
	/// <param name="assemblyToInclude">The assembly to be included in the scan operation</param>
	/// <param name="areNonExportedTypesIncluded">When true non-exported types will be included in the scan</param>
	public AssemblyInclude(
		Assembly assemblyToInclude,
		bool areNonExportedTypesIncluded = false)
	{
		AssemblyToInclude = assemblyToInclude ?? throw new ArgumentNullException(nameof(assemblyToInclude));
		AreNonExportedTypesIncluded = areNonExportedTypesIncluded;
	}

	/// <summary>
	/// When true, non exported types will be included in the type scan.
	/// <para>Defaults to false</para>
	/// </summary>
	public bool AreNonExportedTypesIncluded { get; set; }

	/// <summary>
	/// The <see cref="Assembly"/> to include in the scan operation
	/// </summary>
	public Assembly AssemblyToInclude { get; }

	/// <inheritdoc/>
	public override readonly bool Equals(object? obj)
		=> obj is AssemblyInclude otherAssemblyInclude && otherAssemblyInclude.AssemblyToInclude == this.AssemblyToInclude;

	/// <summary>
	/// Determine if the <paramref name="other"/> <see cref="AssemblyInclude"/> is equivalent to this instance
	/// </summary>
	public readonly bool Equals(AssemblyInclude other) =>
		other.AssemblyToInclude == this.AssemblyToInclude;

	/// <inheritdoc/>
	public override readonly int GetHashCode()
		=> AssemblyToInclude.GetHashCode();

	/// <summary>
	/// Is equal to operator overload
	/// </summary>
	public static bool operator ==(AssemblyInclude left, AssemblyInclude right)
		=> left.Equals(right);

	/// <summary>
	/// Is not equal to operator overload
	/// </summary>
	public static bool operator !=(AssemblyInclude left, AssemblyInclude right)
		=> !(left == right);

}
