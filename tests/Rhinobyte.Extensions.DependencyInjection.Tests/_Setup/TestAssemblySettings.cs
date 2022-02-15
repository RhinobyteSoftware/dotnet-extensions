using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

public class TestAssemblySettings
{
}
