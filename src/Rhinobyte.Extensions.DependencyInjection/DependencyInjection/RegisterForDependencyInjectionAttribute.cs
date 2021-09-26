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
	public class RegisterForDependencyInjectionAttribute : Attribute
	{
		public RegisterForDependencyInjectionAttribute(Type implementationType)
		{
			ImplementationType = implementationType;
		}

		public ConstructorSelectionType? ConstructorSelectionType { get; set; }
		public Type ImplementationType { get; set; }
		public ServiceLifetime? ServiceLifetime { get; set; }
		public ServiceRegistrationOverwriteBehavior? ServiceRegistrationOverwriteBehavior { get; set; }
	}
}
