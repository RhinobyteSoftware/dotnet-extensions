using System;

namespace Rhinobyte.Extensions.Logging;

/// <summary>
/// An empty scope without any logic
/// </summary>
/// <remarks>
/// It would be nice if Microsoft.Extensions.Logging exported this publicly so custom provider's don't have to duplicate it.
/// </remarks>
public sealed class NullScope : IDisposable
{
	/// <summary>
	/// Singleton instance of the <see cref="NullScope"/> returned by loggers when scopes aren't enabled/supported
	/// </summary>
	public static NullScope Instance { get; } = new NullScope();

	private NullScope()
	{ }

	/// <inheritdoc />
	public void Dispose()
	{ }
}
