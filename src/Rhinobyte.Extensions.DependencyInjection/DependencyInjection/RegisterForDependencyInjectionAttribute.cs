using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Attribute used to decorate a type that be registered against a service collection using the specified <see cref="RegisterForDependencyInjectionAttribute.ImplementationType"/>
	/// <para>
	/// The <see cref="AttributeDecoratedConvention"/> and <see cref="RhinobyteServiceCollectionExtensions.RegisterAttributeDecoratedTypes(IServiceCollection, Reflection.AssemblyScanning.IAssemblyScanResult)"/>
	/// method use this attribute to registration of types discovered via reflection
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class RegisterForDependencyInjectionAttribute : Attribute
	{
		/// <summary>
		/// Construct an instance of the attribute with the required <paramref name="implementationType"/> value.
		/// </summary>
		public RegisterForDependencyInjectionAttribute(Type implementationType)
		{
			ImplementationType = implementationType;
		}

		/// <summary>
		/// An optional attribute property to specify a <see cref="ConstructorSelectionType"/> that the <see cref="AttributeDecoratedConvention"/> should apply
		/// when constructing <see cref="ServiceDescriptor"/> instances for discovered types.
		/// </summary>
		public ConstructorSelectionType? ConstructorSelectionType { get; set; }
		/// <summary>
		/// The required implementation <see cref="Type"/> to register against the decorated type.
		/// </summary>
		public Type ImplementationType { get; }
		/// <summary>
		/// An optional attribute property to specify an explicit <see cref="ServiceLifetime"/> to use for the registration.
		/// <para>Used to supercede the <see cref="ServiceRegistrationConventionBase.DefaultLifetime"/> value.</para>
		/// </summary>
		public ServiceLifetime? ServiceLifetime { get; set; }
		/// <summary>
		/// An optional attribute property to specify an explicit <see cref="ServiceRegistrationOverwriteBehavior"/> to use for the registration.
		/// <para>Used to supercede the <see cref="ServiceRegistrationConventionBase.DefaultOverwriteBehavior"/> value.</para>
		/// </summary>
		public ServiceRegistrationOverwriteBehavior? ServiceRegistrationOverwriteBehavior { get; set; }
	}
}
