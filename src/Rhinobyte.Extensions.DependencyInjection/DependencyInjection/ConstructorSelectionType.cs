using Microsoft.Extensions.DependencyInjection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// The constructor selection strategy to use when registering types against an <see cref="IServiceCollection"/>
	/// </summary>
	public enum ConstructorSelectionType
	{
		DefaultBehaviorOnly = 0,
		AttributeThenDefaultBehavior = 1,
		AttributeThenMostParametersWhenAmbiguous = 2,
		MostParametersWhenAmbiguous = 3,
		MostParametersOnly = 4,
		AttributeThenMostParametersOnly = 5,
	}
}
