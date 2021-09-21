using Microsoft.Extensions.DependencyInjection;
using Rhinobyte.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Custom <see cref="ServiceDescriptor"/> which accepts an explicit <see cref="ConstructorInfo"/> reference to use for the implementation type.
	/// </summary>
	public class ExplicitConstructorServiceDescriptor<TImplementationType> : ServiceDescriptor, ICustomServiceDescriptor
		where TImplementationType : class
	{
		public ExplicitConstructorServiceDescriptor(
			Type serviceType,
			ConstructorInfo explicitConstructorToUse,
			ServiceLifetime serviceLifetime)
			: base(serviceType, new ExplicitConstructorFactory<TImplementationType>(explicitConstructorToUse).FactoryMethod, serviceLifetime)
		{
			OriginalImplementationType = typeof(TImplementationType);
		}

		public Type OriginalImplementationType { get; }

		public Type GetImplementationType() => OriginalImplementationType;
	}

	/// <summary>
	/// Custom <see cref="ServiceDescriptor"/> which accepts an explicit <see cref="ConstructorInfo"/> reference to use for the implementation type.
	/// <para>
	/// This non-generic version is intended for use in registration scenarios where type discovery is performed at runtime. For design time registration use the
	/// <see cref="ExplicitConstructorServiceDescriptor{TImplementationType}"/> version.
	/// </para>
	/// </summary>
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
			: base(serviceType, new ExplicitConstructorFactory(explicitConstructorToUse).CallConstructor, serviceLifetime)
		{
			OriginalImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
		}

		public static ExplicitConstructorServiceDescriptor<TImplementationType> CreateScoped<TServiceType, TImplementationType>(ConstructorInfo constructorInfo)
			where TServiceType : class
			where TImplementationType : class, TServiceType
			=> new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), constructorInfo, ServiceLifetime.Scoped);

		public static ServiceDescriptor CreateScoped<TServiceType, TImplementationType>(ConstructorSelectionType constructorSelectionType)
			where TServiceType : class
			where TImplementationType : class, TServiceType
		{
			var constructorInfo = SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
			return constructorInfo == null
				? ServiceDescriptor.Scoped<TServiceType, TImplementationType>()
				: CreateScoped<TServiceType, TImplementationType>(constructorInfo);
		}

		public static ExplicitConstructorServiceDescriptor<TImplementationType> CreateSingleton<TServiceType, TImplementationType>(ConstructorInfo constructorInfo)
			where TServiceType : class
			where TImplementationType : class, TServiceType
			=> new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), constructorInfo, ServiceLifetime.Singleton);

		public static ServiceDescriptor CreateSingleton<TServiceType, TImplementationType>(ConstructorSelectionType constructorSelectionType)
			where TServiceType : class
			where TImplementationType : class, TServiceType
		{
			var constructorInfo = SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
			return constructorInfo == null
				? ServiceDescriptor.Singleton<TServiceType, TImplementationType>()
				: CreateSingleton<TServiceType, TImplementationType>(constructorInfo);
		}

		public static ExplicitConstructorServiceDescriptor<TImplementationType> CreateTransient<TServiceType, TImplementationType>(ConstructorInfo constructorInfo)
			where TServiceType : class
			where TImplementationType : class, TServiceType
			=> new ExplicitConstructorServiceDescriptor<TImplementationType>(typeof(TServiceType), constructorInfo, ServiceLifetime.Transient);

		public static ServiceDescriptor CreateTransient<TServiceType, TImplementationType>(ConstructorSelectionType constructorSelectionType)
			where TServiceType : class
			where TImplementationType : class, TServiceType
		{
			var constructorInfo = SelectCustomConstructor(typeof(TImplementationType), constructorSelectionType);
			return constructorInfo == null
				? ServiceDescriptor.Transient<TServiceType, TImplementationType>()
				: CreateTransient<TServiceType, TImplementationType>(constructorInfo);
		}

		public Type OriginalImplementationType { get; }


		public Type GetImplementationType() => OriginalImplementationType;

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
					constructorParameterTypes.Add(parameterInfo.ParameterType);
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
}
