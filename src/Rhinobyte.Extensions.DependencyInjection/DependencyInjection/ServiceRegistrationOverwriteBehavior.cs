using Microsoft.Extensions.DependencyInjection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Behavior to use when attempting to add a service registration into the <see cref="IServiceCollection"/>
	/// </summary>
	public enum ServiceRegistrationOverwriteBehavior
	{
		/// <summary>
		/// Add the service registration if there is not already a <see cref="ServiceDescriptor"/> with the specified <see cref="ServiceDescriptor.ServiceType"
		/// <para></para>
		/// </summary>
		TryAdd = 0,
		Add = 1,
		ReplaceAll = 2,
		ReplaceFirst = 3
	}
}
