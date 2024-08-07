﻿using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Linq;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class ServiceRegistrationConventionBaseTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void BuildServiceDescriptor_returns_the_expected_result()
	{
		var lifetime = ServiceLifetime.Singleton;
		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		var systemUnderTest = new ServiceRegistrationConventionBaseSubclass();
		var testType = typeof(ClassWithAmbiguousConstructorDependenciesDecorated);

		// DefaultBehavior should return a plain ServiceDescriptor
		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.DefaultBehaviorOnly, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ServiceDescriptor>()
			.And.NotBeAssignableTo<ExplicitConstructorServiceDescriptor<ClassWithAmbiguousConstructorDependenciesDecorated>>();

		systemUnderTest.BuildServiceDescriptor(typeof(ClassWithConstructorSelectionAttributeTwoConstructors), typeof(ClassWithConstructorSelectionAttributeTwoConstructors), serviceRegistrationCache, ConstructorSelectionType.AttributeThenDefaultBehavior, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ServiceDescriptor>()
			.And.NotBeAssignableTo<ExplicitConstructorServiceDescriptor<ClassWithConstructorSelectionAttributeTwoConstructors>>()
			.And.Match<ServiceDescriptor>(descriptor => descriptor.Lifetime == lifetime);

		// The rest should return an ExplicitConstructorServiceDescriptor for this test type
		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.AttributeThenDefaultBehavior, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ExplicitConstructorServiceDescriptor<ClassWithAmbiguousConstructorDependenciesDecorated>>()
			.And.Match<ServiceDescriptor>(descriptor => descriptor.Lifetime == lifetime);

		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.AttributeThenMostParametersOnly, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ExplicitConstructorServiceDescriptor<ClassWithAmbiguousConstructorDependenciesDecorated>>()
			.And.Match<ServiceDescriptor>(descriptor => descriptor.Lifetime == lifetime);

		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.AttributeThenMostParametersWhenAmbiguous, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ExplicitConstructorServiceDescriptor<ClassWithAmbiguousConstructorDependenciesDecorated>>()
			.And.Match<ServiceDescriptor>(descriptor => descriptor.Lifetime == lifetime);

		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.MostParametersOnly, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ExplicitConstructorServiceDescriptor<ClassWithAmbiguousConstructorDependenciesDecorated>>()
			.And.Match<ServiceDescriptor>(descriptor => descriptor.Lifetime == lifetime);

		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.MostParametersWhenAmbiguous, lifetime)
			.Should().NotBeNull()
			.And.BeOfType<ExplicitConstructorServiceDescriptor<ClassWithAmbiguousConstructorDependenciesDecorated>>()
			.And.Match<ServiceDescriptor>(descriptor => descriptor.Lifetime == lifetime);

		// Should return null when set to skip implementation types already in use
		serviceRegistrationCache.AddTransient<ClassWithAmbiguousConstructorDependenciesDecorated>();
		systemUnderTest = new ServiceRegistrationConventionBaseSubclass(skipImplementationTypesAlreadyInUse: true);
		systemUnderTest.BuildServiceDescriptor(testType, testType, serviceRegistrationCache, ConstructorSelectionType.MostParametersWhenAmbiguous, lifetime)
			.Should().BeNull();
	}

	[TestMethod]
	public void HandleType_throws_ArgumentNullException_for_null_arguments_that_are_required()
	{
		var discoveredType = typeof(ISomethingOptions);
		var scanResult = new AssemblyScanResult();
		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

		var systemUnderTest = new ServiceRegistrationConventionBaseSubclass();

		Invoking(() => systemUnderTest.HandleType(discoveredType: null!, scanResult, serviceRegistrationCache))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*discoveredType*");

		Invoking(() => systemUnderTest.HandleType(discoveredType, scanResult: null!, serviceRegistrationCache))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanResult*");

		Invoking(() => systemUnderTest.HandleType(discoveredType, scanResult, serviceRegistrationCache: null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationCache*");
	}

	[TestMethod]
	public void ServiceRegistrationParameters_values_supercede_property_values()
	{
		var scanResult = new AssemblyScanResult();
		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());
		serviceRegistrationCache.AddScoped<SomethingOptions>();

		var systemUnderTest = new ServiceRegistrationConventionBaseSubclass();
		systemUnderTest.SetMockServiceRegistrationParameters(new ServiceRegistrationParameters(ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>()));

		// Should not register because the SkipAlreadyInUseImplementationType value is true
		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache).Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(1);

		// Setting skipImplementationTypesAlreadyInUse to false on the params should supercede the property value
		systemUnderTest.SetMockServiceRegistrationParameters(new ServiceRegistrationParameters(ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>(), skipImplementationTypesAlreadyInUse: false));
		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache).Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(2);
	}

	[TestMethod]
	public void TryRegister_ignores_duplicates_when_skip_duplicates_is_true()
	{
		var skipDuplicates = true;
		var skipImplementationTypesAlreadyInUse = false;

		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		var tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor1, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeTrue();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().Contain(serviceDescriptor1);

		// Everything but TryAdd should hit the SkipDuplicates check and return false, TryAdd tested below would return true
		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeFalse();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);

		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.ReplaceAll, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeFalse();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);

		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.ReplaceFirst, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeFalse();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);
	}

	[TestMethod]
	public void TryRegister_only_registers_the_type_once_when_the_overwrite_behavior_is_TryAdd()
	{
		var skipDuplicates = true;
		var skipImplementationTypesAlreadyInUse = false;

		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		var tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptor1, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeTrue();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().Contain(serviceDescriptor1);

		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeTrue();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);
	}

	[TestMethod]
	public void TryRegister_throws_ArgumentNullExceptions_for_null_arguments_that_are_required()
	{
		var serviceDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

		Invoking(() => ServiceRegistrationConventionBase.TryRegister(ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptor: null!, serviceRegistrationCache, true, true))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceDescriptor*");

		Invoking(() => ServiceRegistrationConventionBase.TryRegister(ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptor, serviceRegistrationCache: null!, true, true))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationCache*");
	}

	[TestMethod]
	public void TryRegister_works_correctly_when_skip_implementation_types_already_in_use_is_true()
	{
		var skipDuplicates = false;
		var skipImplementationTypesAlreadyInUse = true;

		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor1 = ServiceDescriptor.Scoped<IDependencyPart1, SomethingThatImplementsTwoInterfaces>();
		var tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor1, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeTrue();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().Contain(serviceDescriptor1);

		// Everything but TryAdd should hit the SkipImplementationTypesAlreadyInUseCheck and return false, TryAdd is tested separately and would return true
		var serviceDescriptor2 = ServiceDescriptor.Scoped<IDependencyPart2, SomethingThatImplementsTwoInterfaces>();
		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeFalse();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);

		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.ReplaceAll, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeFalse();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);

		tryRegisterResult = ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.ReplaceFirst, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse);
		tryRegisterResult.Should().BeFalse();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().NotContain(serviceDescriptor2);
	}

	[TestMethod]
	public void TryRegister_works_correctly_when_the_overwrite_behavior_is_replace_all()
	{
		var skipDuplicates = true;
		var skipImplementationTypesAlreadyInUse = false;

		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingService, SomethingService>();
		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingService, AlternateSomethingService>();
		var serviceDescriptor3 = ServiceDescriptor.Scoped<ISomethingService, SomethingService3>();
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor1, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor3, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceCollection.Count.Should().Be(3);
		using var serviceProvider1 = serviceCollection.BuildServiceProvider();
		serviceProvider1.GetServices<ISomethingService>().Count().Should().Be(3);

		// Everything but TryAdd should hit the SkipImplementationTypesAlreadyInUseCheck and return false, TryAdd is tested separately and would return true
		var replaceAllDescriptor = ServiceDescriptor.Singleton<ISomethingService>(new SomethingService());
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.ReplaceAll, replaceAllDescriptor, serviceRegistrationCache, false, false)
			.Should().BeTrue();
		serviceCollection.Count.Should().Be(1);
		serviceCollection.Should().Contain(replaceAllDescriptor);
		serviceCollection.Should().NotContain(serviceDescriptor1);
		serviceCollection.Should().NotContain(serviceDescriptor2);
		serviceCollection.Should().NotContain(serviceDescriptor3);
	}

	[TestMethod]
	public void TryRegister_works_correctly_when_the_overwrite_behavior_is_replace_first()
	{
		var skipDuplicates = true;
		var skipImplementationTypesAlreadyInUse = false;

		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingService, SomethingService>();
		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingService, AlternateSomethingService>();
		var serviceDescriptor3 = ServiceDescriptor.Scoped<ISomethingService, SomethingService3>();
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor1, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor2, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.Add, serviceDescriptor3, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceCollection.Count.Should().Be(3);
		using var serviceProvider1 = serviceCollection.BuildServiceProvider();
		serviceProvider1.GetServices<ISomethingService>().Count().Should().Be(3);

		// Everything but TryAdd should hit the SkipImplementationTypesAlreadyInUseCheck and return false, TryAdd is tested separately and would return true
		var replaceAllDescriptor = ServiceDescriptor.Singleton<ISomethingService>(new SomethingService());
		ServiceRegistrationConventionBase
			.TryRegister(ServiceRegistrationOverwriteBehavior.ReplaceFirst, replaceAllDescriptor, serviceRegistrationCache, false, false)
			.Should().BeTrue();
		serviceCollection.Count.Should().Be(3);
		serviceCollection.Should().NotContain(serviceDescriptor1);
		serviceCollection.Should().Contain(replaceAllDescriptor);
		serviceCollection.Should().Contain(serviceDescriptor2);
		serviceCollection.Should().Contain(serviceDescriptor3);
	}

	[TestMethod]
	public void TryRegisterMultiple_ignores_duplicates_when_skip_duplicates_is_true()
	{
		var skipImplementationTypesAlreadyInUse = false;

		var existingDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		var newDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, AlternateSomethingOptions>();

		var serviceDescriptors = new[] {
				ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>(),
				ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>(),
				newDescriptor,
			};

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				existingDescriptor
			};

		// Should ignore the two duplicates and only add the new AlternateSomethingOptions implementation
		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptors, serviceRegistrationCache, true, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(existingDescriptor);
		serviceRegistrationCache.Should().NotContain(newDescriptor);

		serviceRegistrationCache.Remove(newDescriptor);
		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.Add, serviceDescriptors, serviceRegistrationCache, true, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(2);
		serviceRegistrationCache.Should().Contain(existingDescriptor);
		serviceRegistrationCache.Should().Contain(newDescriptor);

		// Skip duplicates in the ReplaceAll scenario will ignore the two duplicates and then replace all with 
		serviceRegistrationCache.Remove(newDescriptor);
		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.ReplaceAll, serviceDescriptors, serviceRegistrationCache, true, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(newDescriptor);

		serviceRegistrationCache.Remove(newDescriptor);
		serviceRegistrationCache.Add(existingDescriptor);
		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.ReplaceFirst, serviceDescriptors, serviceRegistrationCache, true, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(newDescriptor);
	}

	[TestMethod]
	public void TryRegisterMultiple_only_registers_the_type_once_when_the_overwrite_behavior_is_TryAdd()
	{
		var skipDuplicates = true;
		var skipImplementationTypesAlreadyInUse = true;

		var existingDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();

		var serviceDescriptors = new[] {
				ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>(),
				ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>(),
			};

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				existingDescriptor
			};

		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptors, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeTrue();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(existingDescriptor);
	}

	[TestMethod]
	public void TryRegisterMultiple_throws_ArgumentNullExceptions_for_null_arguments_that_are_required()
	{
		var discoveredType = typeof(ISomethingOptions);
		var serviceDescriptors = new[] { ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>() };
		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

		Invoking(() => ServiceRegistrationConventionBase.TryRegisterMultiple(discoveredType: null!, ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptors, serviceRegistrationCache, true, true))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*discoveredType*");

		Invoking(() => ServiceRegistrationConventionBase.TryRegisterMultiple(discoveredType, ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptors: null!, serviceRegistrationCache, true, true))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceDescriptors*");

		Invoking(() => ServiceRegistrationConventionBase.TryRegisterMultiple(discoveredType, ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptors, serviceRegistrationCache: null!, true, true))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationCache*");
	}

	[TestMethod]
	public void TryRegisterMultiple_works_correctly_when_skip_implementation_types_already_in_use_is_true()
	{
		var skipDuplicates = true;
		var skipImplementationTypesAlreadyInUse = true;

		var existingDescriptor = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();

		var serviceDescriptors = new[] {
				ServiceDescriptor.Scoped<SomethingOptions, SomethingOptions>()
			};

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				existingDescriptor
			};

		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.TryAdd, serviceDescriptors, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(existingDescriptor);

		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.Add, serviceDescriptors, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(existingDescriptor);

		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.ReplaceAll, serviceDescriptors, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(existingDescriptor);

		ServiceRegistrationConventionBase
			.TryRegisterMultiple(existingDescriptor.ServiceType, ServiceRegistrationOverwriteBehavior.ReplaceFirst, serviceDescriptors, serviceRegistrationCache, skipDuplicates, skipImplementationTypesAlreadyInUse)
			.Should().BeFalse();
		serviceRegistrationCache.Count.Should().Be(1);
		serviceRegistrationCache.Should().Contain(existingDescriptor);
	}




	/******     TEST SETUP     *****************************
	 *******************************************************/
	public class ServiceRegistrationConventionBaseSubclass : ServiceRegistrationConventionBase
	{
		private ServiceRegistrationParameters? _mockServiceRegistrationParameters;
		private bool _useMockServiceRegistrationParameters;

		public ServiceRegistrationConventionBaseSubclass(
			ConstructorSelectionType defaultConstructorSelectionType = ConstructorSelectionType.DefaultBehaviorOnly,
			ServiceLifetime defaultLifetime = ServiceLifetime.Scoped,
			ServiceRegistrationOverwriteBehavior defaultOverwriteBehavior = ServiceRegistrationOverwriteBehavior.TryAdd,
			bool skipDuplicates = true,
			bool skipImplementationTypesAlreadyInUse = true)
			: base(defaultConstructorSelectionType, defaultLifetime, defaultOverwriteBehavior, skipDuplicates, skipImplementationTypesAlreadyInUse)
		{
		}

		public bool WasGetServiceRegistrationParametersCalled { get; private set; }

		public new ServiceDescriptor? BuildServiceDescriptor(
			Type discoveredServiceType,
			Type implementationType,
			ServiceRegistrationCache serviceRegistrationCache,
			ConstructorSelectionType? constructorSelectionType = null,
			ServiceLifetime? lifetime = null,
			bool? skipImplementedTypesAlreadyInUse = null)
			=> base.BuildServiceDescriptor(discoveredServiceType, implementationType, serviceRegistrationCache, constructorSelectionType, lifetime, skipImplementedTypesAlreadyInUse);

		public override ServiceRegistrationParameters? GetServiceRegistrationParameters(
			System.Type discoveredType,
			IAssemblyScanResult scanResult,
			ServiceRegistrationCache serviceRegistrationCache)
		{
			WasGetServiceRegistrationParametersCalled = true;

			if (_useMockServiceRegistrationParameters)
				return _mockServiceRegistrationParameters;

			throw new System.NotImplementedException();
		}

		public void SetMockServiceRegistrationParameters(ServiceRegistrationParameters serviceRegistrationParameters)
		{
			_mockServiceRegistrationParameters = serviceRegistrationParameters;
			_useMockServiceRegistrationParameters = true;
		}
	}
}
