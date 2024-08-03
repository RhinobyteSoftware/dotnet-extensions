using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// Instruction formatter that recursively crawls through the instructions of a method body to describe the instructions and their child instructions.
/// </summary>
public class RecursiveInstructionFormatter : IInstructionFormatter
{
	/// <summary>
	/// The default string used for indentation of the formatted instruction output.
	/// </summary>
	public const string DefaultIndentationString = "\t";

	/// <summary>
	/// Construct a new <see cref="RecursiveInstructionFormatter"/> with the provided settings.
	/// </summary>
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


	/// <summary>
	/// When true the attempts to parse child method instructions will catch any thrown exceptions, append a message to the output, and continue parsing the remaining instructions.
	/// </summary>
	public bool CatchParseChildExceptions { get; }

	/// <summary>
	/// The string prefix to use for child method instructions.
	/// </summary>
	public string? ChildLinePrefix { get; }

	/// <summary>
	/// The lookup of indentation strings to use for each traversal depth.
	/// </summary>
	public IReadOnlyDictionary<int, string> IndentationStringLookup { get; }

	/// <summary>
	/// The maximum depth to traverse when describing child instructions.
	/// </summary>
	public int MaxTraversalDepth { get; }

	/// <summary>
	/// When true the full type name will be printed for child method instructions.
	/// </summary>
	public bool PrintFullChildTypeName { get; }


	private static Dictionary<int, string> BuildIndentationStringLookup(string indentationString, int traversalDepth)
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

			_ = visitedMembers.Add(childMethodToCrawl);

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
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
				var exceptionMessage = exc.Message.Replace(Environment.NewLine, $"{Environment.NewLine}{nextIndentationString}", StringComparison.Ordinal);
#else
				var exceptionMessage = exc.Message.Replace(Environment.NewLine, $"{Environment.NewLine}{nextIndentationString}");
#endif
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

	/// <summary>
	/// Return a formatted string describing the provided <paramref name="instruction"/>.
	/// </summary>
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

	/// <summary>
	/// Return a formatted string describing the provided <paramref name="instructionsToDescribe"/>.
	/// </summary>
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

	/// <summary>
	/// If the instruction is a <see cref="MethodReferenceInstruction"/> that references a method, return the referenced method to crawl
	/// </summary>
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
