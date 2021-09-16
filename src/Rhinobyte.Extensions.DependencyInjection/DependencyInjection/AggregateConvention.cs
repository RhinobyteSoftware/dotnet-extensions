using System;
using System.Collections.Generic;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public class AggregateConvention : IServiceRegistrationConvention
	{
		private readonly IEnumerable<IServiceRegistrationConvention> _conventions;

		/// <summary>
		/// Construct an AggregateConvention that will check the provided child conventions in order.
		/// </summary>
		/// <param name="conventions">The set of conventions to try</param>
		/// <param name="skipAlreadyRegistered">When true, the discovered type will be skipped right away if there is an existing registration for it</param>
		/// <param name="tryAllConventions">When true, all of the child conventions will be checked even if a previous convention already returned true for handling the type</param>
		public AggregateConvention(
			IEnumerable<IServiceRegistrationConvention> conventions,
			bool skipAlreadyRegistered = true,
			bool tryAllConventions = false)
		{
			_conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
			SkipAlreadyRegistered = skipAlreadyRegistered;
			TryAllConventions = tryAllConventions;
		}

		/// <summary>
		/// When true the <see cref="HandleType(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> method will short circuit and return true
		/// if the service collection contains any items where the <see cref="ServiceDescriptor.ServiceType"/> matches the discovered type.
		/// This happens before any of the child conventions are checked
		/// <para>
		/// Defaults to true
		/// </para>
		/// </summary>
		public bool SkipAlreadyRegistered { get; protected set; }

		/// <summary>
		/// When true <see cref="IServiceRegistrationConvention.HandleType(Type, IAssemblyScanResult, ServiceRegistrationCache)"/> will be checked for all of the
		/// child conventions, even if a previous convention returned true
		/// <para>
		/// Defaults to false
		/// </para>
		/// </summary>
		public bool TryAllConventions { get; protected set; }

		public bool HandleType(Type discoveredType, IAssemblyScanResult scanResult, ServiceRegistrationCache serviceRegistrationCache)
		{
			_ = discoveredType ?? throw new ArgumentNullException(nameof(discoveredType));
			_ = scanResult ?? throw new ArgumentNullException(nameof(scanResult));
			_ = serviceRegistrationCache ?? throw new ArgumentNullException(nameof(serviceRegistrationCache));

			if (SkipAlreadyRegistered && serviceRegistrationCache.HasAnyByServiceType(discoveredType))
				return false;

			var wasHandled = false;
			foreach (var convention in _conventions)
			{
				wasHandled |= convention.HandleType(discoveredType, scanResult, serviceRegistrationCache);
				if (wasHandled && !TryAllConventions)
					return true;
			}

			return wasHandled;
		}
	}
}
