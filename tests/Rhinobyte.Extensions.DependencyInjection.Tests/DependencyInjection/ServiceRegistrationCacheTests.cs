using ExampleLibrary1;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Linq;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests;

[TestClass]
public class ServiceRegistrationCacheTests
{
	[TestMethod]
	public void Add_passes_null_through_to_underlying_collection()
	{
		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);
		serviceRegistrationCache.Add(null!);
		serviceCollection.First().Should().BeNull();
	}

	[TestMethod]
	public void Constructor_throws_ArgumentNullException_for_null_service_collection_argument()
	{
		Invoking(() => new ServiceRegistrationCache(null!))
			.Should()
			.Throw<ArgumentNullException>()
			.WithMessage("Value cannot be null.*serviceCollection*");
	}

	[TestMethod]
	public void CopyTo_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		serviceCollection.Add(serviceDescriptor1);
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);


		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		serviceRegistrationCache.Add(serviceDescriptor2);
		var serviceDescriptor3 = ServiceDescriptor.Scoped<ISomethingService, SomethingService>();
		serviceRegistrationCache.Add(serviceDescriptor3);
		var serviceDescriptor4 = ServiceDescriptor.Scoped<ISomethingService, SomethingService3>();
		serviceRegistrationCache.Add(serviceDescriptor4);


		var arrayToCopyInto = new ServiceDescriptor[6];
		serviceRegistrationCache.CopyTo(arrayToCopyInto, 1);
		arrayToCopyInto[0].Should().BeNull();
		arrayToCopyInto[1].Should().Be(serviceDescriptor1);
		arrayToCopyInto[2].Should().Be(serviceDescriptor2);
		arrayToCopyInto[3].Should().Be(serviceDescriptor3);
		arrayToCopyInto[4].Should().Be(serviceDescriptor4);
		arrayToCopyInto[5].Should().BeNull();
	}

	[TestMethod]
	public void GetEnumerator_behave_as_expected()
	{
		var serviceCollection = new ServiceCollection();

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		serviceCollection.Add(serviceDescriptor1);

		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		serviceRegistrationCache.Add(serviceDescriptor2);

		var enumeratedDescriptors = new ServiceDescriptor?[2];
		var index = 0;
		foreach (var descriptor in serviceRegistrationCache)
		{
			enumeratedDescriptors[index++] = descriptor;
		}
		enumeratedDescriptors[0].Should().Be(serviceDescriptor1);
		enumeratedDescriptors[1].Should().Be(serviceDescriptor2);

		var rawEnumerator = ((IEnumerable)serviceRegistrationCache).GetEnumerator();
		enumeratedDescriptors = new ServiceDescriptor?[2];
		index = 0;
		while (rawEnumerator.MoveNext())
		{
			enumeratedDescriptors[index++] = rawEnumerator.Current as ServiceDescriptor;
		}
		enumeratedDescriptors[0].Should().Be(serviceDescriptor1);
		enumeratedDescriptors[1].Should().Be(serviceDescriptor2);
	}

	[TestMethod]
	public void HasExistingMatch_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();
		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingService, SomethingService>();
		serviceCollection.Add(serviceDescriptor1);
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		serviceRegistrationCache.HasExistingMatch(null!).Should().BeFalse();
		serviceRegistrationCache.HasExistingMatch(ServiceDescriptor.Scoped<IManuallyConfiguredType, ManuallyConfiguredType>()).Should().BeFalse();

		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingService, SomethingService>();
		serviceRegistrationCache.HasExistingMatch(serviceDescriptor2).Should().BeTrue();

		var serviceDescriptor3 = ServiceDescriptor.Scoped<ISomethingService, AlternateSomethingService>();
		var serviceDescriptor4 = ServiceDescriptor.Scoped<ISomethingService, SomethingService3>();

		serviceRegistrationCache.HasExistingMatch(serviceDescriptor3).Should().BeFalse();
		serviceRegistrationCache.HasExistingMatch(serviceDescriptor4).Should().BeFalse();

		serviceRegistrationCache.Add(serviceDescriptor3);
		serviceRegistrationCache.HasExistingMatch(serviceDescriptor3).Should().BeTrue();
		serviceRegistrationCache.HasExistingMatch(serviceDescriptor4).Should().BeFalse();

		serviceRegistrationCache.Add(serviceDescriptor4);
		serviceRegistrationCache.Remove(serviceDescriptor3);
		serviceRegistrationCache.HasExistingMatch(serviceDescriptor3).Should().BeFalse();
		serviceRegistrationCache.HasExistingMatch(serviceDescriptor4).Should().BeTrue();
	}

	[TestMethod]
	public void IndexOf_behave_as_expected()
	{
		var serviceCollection = new ServiceCollection();

		var serviceDescriptor1 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		serviceCollection.Add(serviceDescriptor1);

		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);

		var serviceDescriptor2 = ServiceDescriptor.Scoped<ISomethingOptions, SomethingOptions>();
		serviceRegistrationCache.Add(serviceDescriptor2);

		serviceRegistrationCache.IndexOf(serviceDescriptor1).Should().Be(0).And.Be(serviceCollection.IndexOf(serviceDescriptor1));
		serviceRegistrationCache.IndexOf(serviceDescriptor2).Should().Be(1).And.Be(serviceCollection.IndexOf(serviceDescriptor2));
	}

	[TestMethod]
	public void Insert_behaves_as_expected()
	{
		var serviceCollection = new ServiceCollection();
		_ = serviceCollection
			.AddScoped<ISomethingService, SomethingService>()
			.AddScoped<ISomethingOptions, SomethingOptions>()
			.AddScoped<ISomethingService, SomethingService3>();

		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);
		serviceRegistrationCache.IsReadOnly.Should().Be(serviceCollection.IsReadOnly);

		serviceRegistrationCache[1].ServiceType.Should().Be<ISomethingOptions>();
		serviceCollection[1].ServiceType.Should().Be<ISomethingOptions>();

		serviceRegistrationCache.GetByServiceType(typeof(ISomethingService))!.Count.Should().Be(2);

		var serviceDescriptorToInsert = ServiceDescriptor.Scoped<ISomethingService, AlternateSomethingService>();
		serviceRegistrationCache.Insert(1, serviceDescriptorToInsert);
		serviceRegistrationCache[2].ServiceType.Should().Be<ISomethingOptions>();
		serviceCollection[2].ServiceType.Should().Be<ISomethingOptions>();

		serviceRegistrationCache[1].Should().Be(serviceDescriptorToInsert);
		serviceCollection[1].Should().Be(serviceDescriptorToInsert);

		serviceRegistrationCache.GetByServiceType(typeof(ISomethingService))!.Count.Should().Be(3);
	}

	[TestMethod]
	public void Remove_passes_null_through_to_underlying_collection()
	{
		var serviceCollection = new ServiceCollection();
		var serviceRegistrationCache = new ServiceRegistrationCache(serviceCollection);
		serviceRegistrationCache.Add(null!);
		serviceCollection.First().Should().BeNull();

		serviceRegistrationCache.Remove(null!);
		serviceCollection.Should().BeEmpty();
	}


}
