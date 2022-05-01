using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Implementation of the <see cref="IAdvancedParser{TOptions}"/> build on top of the <see cref="System.CommandLine.Parsing.Parser"/> behaviors
/// </summary>
public class AdvancedParser<TOptions> : IAdvancedParser<TOptions>
	where TOptions : new()
{
	private readonly AdvancedParserOptions _parserOptions;

	/// <summary>
	/// Construct a new instance of the advanced parser
	/// </summary>
	public AdvancedParser(
		AdvancedParserOptions parserOptions)
	{
		_parserOptions = parserOptions ?? throw new ArgumentNullException(nameof(parserOptions));
	}

	/// <summary>
	/// Constructs a pair of aliases in the format "--property-name", "/PropertyName"
	/// </summary>
	public string[] CreateAliasesForPropertyName(string propertyName, CultureInfo? cultureInfo = null)
	{
		if (string.IsNullOrWhiteSpace(propertyName))
			throw new ArgumentException($"{nameof(propertyName)} cannot be null or whitespace");

		var doubleDashAliasBuilder = new StringBuilder("--");
		var isFirstCharacter = true;
		var wasPreviousUppercase = false;
		for (var characterIndex = 0; characterIndex < propertyName.Length; ++characterIndex)
		{
			var currentChar = propertyName[characterIndex];
			var isUppercase = char.IsUpper(currentChar);
			if (isUppercase)
				currentChar = char.ToLower(currentChar, cultureInfo ?? CultureInfo.CurrentCulture);

			if (isFirstCharacter)
			{
				isFirstCharacter = false;
				wasPreviousUppercase = isUppercase;
				_ = doubleDashAliasBuilder.Append(currentChar);
				continue;
			}

			if (isUppercase && !wasPreviousUppercase)
				_ = doubleDashAliasBuilder.Append('-');

			wasPreviousUppercase = isUppercase;
			_ = doubleDashAliasBuilder.Append(currentChar);
		}

		return new string[] { doubleDashAliasBuilder.ToString(), $"/{propertyName}" };
	}

	/// <summary>
	/// Create a symbol for the property using reflection to discover the option configuration attribute, if present.
	/// </summary>
	protected Symbol? CreateSymbolForProperty(Type optionsType, PropertyInfo propertyInfo)
	{
		_ = optionsType ?? throw new ArgumentNullException(nameof(optionsType));
		_ = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));

		var cliAttribute = propertyInfo.GetCustomAttribute<OptionAttribute>();
		if (cliAttribute is null)
		{
			//_logger?.LogWarningUndecoratedOptionProperty(optionsType, propertyInfo);
			if (_parserOptions.UndecoratedPropertyBehavior == OptionsModelUndecoratedPropertyBehavior.Ignore)
				return null;

			if (_parserOptions.UndecoratedPropertyBehavior == OptionsModelUndecoratedPropertyBehavior.Error)
				throw new InvalidOperationException($"{optionsType.Name} has settable property {propertyInfo.Name} that is missing a {nameof(OptionAttribute)} decorator. If the property should be ignored it can be decorated as [BinderIgnored].");
		}

		var aliases = cliAttribute?.Aliases ?? CreateAliasesForPropertyName(propertyInfo.Name);
		return new Option(aliases, argumentType: propertyInfo.PropertyType, arity: cliAttribute?.ArgumentArity ?? default, description: cliAttribute?.Description);
	}

	/// <summary>
	/// Create the property to symbol mapping for the specified <paramref name="optionsModelType"/>
	/// </summary>
	/// <param name="optionsModelType">The type of the options model to construct the command line symbol mapping for</param>
	/// <exception cref="ArgumentNullException"></exception>
	public IReadOnlyDictionary<PropertyInfo, Symbol> CreateSymbolsLookup(Type optionsModelType)
	{
		_ = optionsModelType ?? throw new ArgumentNullException(nameof(optionsModelType));

		var modelProperties = optionsModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		var symbolLookup = new Dictionary<PropertyInfo, Symbol>();

		foreach (var propertyInfo in modelProperties)
		{
			if (propertyInfo.SetMethod is null)
				continue;

			var optionSettings = CreateSymbolForProperty(optionsModelType, propertyInfo);
			if (optionSettings is not null)
				symbolLookup.Add(propertyInfo, optionSettings);
		}

		return symbolLookup;
	}

	/// <summary>
	/// Construct a new instance of <typeparamref name="TOptions"/> and set the properties using the parse result values from the provided <paramref name="commandLineArguments"/>
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public TOptions ParseCommandLineOptions(string[] commandLineArguments)
	{
		var symbolLookup = CreateSymbolsLookup(typeof(TOptions));
		var modelBinder = new ComplexModelBinder<TOptions>(_parserOptions, symbolLookup);

		var optionsParserCommand = new RootCommand();
		foreach (var symbol in symbolLookup.Values)
		{
			switch (symbol)
			{
				case Argument argumentSymbol:
					optionsParserCommand.AddArgument(argumentSymbol);
					break;

				case Option optionSymbol:
					optionsParserCommand.AddOption(optionSymbol);
					break;

				default:
					throw new ParseOptionsException($"Unexpected symbol type {symbol.GetType().Name} encountered while configuring the {nameof(optionsParserCommand)}");
			}
		}

		var parseResult = new CommandLineBuilder(optionsParserCommand)
			.UseVersionOption()
			.UseHelp()
			.Build()
			.Parse(commandLineArguments);

		if (parseResult is null)
			throw new InvalidOperationException($"${nameof(Parser)}.{nameof(Parser.Parse)}({nameof(commandLineArguments)} returned a null {nameof(parseResult)} value");

		return parseResult.CreateOptions<TOptions>(modelBinder);
	}
}

// TODO: Decide if we want to add back in logging. The ILogger interface is not CLS compliant, at least not the lower 2.2.0 version of Microsoft.Extensions.Logging.Abstractions...
// TODO: Switch to using the [LoggerMessage(..)] source generator attribute if we start targetting only net6.0+
//internal static class ParserLoggerExtensions
//{
//	private static readonly Action<ILogger, string, string, Exception?> _logWarningUndecoratedOptionProperty =
//		LoggerMessage.Define<string, string>(
//			LogLevel.Warning,
//			new EventId(0, nameof(LogWarningUndecoratedOptionProperty)),
//			"{OptionsTypeName} has settable property {PropertyName} that is missing a CommandLineOptionAttribute decorator. If the property should be ignored it can be decorated as [CommandLineOption(Ignore = true)]."
//		);

//	public static void LogWarningUndecoratedOptionProperty(this ILogger logger, Type optionsType, PropertyInfo propertyInfo)
//		=> _logWarningUndecoratedOptionProperty(logger, optionsType.Name, propertyInfo.Name, null);

//}
