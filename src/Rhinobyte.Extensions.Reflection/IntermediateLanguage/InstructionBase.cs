using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage;

/// <summary>
/// Abstract base class representing an individual intermediate language (IL) instruction.
/// </summary>
public abstract class InstructionBase
{
	/// <summary>
	/// Constructs an <see cref="InstructionBase"/> instance with the specified <paramref name="index"/>, <paramref name="offset"/>, <paramref name="opcode"/>, and instruction <paramref name="size"/>
	/// </summary>
	/// <param name="index">The instruction index within the collection of instructions for a set of IL bytes</param>
	/// <param name="offset">The offset of the instruction within the IL byte[]</param>
	/// <param name="opcode">The specific <see cref="OpCode"/> for this instruction</param>
	/// <param name="size">The size in bytes of the instruction</param>
	protected InstructionBase(int index, int offset, OpCode opcode, int size)
	{
		Index = index;
		Offset = offset;
		OpCode = opcode;
		Size = size;
	}

	/// <summary>
	/// The instruction's index within the list of method body instructions.
	/// </summary>
	public int Index { get; }

	/// <summary>
	/// The next instruction in the intermediate language (IL) bytes.
	/// </summary>
	public InstructionBase? NextInstruction { get; internal set; }

	/// <summary>
	/// The relative offset of this instruction within the intermediate language (IL) bytes.
	/// </summary>
	public int Offset { get; }

	/// <summary>
	/// The <see cref="OpCode"/> for this instruction.
	/// </summary>
	public OpCode OpCode { get; }

	/// <summary>
	/// The previous instruction in the intermediate language (IL) bytes.
	/// </summary>
	public InstructionBase? PreviousInstruction { get; internal set; }

	/// <summary>
	/// The size in bytes of the intermediate language (IL) instruction.
	/// </summary>
	public virtual int Size { get; }

	/// <summary>
	/// Returns a string the represents the IL instruction with a human readable description for the <see cref="OpCode"/> and corresponding operands
	/// </summary>
	public override string ToString()
	{
		if (OpCodeHelper.ShortDescriptionLookup.TryGetValue(OpCode.Value, out var description))
		{
			return $"({Index}) {description}";
		}

		return $"({Index}) {OpCode.Name ?? "?UnknownOpCode?"}";
	}
}
