namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public interface ISomething
	{
		string GetSomething();
	}

	public class SomethingImplementation
	{
		public string GetSomething() => string.Empty;
	}

	public abstract class AbstractSomething : ISomething
	{
		public abstract string GetSomething();
	}

	public class SomethingSubclass : AbstractSomething
	{
		public override string GetSomething() => string.Empty;
	}

	public class BaseClass
	{
		public virtual string GetSomething() => string.Empty;
	}

	public class ChildClass : BaseClass, ISomething
	{

	}

	public class GrandChildClass : ChildClass
	{
		public override string GetSomething() => nameof(GrandChildClass);
	}
}
