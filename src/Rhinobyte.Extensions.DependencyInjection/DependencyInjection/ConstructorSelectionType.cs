using Microsoft.Extensions.DependencyInjection;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// The constructor selection strategy to use when registering types against an <see cref="IServiceCollection"/>
/// </summary>
public enum ConstructorSelectionType
{
	/// <summary>
	/// Do not select an explicit constructor.
	/// </summary>
	/// <remarks>
	/// <see cref="ExplicitConstructorServiceDescriptor.SelectCustomConstructor(System.Type, ConstructorSelectionType)"/> will return null
	/// and the default <see cref="System.IServiceProvider"/> behavior will be used.
	/// </remarks>
	DefaultBehaviorOnly = 0,
	/// <summary>
	/// Select an explicit constructor if there potentially ambiguous constructors and one of them is decorated with a
	/// <see cref="DependencyInjectionConstructorAttribute"/>.
	/// </summary>
	AttributeThenDefaultBehavior = 1,
	/// <summary>
	/// Select an explicit constructor if there are potentially ambiguous constructors. Prioritizes a constructor with a
	/// <see cref="DependencyInjectionConstructorAttribute"/>. If no attribute decorator is found the constructor with the
	/// most parameters is selected.
	/// </summary>
	AttributeThenMostParametersWhenAmbiguous = 2,
	/// <summary>
	/// Select an explicit constructor if there are potentially ambiguous constructors. Selects the constructor with the
	/// most parameters even if there is a different constructor decorated with a <see cref="DependencyInjectionConstructorAttribute"/>.
	/// </summary>
	MostParametersWhenAmbiguous = 3,
	/// <summary>
	/// Select an explicit constructor if there are multiple potential constructors. The constructors do not necessarily need to be
	/// potentially ambiguous.
	/// <para>
	/// Selects the constructor with the most parameters even if there is a different constructor decorated with a <see cref="DependencyInjectionConstructorAttribute"/>.
	/// </para>
	/// </summary>
	MostParametersOnly = 4,
	/// <summary>
	/// Select an explicit constructor if there are multiple potential constructors. The constructors do not necessarily need to be
	/// potentially ambiguous.
	/// <para>
	/// Prioritizes a constructor with a <see cref="DependencyInjectionConstructorAttribute"/>. If no attribute decorator is found
	/// the constructor with the most parameters is selected.
	/// </para>
	/// </summary>
	AttributeThenMostParametersOnly = 5,
}
