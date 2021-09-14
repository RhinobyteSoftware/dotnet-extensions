using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	[AttributeUsage(AttributeTargets.Class)]
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
