namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Signature for a parser that will parse the provided arguments and bind them to the specified options type.
/// </summary>
public interface IAdvancedParser<TOptions>
	where TOptions : new()
{
	/// <summary>
	/// Parse the provided <paramref name="commandLineArguments"/> and bind the parsed values to a new instance of <typeparamref name="TOptions"/> 
	/// </summary>
	TOptions ParseCommandLineOptions(string[] commandLineArguments);
}

