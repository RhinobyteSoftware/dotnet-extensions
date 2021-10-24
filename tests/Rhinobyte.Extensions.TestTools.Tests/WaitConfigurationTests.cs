using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.TestTools.Tests
{
	[TestClass]
	public class WaitConfigurationTests
	{
		[TestMethod]
		public void Clear_behaves_as_expected()
		{
			var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
			_ = configurationItems.TryAdd("Some.Wait.Id", new WaitConfigurationItem() { Delay = 500 });
			var configurationCache = new ConcurrentDictionary<string, WaitConfigurationCompositeItem>();
			_ = configurationCache.TryAdd("Some.Wait.Id", new WaitConfigurationCompositeItem() { });
			var waitConfiguration = new WaitConfiguration(configurationItems, configurationCache);

			waitConfiguration.Clear();
			configurationItems.Should().BeEmpty();
			configurationCache.Should().BeEmpty();
		}

		[TestMethod]
		public void ClearCache_behaves_as_expected()
		{
			var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
			_ = configurationItems.TryAdd("Some.Wait.Id", new WaitConfigurationItem() { Delay = 500 });
			var configurationCache = new ConcurrentDictionary<string, WaitConfigurationCompositeItem>();
			_ = configurationCache.TryAdd("Some.Wait.Id", new WaitConfigurationCompositeItem() { });
			var waitConfiguration = new WaitConfiguration(configurationItems, configurationCache);

			waitConfiguration.ClearCache();
			configurationItems.Should().NotBeEmpty();
			configurationCache.Should().BeEmpty();
		}

		[TestMethod]
		public void Constructor_behaves_as_expected()
		{
			var waitConfiguration = new WaitConfiguration();
			waitConfiguration.FindWaitConfigurationValues("Some.Wait.Id").Should().BeNull();

			var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
			_ = configurationItems.TryAdd("Some.Wait.Id", new WaitConfigurationItem() { Delay = 500 });
			var configurationCache = new ConcurrentDictionary<string, WaitConfigurationCompositeItem>();
			waitConfiguration = new WaitConfiguration(configurationItems, waitConfigurationCache: configurationCache);

			configurationCache.Should().BeEmpty();
			waitConfiguration.FindWaitConfigurationValues("Some.Wait.Id").Should().NotBeNull();
			configurationCache.Should().NotBeEmpty();
		}

		[TestMethod]
		public void Constructor_throws_ArgumentNullException_for_null_argument()
		{
			Invoking(() => new WaitConfiguration(waitConfigurations: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*waitConfigurations*");
		}

		[TestMethod]
		public void FromDictionary_behaves_as_expected()
		{
			var configurationItems = new Dictionary<string, WaitConfigurationItem>();
			configurationItems.Add("Some.Wait.Id", new WaitConfigurationItem() { Delay = 500 });

			var waitConfiguration = WaitConfiguration.FromDictionary(configurationItems);
			waitConfiguration.FindWaitConfigurationValues("Some.Wait.Id").Should().NotBeNull();

			configurationItems.Add("Not.In.Configuration", new WaitConfigurationItem() { TimeoutInterval = 500 });
			waitConfiguration.FindWaitConfigurationValues("Not.In.Configuration").Should().BeNull();
		}

		[TestMethod]
		public void FromDictionary_throws_for_invalid_arguments()
		{
			Invoking(() => WaitConfiguration.FromDictionary(waitConfigurationItems: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*waitConfigurationItems*");

			Invoking(() => WaitConfiguration.FromDictionary(waitConfigurationItems: new Dictionary<string, WaitConfigurationItem>()))
				.Should()
				.Throw<ArgumentException>()
				.WithMessage("waitConfigurationItems cannot be an empty dictionary");
		}

		[TestMethod]
		public void Reload_behaves_as_expected()
		{
			var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
			_ = configurationItems.TryAdd("Some.Wait.Id", new WaitConfigurationItem() { Delay = 500 });
			var configurationCache = new ConcurrentDictionary<string, WaitConfigurationCompositeItem>();
			_ = configurationCache.TryAdd("Some.Wait.Id", new WaitConfigurationCompositeItem() { Delay = 500 });

			var waitConfiguration = new WaitConfiguration(configurationItems, waitConfigurationCache: configurationCache);

			var newItems = new Dictionary<string, WaitConfigurationItem>()
			{
				{ "Different.Wait.Id", new WaitConfigurationItem() { Delay = 800  } }
			};

			waitConfiguration.Reload(newItems);

			configurationItems.ContainsKey("Some.Wait.Id").Should().BeFalse();
			configurationItems.ContainsKey("Different.Wait.Id").Should().BeTrue();
			configurationCache.Should().BeEmpty();

			waitConfiguration.FindWaitConfigurationValues("Different.Wait.Id").Should().NotBeNull();
			configurationCache.ContainsKey("Different.Wait.Id").Should().BeTrue();
		}

		[TestMethod]
		public void Reload_throws_ArgumentNullException_for_a_null_dictionary_argument()
		{
			var waitConfiguration = new WaitConfiguration();
			Invoking(() => new WaitConfiguration().Reload(newConfigurationItems: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*newConfigurationItems*");
		}
	}
}
