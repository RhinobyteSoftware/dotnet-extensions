using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;

namespace Rhinobyte.Extensions.Logging.Tests;

[TestClass]
public class UnitTest1
{
	[TestMethod]
	public void TestMethod1()
	{
		using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
		{
			_ = loggingBuilder.AddProvider(new LoggerProviderNoExternalScopeSupport());
		});

		var aggregateLogger = loggerFactory.CreateLogger<UnitTest1>();

		// TODO: Lookback at older target code and understand what this test intention / begin scope should never be called assumption was for...
		//using var someScope = aggregateLogger.BeginScope("Test Scope");

#pragma warning disable CA1848 // Use the LoggerMessage delegates
		aggregateLogger.LogInformation("Information logging statement");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
	}
}


public class BeginScopeThrowsLogger : ILogger
{
	public BeginScopeThrowsLogger()
	{
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return false;
	}

	public IDisposable? BeginScope<TState>(TState state)
		where TState : notnull
	{
		throw new InvalidOperationException("BeginScopeExampleLogger.BeginScope should never be called");
	}
}

public sealed class LoggerProviderNoExternalScopeSupport : ILoggerProvider
{
	private readonly ConcurrentDictionary<string, BeginScopeThrowsLogger> _loggers = new();

	public void Dispose()
	{
	}

	public ILogger CreateLogger(string categoryName)
	{
		return _loggers.TryGetValue(categoryName, out var logger)
			? logger
			: _loggers.GetOrAdd(categoryName, new BeginScopeThrowsLogger());
	}
}
