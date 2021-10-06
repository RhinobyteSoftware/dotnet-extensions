using Microsoft.Build.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Rhinobyte.Tools.ProjectStructureTests.Tests.Setup
{
	public class MockBuildEngine : IBuildEngine
	{
		public MockBuildEngine(
			int columnNumberOfTaskNode = 0,
			bool continueOnError = false,
			int lineNumberOfTaskNode = 0,
			string? projectFileOfTaskNode = "")
		{
			ColumnNumberOfTaskNode = columnNumberOfTaskNode;
			ContinueOnError = continueOnError;
			LineNumberOfTaskNode = lineNumberOfTaskNode;
			ProjectFileOfTaskNode = projectFileOfTaskNode;
		}

		public int ColumnNumberOfTaskNode { get; set; }
		public bool ContinueOnError { get; set; }
		public int LineNumberOfTaskNode { get; set; }
		public string? ProjectFileOfTaskNode { get; set; }

#pragma warning disable CA1002 // Do not expose generic lists
		public List<(string ProjectFileName, string[] TargetNames, IDictionary GlobalProperties, IDictionary TargetOutpus)> BuildProjectFileRequests = new();

		public List<CustomBuildEventArgs> LogCustomEventRequests = new();

		public List<BuildErrorEventArgs> LogErrorEventRequests = new();

		public List<BuildMessageEventArgs> LogMessageEventRequests = new();

		public List<BuildWarningEventArgs> LogWarningEventRequests = new();
#pragma warning disable CA1002 // Do not expose generic lists

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			lock (BuildProjectFileRequests)
			{
				BuildProjectFileRequests.Add((projectFileName, targetNames, globalProperties, targetOutputs));
			}

			return true;
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
			lock (LogCustomEventRequests)
			{
				LogCustomEventRequests.Add(e);
			}
		}

		public void LogErrorEvent(BuildErrorEventArgs e)
		{
			lock (LogErrorEventRequests)
			{
				LogErrorEventRequests.Add(e);
			}
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
			lock (LogMessageEventRequests)
			{
				LogMessageEventRequests.Add(e);
			}
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
			lock (LogWarningEventRequests)
			{
				LogWarningEventRequests.Add(e);
			}
		}
	}
}
