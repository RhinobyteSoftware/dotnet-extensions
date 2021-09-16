using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public class ExplicitConstructorFactory
	{
		private readonly ConstructorInfo _explicitConstructorToUse;

		public ExplicitConstructorFactory(ConstructorInfo explicitConstructorToUse)
		{
			_explicitConstructorToUse = explicitConstructorToUse ?? throw new ArgumentNullException(nameof(explicitConstructorToUse));
		}

		public object CallConstructor(IServiceProvider serviceProvider)
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
				return _explicitConstructorToUse.Invoke(parameterValues);
			}
			catch (Exception ex) when (ex.InnerException != null)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				// The above line will always throw, but the compiler requires we throw explicitly.
				throw;
			}
#else
			return _explicitConstructorToUse.Invoke(BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameterValues, culture: null);
#endif
		}
	}


	public class ExplicitConstructorFactory<TImplementationType>
		where TImplementationType : class
	{
		private readonly ConstructorInfo _explicitConstructorToUse;

		public ExplicitConstructorFactory(ConstructorInfo explicitConstructorToUse)
		{
			_explicitConstructorToUse = explicitConstructorToUse ?? throw new ArgumentNullException(nameof(explicitConstructorToUse));
		}

		public Func<IServiceProvider, TImplementationType> FactoryMethod => CallConstructor;

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

		/// <summary>
		/// Uses reflection to create the factory function with a generic argument return type of <paramref name="implementationType"/>
		/// Requires the use of the Microsoft.CSharp package and the dynamic keyword to bind the factory type at runtime
		/// </summary>
		/// <param name="explicitConstructorToUse">The explicit constructor to use for the service provider factory</param>
		/// <param name="implementationType">The implementation type returned by the factory</param>
		/// <remarks>
		/// We have to make the factory actually be of type Func&lt;IServiceProvider, TImplementationType&gt; or else the Microsoft.Extension.DependencyInjection
		/// library will behave unexpectly in certain cases. This happens because calls to the internal only ServiceDescriptor.GetImplementationType() method
		/// implicitly assumes the factory will have an explicitly typed return type argument when it can in fact return object.
		/// <seealso href="https://github.com/dotnet/runtime/blob/v5.0.9/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L137" />
		/// <seealso href="https://github.com/dotnet/extensions/blob/v3.1.19/src/DependencyInjection/DI.Abstractions/src/ServiceDescriptor.cs#L140"/>
		/// </remarks>
		//public static Func<IServiceProvider, object> CreateConstructorFactory(ConstructorInfo explicitConstructorToUse, Type implementationType)
		//{
		//	var factoryClosedType = typeof(ExplicitConstructorFactory<>).MakeGenericType(implementationType);
		//	var factoryConstructor = factoryClosedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
		//	dynamic factoryInstance = factoryConstructor.Invoke(new object[] { explicitConstructorToUse });
		//	return factoryInstance.FactoryMethod;
		//}
	}

}
