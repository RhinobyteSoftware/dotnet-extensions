﻿using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Custom <see cref="ServiceDescriptor"/> which accepts an explicit <see cref="ConstructorInfo"/> reference to use for the implementation type.
/// </summary>
public class ExplicitConstructorServiceDescriptor<TImplementationType> : ServiceDescriptor, ICustomServiceDescriptor
	where TImplementationType : class
{
	/// <summary>
	/// Constructor an <see cref="ExplicitConstructorFactory{TImplementationType}"/> that will use an <see cref="ServiceDescriptor.ImplementationFactory"/>
	/// that calls the provided <paramref name="explicitConstructorToUse"/> when constructing an instance of <typeparamref name="TImplementationType"/>.
	/// </summary>
	public ExplicitConstructorServiceDescriptor(
		Type serviceType,
		ConstructorInfo explicitConstructorToUse,
		ServiceLifetime serviceLifetime)
		: base(serviceType, new ExplicitConstructorFactory<TImplementationType>(explicitConstructorToUse).FactoryMethod, serviceLifetime)
	{
		OriginalImplementationType = typeof(TImplementationType);
	}

	/// <summary>
	/// The <typeparamref name="TImplementationType"/> of the instance that the explicit constructor factory will return.
	/// </summary>
	public Type OriginalImplementationType { get; }

#pragma warning disable CA1721 // Property names should not match get methods	- Reason: The property name ImplementationType in the base ServiceDescriptor class can be null when an explicit implementation or factory is used
	/// <inheritdoc/>
	public Type GetImplementationType() => OriginalImplementationType;
#pragma warning restore CA1721 // Property names should not match get methods
}

/// <summary>
/// Static helper methods type for the generic <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/> class
/// </summary>
public static class ExplicitConstructorServiceDescriptor
{
	/// <summary>
	/// Uses reflection to create the service descriptor/factory function with a generic argument return type of <paramref name="implementationType"/>
	/// </summary>
	/// <param name="serviceType">The <see cref="ServiceDescriptor.ServiceType"/> value</param>
	/// <param name="implementationType">The implementation type returned by the factory</param>
	/// <param name="explicitConstructorToUse">The explicit constructor to use for the service provider factory</param>
	/// <param name="serviceLifetime">The <see cref="ServiceDescriptor.Lifetime"/> value</param>
	/// <remarks>
	/// We want to make the factory actually be of type Func&lt;IServiceProvider, TImplementationType&gt; or else the Microsoft.Extension.DependencyInjection
	/// library will behave unexpectly in certain cases.
	/// <para>
	/// This happens because calls to the internal only ServiceDescriptor.GetImplementationType() method
	/// implicitly assumes the factory will have an explicitly typed return type argument when it can in fact return object.
	/// </para>
	/// <para><seealso href="https://github.com/dotnet/runtime/blob/v5.0.9/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L137" /></para>
	/// <para><seealso href="https://github.com/dotnet/extensions/blob/v3.1.19/src/DependencyInjection/DI.Abstractions/src/ServiceDescriptor.cs#L140"/></para>
	/// </remarks>
	public static ServiceDescriptor Create(Type serviceType, Type implementationType, ConstructorInfo explicitConstructorToUse, ServiceLifetime serviceLifetime)
	{
		_ = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
		_ = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
		_ = explicitConstructorToUse ?? throw new ArgumentNullException(nameof(explicitConstructorToUse));

		if (explicitConstructorToUse.DeclaringType != implementationType)
			throw new ArgumentException($"{nameof(explicitConstructorToUse)}.{nameof(explicitConstructorToUse.DeclaringType)} of {explicitConstructorToUse.DeclaringType} does not match the implementation type {implementationType}");

		var descriptorType = typeof(ExplicitConstructorServiceDescriptor<>).MakeGenericType(implementationType);
		var descriptorConstructor = descriptorType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
		var descriptorInstance = (ServiceDescriptor)descriptorConstructor.Invoke([serviceType, explicitConstructorToUse, serviceLifetime]);
		return descriptorInstance;
	}

	/// <summary>
	/// Convenience method to construct a <see cref="ServiceLifetime.Scoped"/> descriptor that explicitly calls the provided <paramref name="constructorInfo"/>
	/// </summary>
	public static ExplicitConstructorServiceDescriptor<TImplementationType> CreateScoped<TServiceType, TImplementationType>(ConstructorInfo constructorInfo)
		where TServiceType : class
		where TImplementationType : class, TServiceType
		=> new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), constructorInfo, ServiceLifetime.Scoped);

	/// <summary>
	/// Convenience method to construct a <see cref="ServiceLifetime.Scoped"/> descriptor that will use an explicitly selected constructor based
	/// on the <paramref name="constructorSelectionType"/>.
	/// </summary>
	public static ServiceDescriptor CreateScoped<TServiceType, TImplementationType>(ConstructorSelectionType constructorSelectionType)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		var constructorInfo = SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		return constructorInfo is null
			? ServiceDescriptor.Scoped<TServiceType, TImplementationType>()
			: CreateScoped<TServiceType, TImplementationType>(constructorInfo);
	}

	/// <summary>
	/// Convenience method to construct a <see cref="ServiceLifetime.Singleton"/> descriptor that explicitly calls the provided <paramref name="constructorInfo"/>
	/// </summary>
	public static ExplicitConstructorServiceDescriptor<TImplementationType> CreateSingleton<TServiceType, TImplementationType>(ConstructorInfo constructorInfo)
		where TServiceType : class
		where TImplementationType : class, TServiceType
		=> new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), constructorInfo, ServiceLifetime.Singleton);

	/// <summary>
	/// Convenience method to construct a <see cref="ServiceLifetime.Singleton"/> descriptor that will use an explicitly selected constructor based
	/// on the <paramref name="constructorSelectionType"/>.
	/// </summary>
	public static ServiceDescriptor CreateSingleton<TServiceType, TImplementationType>(ConstructorSelectionType constructorSelectionType)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		var constructorInfo = SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		return constructorInfo is null
			? ServiceDescriptor.Singleton<TServiceType, TImplementationType>()
			: CreateSingleton<TServiceType, TImplementationType>(constructorInfo);
	}

	/// <summary>
	/// Convenience method to construct a <see cref="ServiceLifetime.Transient"/> descriptor that explicitly calls the provided <paramref name="constructorInfo"/>
	/// </summary>
	public static ExplicitConstructorServiceDescriptor<TImplementationType> CreateTransient<TServiceType, TImplementationType>(ConstructorInfo constructorInfo)
		where TServiceType : class
		where TImplementationType : class, TServiceType
		=> new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), constructorInfo, ServiceLifetime.Transient);

	/// <summary>
	/// Convenience method to construct a <see cref="ServiceLifetime.Transient"/> descriptor that will use an explicitly selected constructor based
	/// on the <paramref name="constructorSelectionType"/>.
	/// </summary>
	public static ServiceDescriptor CreateTransient<TServiceType, TImplementationType>(ConstructorSelectionType constructorSelectionType)
		where TServiceType : class
		where TImplementationType : class, TServiceType
	{
		var constructorInfo = SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
		return constructorInfo is null
			? ServiceDescriptor.Transient<TServiceType, TImplementationType>()
			: CreateTransient<TServiceType, TImplementationType>(constructorInfo);
	}

	/// <summary>
	/// Potentially returns an explicit <see cref="ConstructorInfo"/> to use for constructing instances of <paramref name="implementationType"/>.
	/// <para>
	/// The constructor selection is determined based on a number of factors including the <paramref name="constructorSelectionType"/>, the number
	/// of available constructors, the constructor parameter types, the presence of <see cref="DependencyInjectionConstructorAttribute"/>
	/// and whether or not the constructor dependencies are potentially ambiguous
	/// </para>
	/// </summary>
	public static ConstructorInfo? SelectCustomConstructor(Type implementationType, ConstructorSelectionType constructorSelectionType)
	{
		_ = implementationType ?? throw new ArgumentNullException(nameof(implementationType));

		var lookForAttribute = false;
		var lookForMostParameters = false;
		var onlyUseMostParametersWhenAmbiguous = false;

		switch (constructorSelectionType)
		{
			case ConstructorSelectionType.AttributeThenDefaultBehavior:
				lookForAttribute = true;
				break;

			case ConstructorSelectionType.AttributeThenMostParametersOnly:
				lookForAttribute = true;
				lookForMostParameters = true;
				break;

			case ConstructorSelectionType.AttributeThenMostParametersWhenAmbiguous:
				lookForAttribute = true;
				lookForMostParameters = true;
				onlyUseMostParametersWhenAmbiguous = true;
				break;

			case ConstructorSelectionType.DefaultBehaviorOnly:
				return null;

			case ConstructorSelectionType.MostParametersOnly:
				lookForMostParameters = true;
				break;

			case ConstructorSelectionType.MostParametersWhenAmbiguous:
				lookForMostParameters = true;
				onlyUseMostParametersWhenAmbiguous = true;
				break;

			default:
				throw new NotImplementedException($"{nameof(SelectCustomConstructor)} is not implemented for the {nameof(ConstructorSelectionType)} value of {constructorSelectionType}");
		}


		var potentialConstructors = new List<ConstructorInfo>();
		var attributeDecoratedConstructors = lookForAttribute
			? new List<ConstructorInfo>()
			: null;

		foreach (var constructor in implementationType.GetTypeInfo().DeclaredConstructors)
		{
			if (!constructor.IsPublic)
				continue;

			var hasInvalidParameterType = false;
			foreach (var constructorParameter in constructor.GetParameters())
			{
				if (!constructorParameter.IsOptional && constructorParameter.ParameterType.IsValueTypeOrString())
				{
					hasInvalidParameterType = true;
					break;
				}
			}

			if (lookForAttribute && constructor.IsDefined(typeof(DependencyInjectionConstructorAttribute), false))
			{
				if (hasInvalidParameterType)
					throw new ConstructorSelectionFailedException($"{implementationType.FullName} has a constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)} that requires value type/string parameters that won't be injectable");

				attributeDecoratedConstructors!.Add(constructor);
			}

			if (hasInvalidParameterType)
				continue;

			potentialConstructors.Add(constructor);
		}

		if (potentialConstructors.Count < 2)
			return null; // No need to use an ExplicitConstructorDescriptor

		if (lookForAttribute)
		{
			if (attributeDecoratedConstructors!.Count > 1)
				throw new ConstructorSelectionFailedException($"{implementationType.FullName} has multiple constructors decorated with the {nameof(DependencyInjectionConstructorAttribute)}");

			if (attributeDecoratedConstructors.Count == 1)
				return attributeDecoratedConstructors[0];
		}

		if (lookForMostParameters)
		{
			potentialConstructors.Sort((a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

			var mostParametersConstructor = potentialConstructors[0];
			if (mostParametersConstructor.GetParameters().Length == potentialConstructors[1].GetParameters().Length)
				return null; // 2+ constructors with the most parameters, use the default behavior since it will try to decide based on what it can actually resolve

			if (!onlyUseMostParametersWhenAmbiguous)
				return mostParametersConstructor;

			var constructorParameterTypes = new HashSet<Type>();
			foreach (var parameterInfo in mostParametersConstructor.GetParameters())
			{
				_ = constructorParameterTypes.Add(parameterInfo.ParameterType);
			}

			for (var constructorIndex = 1; constructorIndex < potentialConstructors.Count; ++constructorIndex)
			{
				var currentConstructorToCheck = potentialConstructors[constructorIndex];
				foreach (var parameterInfo in currentConstructorToCheck.GetParameters())
				{
					if (!constructorParameterTypes.Contains(parameterInfo.ParameterType))
					{
						// ambiguous constructors found, return the mostParametersConstructor
						return mostParametersConstructor;
					}
				}
			}
		}

		return null;
	}
}
