using System;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

public interface IInheritedExampleType
{
	void DoSomethingElse1(int value1, int value2);
	void DoSomethingElse2(int value1, int value2);
}

public abstract class InheritedAbstractClass : IInheritedExampleType
{
	public abstract string DoSomething();

	public virtual string DoSomething(string input) => input;

	public abstract void DoSomethingElse1(int value1, int value2);

	public virtual void DoSomethingElse2(int value1, int value2) => throw new NotImplementedException();

	public virtual void DoSomethingOverridenInGrandchildButNotChild() => throw new NotImplementedException();
}

public class ChildOfAbstractClass : InheritedAbstractClass
{
	public override string DoSomething()
	{
		return $"{nameof(ChildOfAbstractClass)}.{nameof(DoSomething)}";
	}

	public override string DoSomething(string input) => $"{input} [Called From Child Class]";

	public override void DoSomethingElse1(int value1, int value2)
	{
		System.Console.WriteLine($"Sum: {value1 + value2}");
	}
}

public class GrandChildOfAbstractClass : ChildOfAbstractClass
{
	public new virtual void DoSomethingElse1(int value1, int value2)
	{
		if (value1 < value2)
			throw new InvalidOperationException("value1 cannot be less than value2");
	}

	public override void DoSomethingElse2(int value1, int value2)
	{
		if (value1 > value2)
			throw new InvalidOperationException("value1 cannot be greater than value2");
	}

	public override void DoSomethingOverridenInGrandchildButNotChild()
	{
		System.Console.WriteLine("Not going to throw");
	}
}

public class GreatGrandChildOfAbstractClass : ChildOfAbstractClass
{
	public override void DoSomethingElse1(int value1, int value2) => base.DoSomethingElse1(value1, value2);
}
