using System;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Contract for <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/> subclasses that supply a method for return the implementation type of the descriptor
/// </summary>
/// <remarks>
/// The <see cref="ServiceDescriptorExtensions.TryGetImplementationType(Microsoft.Extensions.DependencyInjection.ServiceDescriptor)"/> method will check for and use this
/// contract before checking the base service descriptor properties
/// </remarks>
public interface ICustomServiceDescriptor
{
	/// <summary>
	/// Contract method to return the implementation <see cref="Type"/> that a custom <see cref="Microsoft.Extensions.DependencyInjection.ServiceDescriptor"/>
	/// represents.
	/// </summary>
	Type GetImplementationType();
}
