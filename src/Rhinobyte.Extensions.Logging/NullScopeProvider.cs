using Microsoft.Extensions.Logging;
using System;

namespace Rhinobyte.Extensions.Logging;

/// <summary>
/// Scope provider that does nothing.
/// </summary>
/// <remarks>
/// It would be nice if Microsoft.Extensions.Logging exported this publicly so custom provider's don't have to duplicate it.
/// </remarks>
internal sealed class NullScopeProvider : IExternalScopeProvider
{
	private NullScopeProvider()
	{
	}

	/// <summary>
	/// Singleton instance of the <see cref="NullScopeProvider"/>
	/// </summary>
	public static IExternalScopeProvider Instance { get; } = new NullScopeProvider();

	/// <inheritdoc />
	void IExternalScopeProvider.ForEachScope<TState>(Action<object?, TState> callback, TState state)
	{ }

	/// <inheritdoc />
	IDisposable IExternalScopeProvider.Push(object? state) => NullScope.Instance;
}
