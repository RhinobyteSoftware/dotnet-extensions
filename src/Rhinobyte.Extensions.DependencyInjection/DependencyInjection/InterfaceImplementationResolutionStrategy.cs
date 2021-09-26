namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Resolution strategy used by the <see cref="InterfaceImplementationsConvention"/> to determine which implementation types to register against a discovered
	/// interface type
	/// </summary>
	public enum InterfaceImplementationResolutionStrategy
	{
		DefaultConventionOnly = 0,
		DefaultConventionOrAll = 1,
		DefaultConventionOrSingleImplementationOnly = 2,
		SingleImplementationOnly = 3,
		AllImplementations = 4
	}
}
