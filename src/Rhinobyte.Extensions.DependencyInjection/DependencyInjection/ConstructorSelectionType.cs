namespace Rhinobyte.Extensions.DependencyInjection
{
	public enum ConstructorSelectionType
	{
		AttributeThenDefaultBehavior = 0,
		DefaultBehaviorOnly = 1,
		AttributeThenMostParametersWhenAmbiguous = 2,
		MostParametersWhenAmbiguous = 3,
		MostParametersOnly = 4,
		AttributeThenMostParametersOnly = 5,
	}
}
