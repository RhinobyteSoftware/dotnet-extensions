using System;

namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Exception type thrown when errors occur parsing the command line options or binding the parse results to the complex model members
/// </summary>
[Serializable]
public class ParseOptionsException : InvalidOperationException
{
	/// <summary>
	/// Construct a new instance of the exception
	/// </summary>
	public ParseOptionsException()
		: base()
	{
	}

	/// <summary>
	/// Construct a new instance of the exception with the provided message
	/// </summary>
	public ParseOptionsException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Construct a new instance of the exception with the provided message and inner exception
	/// </summary>
	public ParseOptionsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}


