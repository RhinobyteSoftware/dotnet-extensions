using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.AssemblyScanning;

/// <summary>
/// Default <see cref="IAssemblyScanner"/> implementation.
/// <para>Allows configuration of assemblies, excluded types, included types, and filters used to product an <see cref="IAssemblyScanResult"/></para>
/// </summary>
public class AssemblyScanner : IAssemblyScanner
{
	private readonly HashSet<AssemblyInclude> _assembliesToScan = [];
	private IncludeExcludeConflictResolutionStrategy _currentIncludeExcludeConflictResolutionStrategy;
	private AssemblyScanResult? _currentScanResult;
	private readonly HashSet<Type> _excludedTypes = [];
	private readonly HashSet<Type> _includedTypes = [];
	private readonly HashSet<IScannedAssemblyFilter> _scannedAssemblyFilters;
	private readonly HashSet<IScannedTypeFilter> _scannedTypeFilters;

	/// <summary>
	/// Constructs a new blank assembly scanner with instantiated <see cref="HashSet{T}"/> for the filter fields
	/// </summary>
	protected AssemblyScanner()
	{
		_scannedAssemblyFilters = [];
		_scannedTypeFilters = [];
	}

	/// <summary>
	/// Constructs a new blank assembly scanner
	/// <para>Use <see cref="CreateDefault"/> to construct an instance with the default filters included</para>
	/// </summary>
	public AssemblyScanner(
		HashSet<IScannedAssemblyFilter> scannedAssemblyFilters,
		HashSet<IScannedTypeFilter> scannedTypeFilters)
	{
		_scannedAssemblyFilters = scannedAssemblyFilters ?? throw new ArgumentNullException(nameof(scannedAssemblyFilters));
		_scannedTypeFilters = scannedTypeFilters ?? throw new ArgumentNullException(nameof(scannedTypeFilters));
	}

	/// <summary>
	/// Constructs a new <see cref="AssemblyScanner"/> instance that is pre-configured with the following filters:
	/// <para><see cref="IgnoredAttributeFilter"/></para>
	/// </summary>
	public static AssemblyScanner CreateDefault()
	{
		var assemblyScanner = new AssemblyScanner();

		var ignoredAttributeFilter = new IgnoredAttributeFilter();
		return assemblyScanner
			.AddAssemblyFilter(ignoredAttributeFilter)
			.AddTypeFilter(ignoredAttributeFilter);
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<AssemblyInclude> AssembliesToScan => _assembliesToScan;
	/// <inheritdoc/>
	public IReadOnlyCollection<Type> ExplicitTypeExcludes => _excludedTypes;
	/// <inheritdoc/>
	public IReadOnlyCollection<Type> ExplicitTypeIncludes => _includedTypes;
	/// <inheritdoc/>
	public IReadOnlyCollection<IScannedAssemblyFilter> ScannedAssemblyFilters => _scannedAssemblyFilters;
	/// <inheritdoc/>
	public IReadOnlyCollection<IScannedTypeFilter> ScannedTypeFilters => _scannedTypeFilters;


	/// <summary>
	/// Adds the specified <paramref name="assembly"/> to the collection of assemblies to scan, if not already present.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner Add(Assembly assembly, bool areNonExportedTypesIncluded = false)
	{
		if (assembly is null)
			return this;

		return Add(new AssemblyInclude(assembly, areNonExportedTypesIncluded));
	}

	/// <summary>
	/// Adds the specified <paramref name="assemblyInclude"/> to the collection of assemblies to scan, if not already present.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner Add(AssemblyInclude assemblyInclude)
	{
		var hasChanged = _assembliesToScan.Add(assemblyInclude);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Adds the provided <paramref name="scannedAssemblyFilter"/> to the <see cref="ScannedAssemblyFilters"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner AddAssemblyFilter(IScannedAssemblyFilter scannedAssemblyFilter)
	{
		if (scannedAssemblyFilter is null)
			return this;

		var hasChanged = _scannedAssemblyFilters.Add(scannedAssemblyFilter);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Adds the provided <paramref name="filter"/> to the <see cref="ScannedAssemblyFilters"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner AddAssemblyFilter(Func<AssemblyInclude, IAssemblyScanner, IAssemblyScanResult, bool> filter)
		=> AddAssemblyFilter(new LambdaAssemblyFilter(filter));

	/// <summary>
	/// Adds the assembly for the specified <paramref name="typeInAssembly"/> to the assemblies to scan, if not already present.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner AddForType(Type typeInAssembly, bool areNonExportedTypesIncluded = false)
		=> Add(typeInAssembly?.Assembly!, areNonExportedTypesIncluded);

	/// <summary>
	/// Adds the assembly for the specified <typeparamref name="TType"/> to the assemblies to scan, if not already present.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner AddForType<TType>(bool areNonExportedTypesIncluded = false)
		=> AddForType(typeof(TType), areNonExportedTypesIncluded);

	/// <summary>
	/// Adds the provided <paramref name="scannedTypeFilter"/> to the <see cref="ScannedTypeFilters"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner AddTypeFilter(IScannedTypeFilter scannedTypeFilter)
	{
		if (scannedTypeFilter is null)
			return this;

		var hasChanged = _scannedTypeFilters.Add(scannedTypeFilter);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Adds the provided <paramref name="filter"/> to the <see cref="ScannedTypeFilters"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner AddTypeFilter(Func<AssemblyInclude, Type, IAssemblyScanner, IAssemblyScanResult, bool> filter)
		=> AddTypeFilter(new LambdaTypeFilter(filter));

	/// <summary>
	/// Find and return the registered <see cref="AssemblyInclude"/> that matches <paramref name="assemblyToLookFor"/>, if it exists
	/// </summary>
	public AssemblyInclude? FindAssemblyInclude(Assembly assemblyToLookFor)
	{
		foreach (var assemblyInclude in _assembliesToScan)
		{
			if (assemblyInclude.AssemblyToInclude == assemblyToLookFor)
				return assemblyInclude;
		}

		return null;
	}

	/// <summary>
	/// Adds the <typeparamref name="TType"/> type to the <see cref="ExplicitTypeExcludes"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner ExcludeType<TType>()
		=> ExcludeType(typeof(TType));

	/// <summary>
	/// Adds the <paramref name="typeToExclude"/> type to the <see cref="ExplicitTypeExcludes"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner ExcludeType(Type typeToExclude)
	{
		var hasChanged = _excludedTypes.Add(typeToExclude);

		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Adds the <paramref name="typesToExclude"/> type to the <see cref="ExplicitTypeExcludes"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner ExcludeTypes(IEnumerable<Type> typesToExclude)
	{
		if (typesToExclude != null)
		{
			foreach (var typeToExclude in typesToExclude)
				_ = ExcludeType(typeToExclude);
		}

		return this;
	}

	/// <summary>
	/// Adds the <typeparamref name="TType"/> type to the <see cref="ExplicitTypeIncludes"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner IncludeType<TType>()
		=> IncludeType(typeof(TType));

	/// <summary>
	/// Adds the <paramref name="typeToInclude"/> to the <see cref="ExplicitTypeIncludes"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner IncludeType(Type typeToInclude)
	{
		if (typeToInclude is null)
			return this;

		var hasChanged = _includedTypes.Add(typeToInclude);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Adds the <paramref name="typesToInclude"/> to the <see cref="ExplicitTypeIncludes"/> collection.
	/// <para>If the registration state changes, the cached scan result is cleared if there is one</para>
	/// </summary>
	public AssemblyScanner IncludeTypes(IEnumerable<Type> typesToInclude)
	{
		if (typesToInclude != null)
		{
			foreach (var typeToInclude in typesToInclude)
				_ = IncludeType(typeToInclude);
		}

		return this;
	}

	/// <summary>
	/// Remove the <paramref name="assembly"/> from the registered assemblies to scan.
	/// <para>If assembly is removed and there is a cached scan result, the cached scan result is cleared</para>
	/// </summary>
	public AssemblyScanner Remove(Assembly assembly)
	{
		if (assembly is null)
			return this;

		var matchedAssemblyInclude = FindAssemblyInclude(assembly);
		if (matchedAssemblyInclude is null)
			return this;

		return Remove(matchedAssemblyInclude.Value);
	}

	/// <summary>
	/// Remove the <paramref name="assemblyInclude"/> from the registered assemblies to scan.
	/// <para>If assemblyInclude is removed and there is a cached scan result, the cached scan result is cleared</para>
	/// </summary>
	public AssemblyScanner Remove(AssemblyInclude assemblyInclude)
	{
		var hasChanged = _assembliesToScan.Remove(assemblyInclude);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Remove the <paramref name="scannedAssemblyFilter"/> from the registered filters.
	/// <para>If scannedAssemblyFilter is removed and there is a cached scan result, the cached scan result is cleared</para>
	/// </summary>
	public AssemblyScanner RemoveAssemblyFilter(IScannedAssemblyFilter scannedAssemblyFilter)
	{
		if (scannedAssemblyFilter is null)
			return this;

		var hasChanged = _scannedAssemblyFilters.Remove(scannedAssemblyFilter);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Remove the <paramref name="scannedTypeFilter"/> from the registered filters.
	/// <para>If scannedTypeFilter is removed and there is a cached scan result, the cached scan result is cleared</para>
	/// </summary>
	public AssemblyScanner RemoveTypeFilter(IScannedTypeFilter scannedTypeFilter)
	{
		if (scannedTypeFilter is null)
			return this;

		var hasChanged = _scannedTypeFilters.Remove(scannedTypeFilter);
		if (hasChanged)
			_currentScanResult = null;

		return this;
	}

	/// <summary>
	/// Scans the registered assemblies and return the generated <see cref="IAssemblyScanResult"/>
	/// </summary>
	/// <param name="includeExcludeConflictResolutionStrategy">
	/// The resolution strategy to apply if there is overlap
	/// between the <see cref="ExplicitTypeExcludes"/> and <see cref="ExplicitTypeIncludes"/> collections.
	/// </param>
	/// <returns>The cached scan result or a new scan result if one is not already cached.</returns>
	/// <remarks>
	/// Internally this implementation caches the scan result and returns the cached object on subsequent calls to this method.
	/// <para>Calls to any of the methods that change the scanner configuration will clear the cached result object.</para>
	/// </remarks>
	public IAssemblyScanResult ScanAssemblies(
		IncludeExcludeConflictResolutionStrategy includeExcludeConflictResolutionStrategy = IncludeExcludeConflictResolutionStrategy.PrioritizeExcludes)
	{
		if (_currentScanResult != null && _currentIncludeExcludeConflictResolutionStrategy == includeExcludeConflictResolutionStrategy)
			return _currentScanResult;

		var newScanResult = new AssemblyScanResult();

		var duplicateIncludeExcludeTypes = new List<string>();

		foreach (var explicitlyIncludedType in _includedTypes)
		{
			if (_excludedTypes.Contains(explicitlyIncludedType))
			{
				if (includeExcludeConflictResolutionStrategy == IncludeExcludeConflictResolutionStrategy.PrioritizeExcludes)
				{
					_ = newScanResult.IgnoredTypes.Add(explicitlyIncludedType);
					continue;
				}

				if (includeExcludeConflictResolutionStrategy == IncludeExcludeConflictResolutionStrategy.ThrowException)
				{
					duplicateIncludeExcludeTypes.Add(explicitlyIncludedType.FullName ?? explicitlyIncludedType.ToString());
					continue;
				}
			}

			SetResultTypes(newScanResult, explicitlyIncludedType);
		}

		if (duplicateIncludeExcludeTypes.Count > 0)
			throw new InvalidOperationException($"{nameof(ScanAssemblies)} cannot complete because the following types exist in both the explicit include and explicit exclude collections:{Environment.NewLine}  {string.Join(", ", duplicateIncludeExcludeTypes)}");

		foreach (var assemblyInclude in _assembliesToScan)
		{
			var ignoreAssembly = false;
			foreach (var assemblyFilter in _scannedAssemblyFilters)
			{
				if (assemblyFilter.ShouldIgnoreAssembly(assemblyInclude, this, newScanResult))
				{
					ignoreAssembly = true;
					break;
				}
			}

			if (ignoreAssembly)
			{
				_ = newScanResult.IgnoredAssemblies.Add(assemblyInclude);
				continue;
			}

			_ = newScanResult.ScannedAssemblies.Add(assemblyInclude);
			var discoveredTypes = assemblyInclude.AreNonExportedTypesIncluded
				? assemblyInclude.AssemblyToInclude.GetTypes()
				: assemblyInclude.AssemblyToInclude.GetExportedTypes();

			foreach (var discoveredType in discoveredTypes)
			{
				if (discoveredType.IsCompilerGenerated())
					continue;

				if (_excludedTypes.Contains(discoveredType))
				{
					_ = newScanResult.IgnoredTypes.Add(discoveredType);
					continue;
				}

				var isIgnoredType = false;
				foreach (var typeFilter in _scannedTypeFilters)
				{
					if (typeFilter.ShouldIgnoreType(assemblyInclude, discoveredType, this, newScanResult))
					{
						isIgnoredType = true;
						break;
					}
				}

				if (isIgnoredType)
				{
					_ = newScanResult.IgnoredTypes.Add(discoveredType);
					continue;
				}

				SetResultTypes(newScanResult, discoveredType);
			}
		}

		_currentIncludeExcludeConflictResolutionStrategy = includeExcludeConflictResolutionStrategy;
		_currentScanResult = newScanResult;

		return _currentScanResult;
	}

	private static void SetResultTypes(AssemblyScanResult scanResult, Type scannedType)
	{
		_ = scanResult.AllDiscoveredTypes.Add(scannedType);

		if (scannedType.IsInterface)
			_ = scanResult.InterfaceTypes.Add(scannedType);

		if (scannedType.IsClass && !scannedType.IsAbstract)
			_ = scanResult.ConcreteTypes.Add(scannedType);

		if (scannedType.IsOpenGeneric())
			_ = scanResult.OpenGenericTypes.Add(scannedType);
	}
}
