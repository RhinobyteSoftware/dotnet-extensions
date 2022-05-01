using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Custom binder implementation for the System.CommandLine parser so we can support binding complex models with attributes decorated and/or default behavior fallbacks based on member names
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public class ComplexModelBinder<TOptions> : BinderBase<TOptions>
	where TOptions : new()
{
	private readonly AdvancedParserOptions _parserOptions;
	private readonly IReadOnlyDictionary<PropertyInfo, Symbol> _symbolLookup;

	/// <summary>
	/// Construct a new binder instance
	/// </summary>
	/// <param name="parserOptions">The options for configurable behaviors</param>
	/// <param name="symbolLookup">The dictionary used to lookup the parser symbol for a <see cref="PropertyInfo"/> from the <typeparamref name="TOptions"/> type</param>
	/// <exception cref="ArgumentNullException">Thrown if either of the required <paramref name="parserOptions"/> or <paramref name="symbolLookup"/> are null</exception>
	public ComplexModelBinder(
		AdvancedParserOptions parserOptions,
		IReadOnlyDictionary<PropertyInfo, Symbol> symbolLookup)
	{
		_parserOptions = parserOptions ?? throw new ArgumentNullException(nameof(parserOptions));
		_symbolLookup = symbolLookup ?? throw new ArgumentNullException(nameof(symbolLookup));
	}

	/// <summary>
	/// Method to expose protected <see cref="GetBoundValue(BindingContext)"/> implementation publicly on the binder
	/// </summary>
	public TOptions CreateOptions(BindingContext bindingContext) => GetBoundValue(bindingContext);

	/// <inheritdoc/>
	protected override TOptions GetBoundValue(BindingContext bindingContext)
	{
		_ = bindingContext ?? throw new ArgumentNullException(nameof(bindingContext));

		var optionsType = typeof(TOptions);
		var optionsProperties = optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		var missingOptions = new List<string>();

		var boundOptions = new TOptions();
		var parseResult = bindingContext.ParseResult;
		foreach (var propertyInfo in optionsProperties)
		{
			if (!_symbolLookup.TryGetValue(propertyInfo, out var symbol))
				continue;

			var isRequired = propertyInfo.GetCustomAttributes()?.Any(attribute => attribute.GetType()?.FullName == "System.ComponentModel.DataAnnotations.RequiredAttribute") == true;
			if (isRequired && parseResult.FindResultFor(symbol) is null)
			{
				var symbolAliases = symbol is IdentifierSymbol identifierSymbol
					? $"'{string.Join("', '", identifierSymbol.Aliases)}' "
					: string.Empty;
				missingOptions.Add($"  {symbolAliases}[Property: {propertyInfo.Name}]");
				continue;
			}

			var propertyValue = symbol switch
			{
				Argument argumentSymbol => parseResult.GetValueForArgument(argumentSymbol),
				Option optionSymbol => parseResult.GetValueForOption(optionSymbol),
				_ => throw new ParseOptionsException($"Unexpected symbol type {symbol.GetType().Name} for property {propertyInfo.Name}")
			};

			propertyInfo.SetValue(boundOptions, propertyValue);
		}

		if (bindingContext.ParseResult.Errors.Count > 0)
		{
			var combinedErrors = $"{Environment.NewLine}{string.Join(Environment.NewLine, bindingContext.ParseResult.Errors)}";

			if (_parserOptions.ThrowOnOptionsParserErrors)
				throw new ParseOptionsException($"The command line options parse result contains errors:{combinedErrors}");
		}

		if (missingOptions.Count > 0)
			throw new ParseOptionsException($"The following required options have not been provided:{Environment.NewLine}{string.Join(Environment.NewLine, missingOptions)}{Environment.NewLine}");

		return boundOptions;
	}
}

