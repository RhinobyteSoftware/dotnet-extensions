﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
#if NETFRAMEWORK || NETSTANDARD2_0
using System.Runtime.ExceptionServices;
#endif

namespace Rhinobyte.Extensions.DependencyInjection;

/// <summary>
/// Constructor factory used by <see cref="ExplicitConstructorFactory{TImplementationType}"/>
/// </summary>
/// <typeparam name="TImplementationType">The implementation type returned by the factory</typeparam>
public class ExplicitConstructorFactory<TImplementationType>
	where TImplementationType : class
{
	private readonly ConstructorInfo _explicitConstructorToUse;

	/// <summary>
	/// Construct a factory instance for the provided <paramref name="explicitConstructorToUse"/>
	/// </summary>
	public ExplicitConstructorFactory(ConstructorInfo explicitConstructorToUse)
	{
		_explicitConstructorToUse = explicitConstructorToUse ?? throw new ArgumentNullException(nameof(explicitConstructorToUse));
	}

	/// <summary>
	/// See the <see cref="Func{T, TResult}"/> to use for the <see cref="ServiceDescriptor.ImplementationFactory"/>
	/// </summary>
	public Func<IServiceProvider, TImplementationType> FactoryMethod => CallConstructor;

	/// <summary>
	/// The factory method that will call the explicitly selected constructor and return the constructed instance of <typeparamref name="TImplementationType"/>
	/// </summary>
	public TImplementationType CallConstructor(IServiceProvider serviceProvider)
	{
		var parameterDetails = _explicitConstructorToUse.GetParameters();
		var parameterValues = new object?[parameterDetails.Length];
		for (var parameterIndex = 0; parameterIndex < parameterDetails.Length; ++parameterIndex)
		{
			var currentParameter = parameterDetails[parameterIndex];
			parameterValues[parameterIndex] = currentParameter.IsOptional
#pragma warning disable CA1062 // Validate arguments of public methods
							? serviceProvider.GetService(currentParameter.ParameterType)
#pragma warning restore CA1062 // Validate arguments of public methods
							: serviceProvider.GetRequiredService(currentParameter.ParameterType);
		}

		// As closely as possible, mirror how the OOTB dependency injection calls the constructor...
		// See https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.DependencyInjection/src/ServiceLookup/CallSiteRuntimeResolver.cs#L56
#if NETFRAMEWORK || NETSTANDARD2_0
		try
		{
			return (TImplementationType)_explicitConstructorToUse.Invoke(parameterValues);
		}
		catch (Exception ex) when (ex.InnerException != null)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			// The above line will always throw, but the compiler requires we throw explicitly.
			throw;
		}
#else
		return (TImplementationType)_explicitConstructorToUse.Invoke(BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameterValues, culture: null);
#endif
	}
}
