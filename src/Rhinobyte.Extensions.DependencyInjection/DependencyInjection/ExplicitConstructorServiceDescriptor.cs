using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public class ExplicitConstructorServiceDescriptor<TImplementationType> : ExplicitConstructorServiceDescriptor
		where TImplementationType : class
	{
		public ExplicitConstructorServiceDescriptor(
			Type serviceType,
			ConstructorInfo explicitConstructorToUse,
			ServiceLifetime serviceLifetime)
			: base(serviceType, typeof(TImplementationType), new ExplicitConstructorFactory<TImplementationType>(explicitConstructorToUse).CallConstructor, serviceLifetime)
		{
		}
	}

	public class ExplicitConstructorServiceDescriptor : ServiceDescriptor
	{
		public ExplicitConstructorServiceDescriptor(
			Type serviceType,
#if NET5_0_OR_GREATER
			[DynamicallyAccessedMembersAttribute(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
#else
			Type implementationType,
#endif
			ConstructorInfo explicitConstructorToUse,
			ServiceLifetime serviceLifetime)
			: base(serviceType, CreateConstructorFactory(explicitConstructorToUse, implementationType), serviceLifetime)
		{
			OriginalImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
		}

		protected ExplicitConstructorServiceDescriptor(
			Type serviceType,
			Type implementationType,
			Func<IServiceProvider, object> constructorFactory,
			ServiceLifetime serviceLifetime)
			: base(serviceType, constructorFactory, serviceLifetime)
		{
			OriginalImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
		}

		public Type OriginalImplementationType { get; }

		/// <summary>
		/// Uses reflection to create the factory function with a generic argument return type of <paramref name="implementationType"/>
		/// </summary>
		/// <param name="explicitConstructorToUse">The explicit constructor to use for the service provider factory</param>
		/// <param name="implementationType">The implementation type returned by the factory</param>
		/// <remarks>
		/// We have to make the factory actually be of type Func&lt;IServiceProvider, TImplementationType&gt; or else the Microsoft.Extension.DependencyInjection
		/// library will throw in certain cases. This happens because calls to the internal only ServiceDescriptor.GetImplementationType() method assumes the
		/// factory will have an explicitly typed return type argument, despite the constructor signature excepting the generic object return type for the function.
		/// <seealso href="https://github.com/dotnet/runtime/blob/v5.0.9/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L137" />
		/// <seealso href="https://github.com/dotnet/extensions/blob/v3.1.19/src/DependencyInjection/DI.Abstractions/src/ServiceDescriptor.cs#L140"/>
		/// </remarks>
		public static Func<IServiceProvider, object> CreateConstructorFactory(ConstructorInfo explicitConstructorToUse, Type implementationType)
		{
			var factoryClosedType = typeof(ExplicitConstructorFactory<>).MakeGenericType(implementationType);
			var factoryConstructor = factoryClosedType.GetConstructors(BindingFlags.Public).Single();
			var factoryInstance = (IExplicitConstructorFactory)factoryConstructor.Invoke(new object[] { explicitConstructorToUse });
			return factoryInstance.Factory;
		}
	}
}
