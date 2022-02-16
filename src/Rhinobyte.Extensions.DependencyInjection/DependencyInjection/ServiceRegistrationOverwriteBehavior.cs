using Microsoft.Extensions.DependencyInjection;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Behavior to use when attempting to add a service registration into the <see cref="IServiceCollection"/>
/// </summary>
public enum ServiceRegistrationOverwriteBehavior
{
	/// <summary>
	/// Add the service registration if there is not already a <see cref="ServiceDescriptor"/> with the specified <see cref="ServiceDescriptor.ServiceType" />
	/// </summary>
	TryAdd = 0,
	/// <summary>
	/// Adds the service registration even if one or more descriptors exist with the specified <see cref="ServiceDescriptor.ServiceType" />
	/// </summary>
	Add = 1,
	/// <summary>
	/// Removes all existing <see cref="ServiceDescriptor"/> with the specified <see cref="ServiceDescriptor.ServiceType"/> before adding the new registrations
	/// </summary>
	ReplaceAll = 2,
	/// <summary>
	/// Removes the first instance with the specified <see cref="ServiceDescriptor.ServiceType"/> and replaces it with the new registration.
	/// <para>Cannot be used when registering multiple descriptors at once.</para>
	/// </summary>
	ReplaceFirst = 3
}
