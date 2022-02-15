using System;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

public class MockLocalVariableInfo : LocalVariableInfo
{
	public MockLocalVariableInfo(
		bool isPinned,
		int localIndex,
		Type? localType)
		: base()
	{
		IsPinned = isPinned;
		LocalIndex = localIndex;
		LocalType = localType;
	}

	public override bool IsPinned { get; }
	public override int LocalIndex { get; }
#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
	public override Type? LocalType { get; }
#pragma warning restore CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
}
