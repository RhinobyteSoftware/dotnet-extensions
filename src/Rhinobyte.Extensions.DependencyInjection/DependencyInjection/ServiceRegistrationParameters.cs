using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Wrapper type used by <see cref="ServiceRegistrationConventionBase"/> to allow convention subclasses to specify one or more service descriptors
	/// to register.
	/// <para>
	/// Subclasses implementing ServiceRegistrationConventionBase can specify override values such as <see cref="ServiceRegistrationOverwriteBehavior"/>
	/// to use for the specific registration in lieu of the convention class's default value
	/// </summary>
	[DebuggerDisplay("ServiceDescriptor = {ServiceDescriptor}, ServiceDescriptors = {ServiceDescriptors}, OverwriteBehavior = {OverwriteBehavior}, SkipDuplicates = {SkipDuplicates}, SkipImplementationTypesAlreadyInUse = {SkipImplementationTypesAlreadyInUse}")]
	public class ServiceRegistrationParameters
	{
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

		public ServiceRegistrationOverwriteBehavior? OverwriteBehavior { get; }
		public ServiceDescriptor? ServiceDescriptor { get; }
		public IEnumerable<ServiceDescriptor>? ServiceDescriptors { get; }
		public bool? SkipDuplicates { get; }
		public bool? SkipImplementationTypesAlreadyInUse { get; }
	}
}
