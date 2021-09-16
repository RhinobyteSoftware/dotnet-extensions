using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Wrapper type used by <see cref="ServiceRegistrationConventionBase"/> to allow convention subclasses to specify
	/// a <see cref="ServiceRegistrationOverwriteBehavior"/> in lieu of the convention's default for a specific <see cref="ServiceDescriptor"/>
	/// </summary>
	[DebuggerDisplay("ServiceDescriptors = {ServiceDescriptors}, OverwriteBehavior = {OverwriteBehavior}")]
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
