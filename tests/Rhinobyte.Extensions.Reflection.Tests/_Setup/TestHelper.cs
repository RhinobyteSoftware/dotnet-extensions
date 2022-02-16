using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

public static class TestHelper
{
	public static Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken)
	{
#if NETCOREAPP
		return File.ReadAllTextAsync(filePath, cancellationToken);
#else
		return Task.FromResult(File.ReadAllText(filePath));
#endif
	}
}
