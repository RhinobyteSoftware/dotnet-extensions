using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.SolutionIntegrationTests;

[TestClass]
public class DotNetSdkTests
{
	public CancellationToken CancellationTokenForTest => TestContext.CancellationTokenSource.Token;

	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task Global_json_file_specifies_highest_stable_sdk_version()
	{
		var directoryRoot = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
		var repoRootDirectory = Directory.GetCurrentDirectory();
		var solutionFilePath = Path.Combine(repoRootDirectory, "Rhinobyte.Extensions.sln");
		while (!File.Exists(solutionFilePath))
		{
			repoRootDirectory = Directory.GetParent(repoRootDirectory)?.FullName;
			if (string.IsNullOrWhiteSpace(repoRootDirectory) || repoRootDirectory == directoryRoot)
				throw new AssertFailedException("Unable to locate the repository root directory containing the Rhinobyte.Extensions.sln file");

			solutionFilePath = Path.Combine(repoRootDirectory, "Rhinobyte.Extensions.sln");
		}

		var globalJsonPath = Path.Combine(repoRootDirectory, "global.json");
		if (!File.Exists(globalJsonPath))
			throw new AssertFailedException($"{globalJsonPath} file does not exist");

		var globalJsonFileContent = await File.ReadAllTextAsync(globalJsonPath, CancellationTokenForTest);
		var globalJsonConfiguration = JsonConvert.DeserializeObject<GlobalJsonConfiguration>(globalJsonFileContent);

		_ = globalJsonConfiguration ?? throw new AssertFailedException("Failed to deserialize global.json file contents");

		var ourDotnetSdkVersion = globalJsonConfiguration.Sdk?.Version;
		if (string.IsNullOrWhiteSpace(ourDotnetSdkVersion))
			throw new AssertFailedException("global.json sdk version value is null or whitespace");

		using var serviceProvider = new ServiceCollection()
			.AddHttpClient()
			.BuildServiceProvider();

		var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
		using var dotnetDownloadsHttpClient = httpClientFactory.CreateClient();
		dotnetDownloadsHttpClient.BaseAddress = new System.Uri("https://dotnet.microsoft.com/");

		var dotnetMajorVersion = 5;
		var dotnetMinorVersion = 0;
		string? latestStableSdkVersion = null;
		var allDiscoveredSdkVersions = new List<string>();
		while (true)
		{
			var nextSdkVersion = await ScreenScrapeLatestSdkVersionAsync(dotnetDownloadsHttpClient, dotnetMajorVersion, dotnetMinorVersion, CancellationTokenForTest);
			if (string.IsNullOrWhiteSpace(nextSdkVersion))
			{
				if (dotnetMinorVersion == 0)
					break;

				dotnetMinorVersion = 0;
				++dotnetMajorVersion;
				continue;
			}

			allDiscoveredSdkVersions.Add(nextSdkVersion);

			if (!nextSdkVersion.Contains('-'))
			{
				latestStableSdkVersion = nextSdkVersion;
			}

			++dotnetMinorVersion;
		}

		if (string.IsNullOrWhiteSpace(latestStableSdkVersion))
			throw new AssertFailedException($"Failed to locate any values for the latest stable sdk version:  [{string.Join(", ", allDiscoveredSdkVersions)}]");

		if (ourDotnetSdkVersion != latestStableSdkVersion)
			throw new AssertFailedException($"Our global.json specifies a dotnet sdk version of {ourDotnetSdkVersion} that is lower than the latest stable version of {latestStableSdkVersion}");
	}

	public static async Task<string?> ScreenScrapeLatestSdkVersionAsync(HttpClient dotnetDownloadClient, int dotnetMajorVersion, int dotnetMinorVersion, CancellationToken cancellationToken)
	{
		using var httpResponse = await dotnetDownloadClient.GetAsync($"download/dotnet/{dotnetMajorVersion}.{dotnetMinorVersion}", cancellationToken);
		if (!httpResponse.IsSuccessStatusCode)
			return null;

		var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

		if (string.IsNullOrWhiteSpace(responseString))
			throw new AssertFailedException("dotnet download versions response string was null or whitespace");

		// Screen scrape release dotnet versions until I find a better way to query for these
		var sdkStringIndex = responseString.IndexOf($"SDK {dotnetMajorVersion}.{dotnetMinorVersion}.");
		var sdkSubstring = responseString.Substring(sdkStringIndex + 4, 100);
		var endOfVersionNumberIndex = sdkSubstring.IndexOf('<');
		var latestSdkVersion = sdkSubstring.Substring(0, endOfVersionNumberIndex);
		return latestSdkVersion;
	}

	public class GlobalJsonConfiguration
	{
		public SdkConfiguration? Sdk { get; set; }

		public class SdkConfiguration
		{
			public string? Version { get; set; }
		}
	}
}
