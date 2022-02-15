namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Resolution strategy used by the <see cref="InterfaceImplementationsConvention"/> to determine which implementation types to register against a discovered
/// interface type.
/// </summary>
public enum InterfaceImplementationResolutionStrategy
{
	/// <summary>
	/// <see cref="InterfaceImplementationsConvention"/> instances will only attempt to register a <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/>
	/// for the discovered interface type if there is a concrete type that matches the default convention name.
	/// <para>e.g. A concrete type named <c>Something</c> that implements an interface named <c>ISomething</c></para>
	/// </summary>
	DefaultConventionOnly = 0,
	/// <summary>
	/// <see cref="InterfaceImplementationsConvention"/> instances will attempt to register a single <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/>
	/// for the discovered interface type if there is a concrete type that matches the default convention name.
	/// <para>
	/// If a concrete implementation is not found that matches the default naming convention then all a service registration is attempted for all discovered
	/// implementations of the interface type.
	/// </para>
	/// </summary>
	DefaultConventionOrAll = 1,
	/// <summary>
	/// <see cref="InterfaceImplementationsConvention"/> will attempt to register a single <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/>
	/// for the discovered interface type.
	/// <para>
	/// If multiple implementation types are discovered the registration will only occur if one of the implementation types matches the default naming convention.
	/// </para>
	/// <para>
	/// If no implementation matches the default naming convention but there is only a single discovered implementation type, the single implementation will be used
	/// for the registration.
	/// </para>
	/// </summary>
	DefaultConventionOrSingleImplementationOnly = 2,
	/// <summary>
	/// <see cref="InterfaceImplementationsConvention"/> will attempt to register a <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/>
	/// when a single implementation type is discovered. The implementation type does not need to match the default naming convention.
	/// <para>
	/// If multiple implementation types are discovered then convention will not attempt to register any descriptors for the interface.
	/// </para>
	/// </summary>
	SingleImplementationOnly = 3,
	/// <summary>
	/// <see cref="InterfaceImplementationsConvention"/> will attempt to register <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/>
	/// instances for all discovered implementation types.
	/// </summary>
	AllImplementations = 4
}
