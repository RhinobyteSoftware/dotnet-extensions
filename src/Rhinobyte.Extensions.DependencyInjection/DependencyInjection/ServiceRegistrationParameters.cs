using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Wrapper type used by <see cref="ServiceRegistrationConventionBase"/> to allow convention subclasses to specify one or more service descriptors
/// to register.
/// <para>
/// Subclasses implementing ServiceRegistrationConventionBase can specify override values such as <see cref="ServiceRegistrationOverwriteBehavior"/>
/// to use for the specific registration in lieu of the convention class's default value
/// </para>
/// </summary>
[DebuggerDisplay("ServiceDescriptor = {ServiceDescriptor}, ServiceDescriptors = {ServiceDescriptors}, OverwriteBehavior = {OverwriteBehavior}, SkipDuplicates = {SkipDuplicates}, SkipImplementationTypesAlreadyInUse = {SkipImplementationTypesAlreadyInUse}")]
public class ServiceRegistrationParameters
{
	/// <summary>
	/// Constructs an instance of the registration parameters for a single <paramref name="serviceDescriptor"/>
	/// </summary>
	public ServiceRegistrationParameters(
		ServiceDescriptor serviceDescriptor,
		ServiceRegistrationOverwriteBehavior? overwriteBehavior = null,
		bool? skipDuplicates = null,
		bool? skipImplementationTypesAlreadyInUse = null)
	{
		ServiceDescriptor = serviceDescriptor ?? throw new ArgumentNullException(nameof(serviceDescriptor));
		OverwriteBehavior = overwriteBehavior;
		SkipDuplicates = skipDuplicates;
		SkipImplementationTypesAlreadyInUse = skipImplementationTypesAlreadyInUse;
	}

	/// <summary>
	/// Constructs an instance of the registration parameters for multiple <paramref name="serviceDescriptors"/>
	/// </summary>
	public ServiceRegistrationParameters(
		IEnumerable<ServiceDescriptor> serviceDescriptors,
		ServiceRegistrationOverwriteBehavior? overwriteBehavior = null,
		bool? skipDuplicates = null,
		bool? skipImplementationTypesAlreadyInUse = null)
	{
		ServiceDescriptors = serviceDescriptors ?? throw new ArgumentNullException(nameof(serviceDescriptors));
		OverwriteBehavior = overwriteBehavior;
		SkipDuplicates = skipDuplicates;
		SkipImplementationTypesAlreadyInUse = skipImplementationTypesAlreadyInUse;
	}

	/// <summary>
	/// An explicit <see cref="ServiceRegistrationOverwriteBehavior"/> value to use for this registration.
	/// <para>When left null, the default value from the registration convention will be used</para>
	/// </summary>
	public ServiceRegistrationOverwriteBehavior? OverwriteBehavior { get; }

	/// <summary>
	/// The service descriptor to register
	/// </summary>
	public ServiceDescriptor? ServiceDescriptor { get; }

	/// <summary>
	/// The service descriptors to register
	/// </summary>
	public IEnumerable<ServiceDescriptor>? ServiceDescriptors { get; }

	/// <summary>
	/// An explicit SkipDuplicates value to use for this registration.
	/// <para>When left null, the default value from the registration convention will be used</para>
	/// </summary>
	public bool? SkipDuplicates { get; }

	/// <summary>
	/// An explicit SkipImplementationTypesAlreadyInUse value to use for this registration.
	/// <para>When left null, the default value from the registration convention will be used</para>
	/// </summary>
	public bool? SkipImplementationTypesAlreadyInUse { get; }
}
