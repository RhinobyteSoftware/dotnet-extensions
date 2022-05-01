using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Convenience extension methods to allow for fluent like call chaining against <see cref="ParseResult"/> type
/// </summary>
public static class ParseResultExtensions
{

	/// <summary>
	/// Create and return a <typeparamref name="TOptions"/> instance using <paramref name="modelBinder"/> to bind the values from the <paramref name="parseResult"/>
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	public static TOptions CreateOptions<TOptions>(this ParseResult parseResult, ComplexModelBinder<TOptions> modelBinder, IConsole? console = null)
		where TOptions : new()
	{
		_ = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
		_ = modelBinder ?? throw new ArgumentNullException(nameof(modelBinder));

		var bindingContext = new InvocationContext(parseResult, console: console).BindingContext;
		return modelBinder.CreateOptions(bindingContext);
	}
}
