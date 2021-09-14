namespace Rhinobyte.Extensions.DependencyInjection
{
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
