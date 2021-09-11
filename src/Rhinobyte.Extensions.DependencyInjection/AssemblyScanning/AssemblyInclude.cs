using System;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public struct AssemblyInclude
	{
		public AssemblyInclude(
			Assembly assemblyToInclude,
			bool areNonExportedTypesIncluded = false)
		{
			AssemblyToInclude = assemblyToInclude ?? throw new ArgumentNullException(nameof(assemblyToInclude));
			AreNonExportedTypesIncluded = areNonExportedTypesIncluded;
		}

		public bool AreNonExportedTypesIncluded { get; set; }
		public Assembly AssemblyToInclude { get; }


		public override bool Equals(object? obj)
			=> obj is AssemblyInclude otherAssemblyInclude && otherAssemblyInclude.AssemblyToInclude == this.AssemblyToInclude;

		public override int GetHashCode()
			=> AssemblyToInclude.GetHashCode();

		public static bool operator ==(AssemblyInclude left, AssemblyInclude right)
			=> left.Equals(right);

		public static bool operator !=(AssemblyInclude left, AssemblyInclude right)
			=> !(left == right);

	}
}
