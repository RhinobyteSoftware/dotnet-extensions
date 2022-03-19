using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;

namespace Rhinobyte.Extensions.Logging.Queue;

/// <summary>
/// An interface representing a formatter used by the <see cref="QueueLogger{TMessageEntry}"/> to create the message entry to be enqueued.
/// </summary>
/// <typeparam name="TMessageEntry">The type of the message entry returned by the formatter</typeparam>
public interface ILogMessageFormatter<TMessageEntry>
{
	/// <summary>
	/// Whether or not this log message formatter neeeds the <see cref="TextWriter"/> parameter to be supplied to the <see cref="FormatEntry{TState}(in LogEntry{TState}, IExternalScopeProvider?, TextWriter?)"/> method.
	/// <para>
	/// Used by the <see cref="QueueLogger{TMessageEntry}"/> to determine if the loggers internally maintained thread static <see cref="StringWriter"/> needs to be initialized or can be left null.
	/// </para>
	/// </summary>
	bool IsTextWriterNeeded { get; }

	/// <summary>
	/// Gets the name associated with the log message formatter.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Writes the formatted log message to the specified TextWriter.
	/// </summary>
	/// <param name="logEntry">The log entry.</param>
	/// <param name="scopeProvider">The provider of scope data.</param>
	/// <param name="textWriter">A text writer that is conditionally supplied. If the <see cref="IsTextWriterNeeded"/> property is false, then null is passed instead.</param>
	/// <typeparam name="TState">The type of the object to be written.</typeparam>
	TMessageEntry? FormatEntry<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter? textWriter);
}


/// <summary>
/// An interface representing a formatter used by the <see cref="QueueLogger{TMessageEntry}"/> to create the message entry to be enqueued.
/// /// <para>
/// The <typeparamref name="TOptions"/> generic parameter is used to distinguish service descriptor registrations for different derived implementations of the QueueLoggerProvider and QueueLoggerOptions.
/// </para>
/// </summary>
/// <typeparam name="TMessageEntry">The type of the message entry returned by the formatter</typeparam>
/// <typeparam name="TOptions">The logger options type used to differentiate the service registrations</typeparam>
public interface ILogMessageFormatter<TMessageEntry, TOptions> : ILogMessageFormatter<TMessageEntry>
	where TOptions : QueueLoggerOptions
{ }
