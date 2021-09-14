using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public static class ServiceDescriptorExtensions
	{
		/// <summary>
		/// Similar to the internal GetImplementationType() instance method but modified to handle our subclass(es)
		/// <seealso href="https://github.com/dotnet/runtime/blob/v5.0.9/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L125"/>
		/// </summary>
		public static Type? TryGetImplementationType(this ServiceDescriptor serviceDescriptor)
		{
			if (serviceDescriptor is null)
				return null;

			if (serviceDescriptor is ICustomServiceDescriptor customServiceDescriptor)
				return customServiceDescriptor.GetImplementationType();

			if (serviceDescriptor.ImplementationType != null)
				return serviceDescriptor.ImplementationType;

			if (serviceDescriptor.ImplementationInstance != null)
				return serviceDescriptor.ImplementationInstance.GetType();

			if (serviceDescriptor.ImplementationFactory != null)
			{
				var typeArguments = serviceDescriptor.ImplementationFactory.GetType().GenericTypeArguments;
				if (typeArguments?.Length == 2)
					return typeArguments[1];
			}

			return null;
		}
	}
}
