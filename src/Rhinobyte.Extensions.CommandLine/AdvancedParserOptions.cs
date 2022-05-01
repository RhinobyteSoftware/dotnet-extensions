namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Configuration options for the <see cref="ComplexModelBinder{TOptions}"/> behavior
/// </summary>
public class AdvancedParserOptions
{
	/// <summary>
	/// When true the <see cref="ComplexModelBinder{TOptions}"/> will throw if the underlying <see cref="System.CommandLine.Parsing.ParseResult"/> has any errors
	/// </summary>
	public bool ThrowOnOptionsParserErrors { get; set; }

	/// <summary>
	/// The behavior for the parser to apply for any settable properties on the TOptions type that are missing an explicit
	/// <see cref="OptionAttribute"/> decorator.
	/// </summary>
	public OptionsModelUndecoratedPropertyBehavior UndecoratedPropertyBehavior { get; set; }
}
