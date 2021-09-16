namespace Rhinobyte.Extensions.DependencyInjection
{
	public enum InterfaceImplementationResolutionStrategy
	{
		DefaultConventionOnly = 0,
		DefaultConventionOrAll = 1,
		DefaultConventionOrSingleImplementationOnly = 2,
		SingleImplementationOnly = 3,
		AllImplementations = 4
	}
}
