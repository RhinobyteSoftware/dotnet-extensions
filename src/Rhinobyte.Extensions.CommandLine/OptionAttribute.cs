using System;
using System.CommandLine;

namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Attribute for configuring command line option values that will be used to bind to the decorated property
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class OptionAttribute : Attribute
{
	/// <summary>
	/// The aliases for the <see cref="System.CommandLine.Option{T}"/>
	/// </summary>
	public string[]? Aliases { get; set; }

	/// <summary>
	/// The arity for the option arguments
	/// </summary>
	public ArgumentArity ArgumentArity { get; set; }

	/// <summary>
	/// An optional value for the <see cref="System.CommandLine.Symbol.Description"/>
	/// </summary>
	public string? Description { get; set; }
}
