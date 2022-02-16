using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

// TODO: Next Major Release
// Export this as part of the the Rhinobyte.Extensions.Reflection library's public api so others can use it if they want
public class RecursiveInstructionFormatter : IInstructionFormatter
{
	public const string DefaultIndentationString = "\t";

	public RecursiveInstructionFormatter(
		bool catchParseChildExceptions = true,
		string? childLinePrefix = "- ",
		string? indentationString = null,
		int maxTraversalDepth = 5,
		bool printFullChildTypeName = true)
	{
		CatchParseChildExceptions = catchParseChildExceptions;
		ChildLinePrefix = childLinePrefix;
		MaxTraversalDepth = maxTraversalDepth;
		PrintFullChildTypeName = printFullChildTypeName;

		// Add a string for (maxTraversalDepth + 1) since we'll add a line with a single message that the traversal depth has been exceeded
		IndentationStringLookup = BuildIndentationStringLookup(indentationString ?? DefaultIndentationString, maxTraversalDepth + 1);
	}


	public bool CatchParseChildExceptions { get; }

	public string? ChildLinePrefix { get; }

	public IReadOnlyDictionary<int, string> IndentationStringLookup { get; }

	public int MaxTraversalDepth { get; }

	public bool PrintFullChildTypeName { get; }


	private static IReadOnlyDictionary<int, string> BuildIndentationStringLookup(string indentationString, int traversalDepth)
	{
		var indentationStringLookup = new Dictionary<int, string>
		{
			[0] = string.Empty
		};

		var stringBuilder = new StringBuilder(indentationString.Length * traversalDepth);
		for (var indentationIndex = 1; indentationIndex <= traversalDepth; ++indentationIndex)
		{
			_ = stringBuilder.Append(indentationString);
			indentationStringLookup[indentationIndex] = stringBuilder.ToString();
		}

		return indentationStringLookup;
	}

	private static void CrawlInstructions(
		RecursiveInstructionFormatter formatter,
		Stack<NodeToCrawl> nodesToCrawl,
		StringBuilder stringBuilder,
		HashSet<MethodBase> visitedMembers)
	{
		while (nodesToCrawl.Count > 0)
		{
			var nodeToCrawl = nodesToCrawl.Pop();

			var indentationString = formatter.IndentationStringLookup[nodeToCrawl.TraversalDepth];
			_ = stringBuilder
				.Append(indentationString)
				.Append(nodeToCrawl.Instruction.ToString())
				.Append(Environment.NewLine);


			var childMethodToCrawl = GetMemberToCrawl(nodeToCrawl.Instruction);
			if (childMethodToCrawl is null)
				continue;

			var nextDepth = nodeToCrawl.TraversalDepth + 1;
			var nextIndentationString = formatter.IndentationStringLookup[nextDepth];
			_ = stringBuilder
				.Append(nextIndentationString)
				.Append(formatter.ChildLinePrefix)
				.Append('[');

			_ = stringBuilder
				.Append(childMethodToCrawl.GetSignature(formatter.PrintFullChildTypeName)).Append(']')
				.Append(Environment.NewLine);

			if (visitedMembers.Contains(childMethodToCrawl))
			{
				_ = stringBuilder
					.Append(nextIndentationString)
					.Append("... previously crawled ...")
					.Append(Environment.NewLine);
				continue;
			}

			if (nodeToCrawl.TraversalDepth == formatter.MaxTraversalDepth)
			{
				// Crawling further would exceed max depth
				_ = stringBuilder
					.Append(nextIndentationString)
					.Append("... exceeds max traversal depth ...")
					.Append(Environment.NewLine);
				continue;
			}

			visitedMembers.Add(childMethodToCrawl);

			try
			{
				var childInstructions = new MethodBodyParser(childMethodToCrawl).ParseInstructions();

				// Push the instructions onto the stack in reverse order so the output will be in sequence for a given traversal level
				for (var instructionIndex = childInstructions.Count - 1; instructionIndex > -1; --instructionIndex)
					nodesToCrawl.Push(new NodeToCrawl(childInstructions[instructionIndex], nextDepth));
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception exc) when (formatter.CatchParseChildExceptions)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				var exceptionMessage = exc.Message.Replace(Environment.NewLine, $"{Environment.NewLine}{nextIndentationString}");

				_ = stringBuilder
					.Append(nextIndentationString)
					.Append("... exception thrown attempting to parse child method ...")
					.Append(Environment.NewLine)
					.Append(nextIndentationString)
					.Append(exceptionMessage)
					.Append(Environment.NewLine);
			}
		}
	}

	public string DescribeInstruction(InstructionBase instruction)
	{
		_ = instruction ?? throw new ArgumentNullException(nameof(instruction));

		var stringBuilder = new StringBuilder();
		var nodesToCrawl = new Stack<NodeToCrawl>();
		var visitedMembers = new HashSet<MethodBase>();

		nodesToCrawl.Push(new NodeToCrawl(instruction, 0));
		CrawlInstructions(this, nodesToCrawl, stringBuilder, visitedMembers);

		return stringBuilder.ToString();
	}

	public string DescribeInstructions(IEnumerable<InstructionBase> instructionsToDescribe)
	{
		_ = instructionsToDescribe ?? throw new ArgumentNullException(nameof(instructionsToDescribe));

		var stringBuilder = new StringBuilder();
		var nodesToCrawl = new Stack<NodeToCrawl>();
		var visitedMembers = new HashSet<MethodBase>();

		// Push the instructions onto the stack in reverse order so the output will be in sequence for a given traversal level
		if (instructionsToDescribe is IReadOnlyList<InstructionBase> instructionsList)
		{
			for (var instructionIndex = instructionsList.Count - 1; instructionIndex > -1; --instructionIndex)
				nodesToCrawl.Push(new NodeToCrawl(instructionsList[instructionIndex], 0));
		}
		else
		{
			foreach (var instruction in instructionsToDescribe.Reverse())
				nodesToCrawl.Push(new NodeToCrawl(instruction, 0));
		}

		CrawlInstructions(this, nodesToCrawl, stringBuilder, visitedMembers);

		return stringBuilder.ToString();
	}

	public static MethodBase? GetMemberToCrawl(InstructionBase instruction)
	{
		switch (instruction)
		{
			case MethodReferenceInstruction methodReferenceInstruction:
			{
				var previousInstruction = methodReferenceInstruction.PreviousInstruction;
				if (previousInstruction is not null
					&& methodReferenceInstruction.MethodReference is not null
					&& previousInstruction.OpCode == OpCodes.Constrained
					&& previousInstruction is TypeReferenceInstruction typeReferenceInstruction
					&& typeReferenceInstruction.TypeReference is not null)
				{
					var constrainedVirtualType = typeReferenceInstruction.TypeReference;
					var constrainedVirtualMethod = MethodBodyParser.FindMethodOnConstrainingType(constrainedVirtualType, methodReferenceInstruction.MethodReference);
					if (constrainedVirtualMethod is not null)
						return constrainedVirtualMethod;
				}

				return methodReferenceInstruction.MethodReference;
			}

			default:
				return null;
		}
	}

	private class NodeToCrawl
	{
		public NodeToCrawl(
			InstructionBase instruction,
			int traversalDepth)
		{
			Instruction = instruction;
			TraversalDepth = traversalDepth;
		}

		public InstructionBase Instruction { get; }

		public int TraversalDepth { get; }
	}
}
