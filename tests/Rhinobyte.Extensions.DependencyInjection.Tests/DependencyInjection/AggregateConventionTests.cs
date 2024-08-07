﻿using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Linq;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class AggregateConventionTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void Constructor_throws_ArgumentNullException_for_null_conventions_argument()
	{
		Invoking(() => new AggregateConvention(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*conventions*");
	}

	[TestMethod]
	public void HandleType_behaves_as_expected_when_try_all_conventions_is_false()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

		var dummyConvention1 = new DummyConvention() { ReturnValue = false };
		var dummyConvention2 = new DummyConvention() { ReturnValue = true };
		var dummyConvention3 = new DummyConvention() { ReturnValue = false };
		var childConventions = new[] { dummyConvention1, dummyConvention2, dummyConvention3 };

		var systemUnderTest = new AggregateConvention(
			childConventions,
			skipAlreadyRegistered: true,
			tryAllConventions: false
		);

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();

		dummyConvention1.WasHandleTypeCalled.Should().BeTrue();
		dummyConvention2.WasHandleTypeCalled.Should().BeTrue();
		dummyConvention3.WasHandleTypeCalled.Should().BeFalse();
	}

	[TestMethod]
	public void HandleType_behaves_as_expected_when_try_all_conventions_is_true()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

		var dummyConvention1 = new DummyConvention() { ReturnValue = true };
		var dummyConvention2 = new DummyConvention() { ReturnValue = true };
		var dummyConvention3 = new DummyConvention() { ReturnValue = false };
		var dummyConvention4 = new DummyConvention() { ReturnValue = true };
		var childConventions = new[] { dummyConvention1, dummyConvention2, dummyConvention3, dummyConvention4 };

		var systemUnderTest = new AggregateConvention(
			childConventions,
			skipAlreadyRegistered: true,
			tryAllConventions: true
		);

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();

		dummyConvention1.WasHandleTypeCalled.Should().BeTrue();
		dummyConvention2.WasHandleTypeCalled.Should().BeTrue();
		dummyConvention3.WasHandleTypeCalled.Should().BeTrue();
		dummyConvention4.WasHandleTypeCalled.Should().BeTrue();
	}

	[TestMethod]
	public void HandleType_does_not_skip_already_registered_types_when_value_is_false()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				ServiceDescriptor.Singleton<ISomethingOptions, SomethingOptions>()
			};

		var dummyConvention1 = new DummyConvention() { ReturnValue = true };
		var dummyConvention2 = new DummyConvention() { ReturnValue = true };
		var dummyConvention3 = new DummyConvention() { ReturnValue = true };
		var childConventions = new[] { dummyConvention1, dummyConvention2, dummyConvention3 };

		var systemUnderTest = new AggregateConvention(
			childConventions,
			skipAlreadyRegistered: false,
			tryAllConventions: true
		);

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeTrue();

		childConventions.Select(dummyConvention => dummyConvention.WasHandleTypeCalled).Should().AllBeEquivalentTo(true);
	}

	[TestMethod]
	public void HandleType_ignores_already_registered_types_when_value_is_true()
	{
		var scanResult = AssemblyScanner.CreateDefault()
			.AddExampleLibrary1()
			.ScanAssemblies();

		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection())
			{
				ServiceDescriptor.Singleton<ISomethingOptions, SomethingOptions>()
			};

		var dummyConvention1 = new DummyConvention() { ReturnValue = true };
		var dummyConvention2 = new DummyConvention() { ReturnValue = true };
		var dummyConvention3 = new DummyConvention() { ReturnValue = true };
		var childConventions = new[] { dummyConvention1, dummyConvention2, dummyConvention3 };

		var systemUnderTest = new AggregateConvention(
			childConventions,
			skipAlreadyRegistered: true,
			tryAllConventions: false
		);

		systemUnderTest.HandleType(typeof(ISomethingOptions), scanResult, serviceRegistrationCache)
			.Should().BeFalse();
		childConventions.Select(dummyConvention => dummyConvention.WasHandleTypeCalled).Should().AllBeEquivalentTo(false);
	}

	[TestMethod]
	public void HandleType_throws_ArgumentNullException_for_null_arguments()
	{
		var aggregateConvention = new AggregateConvention(new HashSet<IServiceRegistrationConvention>());
		var discoveredType = typeof(ISomethingOptions);
		var scanResult = new AssemblyScanResult();
		var serviceRegistrationCache = new ServiceRegistrationCache(new ServiceCollection());

		Invoking(() => aggregateConvention.HandleType(null!, scanResult, serviceRegistrationCache))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*discoveredType*");

		Invoking(() => aggregateConvention.HandleType(discoveredType, null!, serviceRegistrationCache))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*scanResult*");

		Invoking(() => aggregateConvention.HandleType(discoveredType, scanResult, null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceRegistrationCache*");
	}



	/******     TEST SETUP     *****************************
	 *******************************************************/
	public class DummyConvention : IServiceRegistrationConvention
	{
		public bool ReturnValue { get; set; }
		public bool WasHandleTypeCalled { get; private set; }

		public bool HandleType(Type discoveredType, IAssemblyScanResult scanResult, ServiceRegistrationCache serviceRegistrationCache)
		{
			WasHandleTypeCalled = true;
			return ReturnValue;
		}
	}
}
