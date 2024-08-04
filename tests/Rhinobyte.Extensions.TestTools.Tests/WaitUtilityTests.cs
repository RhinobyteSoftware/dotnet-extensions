using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.TestTools.Tests;

[TestClass]
public class WaitUtilityTests
{
	/******     TEST METHODS     ****************************
	 ********************************************************/
	[TestMethod]
	public void Constructor_works()
	{
		new WaitUtility().Should().NotBeNull();
	}

	[TestMethod]
	public async Task ConfiguredDelayAsync_behaves_as_expected_for_ignoreDefaultConfigItem_argument_values()
	{
		var waitUtility = new WaitUtility();

		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		_ = configurationItems.TryAdd("Default", new WaitConfigurationItem() { Delay = 500 });

		waitUtility.Configuration = new WaitConfiguration(configurationItems);

		// Does nothing when ignoreDefaultConfigItem is true
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.ConfiguredDelayAsync("Non.Existant.WaitId", ignoreDefaultConfigItem: true, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);

		// Delays when ignoreDefaultConfigItem is false
		stopwatch.Restart();
		await waitUtility.ConfiguredDelayAsync("Non.Existant.WaitId", ignoreDefaultConfigItem: false, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(495);
	}

	[TestMethod]
	public async Task ConfiguredDelayAsync_does_nothing_when_not_configured()
	{
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		_ = configurationItems.TryAdd("Some.Wait.Id", new WaitConfigurationItem() { Delay = 0 });

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems)
		};

		// Does nothing since the configured delay value is zero
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.ConfiguredDelayAsync("Some.Wait.Id", ignoreDefaultConfigItem: false, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);
	}

	[TestMethod]
	public async Task ConfiguredDelayAsync_does_nothing_when_configured_to_zero()
	{
		var waitUtility = new WaitUtility();

		// Does nothing if an invalid wait id is passed in
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.ConfiguredDelayAsync(null!, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);

		// Does nothing when configuration container is null
		stopwatch.Restart();
		await waitUtility.ConfiguredDelayAsync("Non.Existant.WaitId", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);


		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		waitUtility.Configuration = new WaitConfiguration(configurationItems);

		// Does nothing when configuration container is empty
		stopwatch.Restart();
		await waitUtility.ConfiguredDelayAsync("Non.Existant.WaitId", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);


		configurationItems.TryAdd("Category1", new WaitConfigurationItem() { Delay = 5000 });

		// Does nothing when no configuration items match
		stopwatch.Restart();
		await waitUtility.ConfiguredDelayAsync("Non.Existant.WaitId", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);
	}

	[TestMethod]
	public async Task ConfiguredDelayAsync_uses_the_minimum_interval_when_necessary()
	{
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		_ = configurationItems.TryAdd("Some.Wait", new WaitConfigurationItem() { Delay = 75 });

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems),
			MinimumInterval = 500
		};


		// Does nothing if an invalid wait id is passed in
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.ConfiguredDelayAsync("Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(495);
	}

	[TestMethod]
	public async Task ConfiguredDelayAsync_uses_the_most_specific_category_with_a_non_null_delay_value()
	{
		var waitUtility = new WaitUtility();

		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		var item1 = new WaitConfigurationItem() { Delay = 500, TimeoutInterval = 30_000 };
		_ = configurationItems.TryAdd("Some.Wait.Id", item1);
		var item2 = new WaitConfigurationItem() { Delay = 1000, TimeoutInterval = 45_000 };
		_ = configurationItems.TryAdd("Some", item2);
		var defaultItem = new WaitConfigurationItem() { Delay = 1500, TimeoutInterval = 30_000 };
		_ = configurationItems.TryAdd("Default", defaultItem);


		waitUtility.Configuration = new WaitConfiguration(configurationItems);

		// Uses Some.Wait.Id value of 500
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.ConfiguredDelayAsync("Some.Wait.Id", ignoreDefaultConfigItem: false, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(495).And.BeLessThan(900);

		item1.Delay = null;
		waitUtility.Configuration.ClearCache(); // Clear the composite cache entries

		// Should use the 'Some' delay since 'Some.Wait.Id'
		stopwatch.Restart();
		await waitUtility.ConfiguredDelayAsync("Some.Wait.Id", ignoreDefaultConfigItem: false, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(995).And.BeLessThan(1400);
	}

	[TestMethod]
	public async Task DelayAsync_behaves_as_expected()
	{
		var waitUtility = new WaitUtility();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.DelayAsync(200, waitId: null, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(195).And.BeLessThan(500);

		stopwatch.Restart();
		await waitUtility.DelayAsync(500, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(495);
	}

	[TestMethod]
	public async Task DelayAsync_uses_the_minimum_interval_when_necessary()
	{
		var waitUtility = new WaitUtility
		{
			MinimumInterval = 500
		};

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.DelayAsync(50, waitId: null, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(495);
	}

	[TestMethod]
	public async Task DelayAsync_uses_the_configuration_value_when_available()
	{
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		_ = configurationItems.TryAdd("Some.Wait.Id", new WaitConfigurationItem() { PollingInterval = 500, TimeoutInterval = 10_000 });
		_ = configurationItems.TryAdd("Some.Wait", new WaitConfigurationItem() { Delay = 500 });

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems)
		};

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		await waitUtility.DelayAsync(3000, "Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(495).And.BeLessThan(900);
	}

	[TestMethod]
	public async Task WaitUntilAsync_correctly_waits_until_the_condition_is_met1()
	{
		var waitUtility = new WaitUtility();
		var waitCount = 0;
		bool waitCondition()
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return incrementedValue > 4;
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<bool>(waitCondition, initialDelay: 0, pollingInterval: 200, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(795).And.BeLessThan(1000);

		result.Should().BeTrue();
		waitCount.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_correctly_waits_until_the_condition_is_met2()
	{
		// Test the overload that takes a conditionState
		var waitUtility = new WaitUtility();

		var stateToUse = new WaitIntegerState();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<WaitIntegerState, bool>(StaticBoolWaitCondition, stateToUse, initialDelay: 0, pollingInterval: 200, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(795).And.BeLessThan(1000);

		result.Should().BeTrue();
		stateToUse.CurrentValue.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_correctly_waits_until_the_condition_is_met3()
	{
		var waitUtility = new WaitUtility();
		var waitCount = 0;
		Task<bool> waitConditionAsync(CancellationToken cancellationToken)
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return Task.FromResult(incrementedValue > 4);
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<bool>(waitConditionAsync, initialDelay: 0, pollingInterval: 200, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(795).And.BeLessThan(1000);

		result.Should().BeTrue();
		waitCount.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_correctly_waits_until_the_condition_is_met4()
	{
		// Test the overload that takes a conditionState
		var waitUtility = new WaitUtility();

		var stateToUse = new WaitIntegerState();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<WaitIntegerState, bool>(StaticBoolWaitConditionAsync, stateToUse, initialDelay: 0, pollingInterval: 200, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(795).And.BeLessThan(1000);

		result.Should().BeTrue();
		stateToUse.CurrentValue.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_ArgumentNullException_for_a_null_condition_argument()
	{
		await Awaiting(() => new WaitUtility().WaitUntilAsync<string>(condition: null!))
			.Should()
			.ThrowAsync<ArgumentNullException>()
			.WithMessage("Value cannot be null.*condition*");

		await Awaiting(() => new WaitUtility().WaitUntilAsync<string>(asyncCondition: null!))
			.Should()
			.ThrowAsync<ArgumentNullException>()
			.WithMessage("Value cannot be null.*asyncCondition*");

		await Awaiting(() => new WaitUtility().WaitUntilAsync<int, string>(condition: null!, conditionState: 5))
			.Should()
			.ThrowAsync<ArgumentNullException>()
			.WithMessage("Value cannot be null.*condition*");

		await Awaiting(() => new WaitUtility().WaitUntilAsync<int, string>(asyncCondition: null!, conditionState: 5))
			.Should()
			.ThrowAsync<ArgumentNullException>()
			.WithMessage("Value cannot be null.*asyncCondition*");
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TaskCancellationException_when_cancelled1()
	{
		using var cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable IDE0039 // Use local function
		Func<object?> waitCondition = () =>
#pragma warning restore IDE0039 // Use local function
		{
			cancellationTokenSource.Cancel();
			return null;
		};

		var waitUtility = new WaitUtility();


		TaskCanceledException? taskCanceledException = null;
		try
		{
			// Use the cancellationTokenSource.Token for this test's cancellationToken parameter
			await waitUtility
				.WaitUntilAsync<object>(waitCondition, initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: cancellationTokenSource.Token);
		}
		catch (TaskCanceledException caughtException)
		{
			taskCanceledException = caughtException;
		}

		taskCanceledException.Should().NotBeNull();
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TaskCancellationException_when_cancelled2()
	{
#pragma warning disable IDE0039 // Use local function
		Func<CancellationTokenSource, object?> waitCondition2 = (tokenSourceState) =>
#pragma warning restore IDE0039 // Use local function
		{
			tokenSourceState.Cancel();
			return null;
		};

		var waitUtility = new WaitUtility();
		using var cancellationTokenSource = new CancellationTokenSource();

		TaskCanceledException? taskCanceledException = null;
		try
		{
			// Use the cancellationTokenSource.Token for this test's cancellationToken parameter
			await waitUtility
				.WaitUntilAsync<CancellationTokenSource, object>(waitCondition2, cancellationTokenSource, initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: cancellationTokenSource.Token);
		}
		catch (TaskCanceledException caughtException)
		{
			taskCanceledException = caughtException;
		}

		taskCanceledException.Should().NotBeNull();
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TaskCancellationException_when_cancelled3()
	{
		using var cancellationTokenSource = new CancellationTokenSource();

#pragma warning disable IDE0039 // Use local function
#if NET8_0_OR_GREATER
		Func<CancellationToken, Task<object?>> waitCondition = async (cancellationToken) =>
		{
			await cancellationTokenSource.CancelAsync();
			return null;
		};
#else
		Func<CancellationToken, Task<object?>> waitCondition = (cancellationToken) =>
		{
			cancellationTokenSource.Cancel();
			return Task.FromResult<object?>(null);
		};
#endif
#pragma warning restore IDE0039 // Use local function

		var waitUtility = new WaitUtility();


		TaskCanceledException? taskCanceledException = null;
		try
		{
			// Use the cancellationTokenSource.Token for this test's cancellationToken parameter
			await waitUtility
				.WaitUntilAsync<object>(waitCondition, initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: cancellationTokenSource.Token);
		}
		catch (TaskCanceledException caughtException)
		{
			taskCanceledException = caughtException;
		}

		taskCanceledException.Should().NotBeNull();
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TaskCancellationException_when_cancelled4()
	{
#pragma warning disable IDE0039 // Use local function
#if NET8_0_OR_GREATER
		Func<CancellationTokenSource, CancellationToken, Task<object?>> waitCondition4 = async (tokenSourceState, cancellationToken) =>
		{
			await tokenSourceState.CancelAsync();
			return null;
		};
#else
		Func<CancellationTokenSource, CancellationToken, Task<object?>> waitCondition4 = (tokenSourceState, cancellationToken) =>
		{
			tokenSourceState.Cancel();
			return Task.FromResult<object?>(null);
		};
#endif
#pragma warning restore IDE0039 // Use local function



		var waitUtility = new WaitUtility();
		using var cancellationTokenSource = new CancellationTokenSource();

		TaskCanceledException? taskCanceledException = null;
		try
		{
			// Use the cancellationTokenSource.Token for this test's cancellationToken parameter
			await waitUtility
				.WaitUntilAsync<CancellationTokenSource, object>(waitCondition4, cancellationTokenSource, initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: cancellationTokenSource.Token);
		}
		catch (TaskCanceledException caughtException)
		{
			taskCanceledException = caughtException;
		}

		taskCanceledException.Should().NotBeNull();
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TimeoutException_when_interval_is_exceeded1()
	{
		var waitUtility = new WaitUtility();

		var waitCount = 0;
		bool waitCondition()
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return incrementedValue > 4;
		}

		await Awaiting(() => waitUtility.WaitUntilAsync<bool>(waitCondition, initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest))
			.Should()
			.ThrowAsync<TimeoutException>()
			.WithMessage("The timeout threshold was reached before the*condition was met");
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TimeoutException_when_interval_is_exceeded2()
	{
		await Awaiting(() => new WaitUtility().WaitUntilAsync<WaitIntegerState, bool>(StaticBoolWaitCondition, new WaitIntegerState(), initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest))
			.Should()
			.ThrowAsync<TimeoutException>()
			.WithMessage("The timeout threshold was reached before the*condition was met");
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TimeoutException_when_interval_is_exceeded3()
	{
		var waitUtility = new WaitUtility();

		var waitCount = 0;
		Task<bool> waitConditionAsync(CancellationToken cancellationToken)
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return Task.FromResult(incrementedValue > 4);
		}

		await Awaiting(() => waitUtility.WaitUntilAsync<bool>(waitConditionAsync, initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest))
			.Should()
			.ThrowAsync<TimeoutException>()
			.WithMessage("The timeout threshold was reached before the*condition was met");
	}

	[TestMethod]
	public async Task WaitUntilAsync_throws_TimeoutException_when_interval_is_exceeded4()
	{
		await Awaiting(() => new WaitUtility().WaitUntilAsync<WaitIntegerState, bool>(StaticBoolWaitConditionAsync, new WaitIntegerState(), initialDelay: 0, pollingInterval: 200, timeoutInterval: 400, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest))
			.Should()
			.ThrowAsync<TimeoutException>()
			.WithMessage("The timeout threshold was reached before the*condition was met");
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_configuration_values_when_present1()
	{
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		var item1 = new WaitConfigurationItem() { Delay = 1000 };
		_ = configurationItems.TryAdd("Some.Wait.Id", item1);
		var item2 = new WaitConfigurationItem() { PollingInterval = 150 };
		_ = configurationItems.TryAdd("Some", item2);
		var defaultItem = new WaitConfigurationItem() { TimeoutInterval = 30_000 };
		_ = configurationItems.TryAdd("Default", defaultItem);

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems)
		};

		var waitCount = 0;
		bool waitCondition()
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return incrementedValue > 4;
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<bool>(
			waitCondition, initialDelay: 0, pollingInterval: 1000, timeoutInterval: 1500, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 1000 delay + (4 x 150) polling interval
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(1595).And.BeLessThan(2000);

		result.Should().BeTrue();
		waitCount.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_configuration_values_when_present2()
	{
		// Test the overload that takes a conditionState
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		var item1 = new WaitConfigurationItem() { Delay = 1000 };
		_ = configurationItems.TryAdd("Some.Wait.Id", item1);
		var item2 = new WaitConfigurationItem() { PollingInterval = 150 };
		_ = configurationItems.TryAdd("Some", item2);
		var defaultItem = new WaitConfigurationItem() { TimeoutInterval = 30_000 };
		_ = configurationItems.TryAdd("Default", defaultItem);

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems)
		};

		var stateToUse = new WaitIntegerState();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<WaitIntegerState, bool>(
			StaticBoolWaitCondition, stateToUse, initialDelay: 0, pollingInterval: 1000, timeoutInterval: 1500, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 1000 delay + (4 x 150) polling interval
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(1595).And.BeLessThan(2000);

		result.Should().BeTrue();
		stateToUse.CurrentValue.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_configuration_values_when_present3()
	{
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		var item1 = new WaitConfigurationItem() { Delay = 1000 };
		_ = configurationItems.TryAdd("Some.Wait.Id", item1);
		var item2 = new WaitConfigurationItem() { PollingInterval = 150 };
		_ = configurationItems.TryAdd("Some", item2);
		var defaultItem = new WaitConfigurationItem() { TimeoutInterval = 30_000 };
		_ = configurationItems.TryAdd("Default", defaultItem);

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems)
		};

		var waitCount = 0;
		Task<bool> waitConditionAsync(CancellationToken cancellationToken)
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return Task.FromResult(incrementedValue > 4);
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<bool>(
			waitConditionAsync, initialDelay: 0, pollingInterval: 1000, timeoutInterval: 1500, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 1000 delay + (4 x 150) polling interval
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(1595).And.BeLessThan(2000);

		result.Should().BeTrue();
		waitCount.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_configuration_values_when_present4()
	{
		// Test the overload that takes a conditionState
		var configurationItems = new ConcurrentDictionary<string, WaitConfigurationItem>();
		var item1 = new WaitConfigurationItem() { Delay = 1000 };
		_ = configurationItems.TryAdd("Some.Wait.Id", item1);
		var item2 = new WaitConfigurationItem() { PollingInterval = 150 };
		_ = configurationItems.TryAdd("Some", item2);
		var defaultItem = new WaitConfigurationItem() { TimeoutInterval = 30_000 };
		_ = configurationItems.TryAdd("Default", defaultItem);

		var waitUtility = new WaitUtility()
		{
			Configuration = new WaitConfiguration(configurationItems)
		};

		var stateToUse = new WaitIntegerState();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<WaitIntegerState, bool>(
			StaticBoolWaitConditionAsync, stateToUse, initialDelay: 0, pollingInterval: 1000, timeoutInterval: 1500, waitId: "Some.Wait.Id", cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 1000 delay + (4 x 150) polling interval
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(1595).And.BeLessThan(2000);

		result.Should().BeTrue();
		stateToUse.CurrentValue.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_minimum_interval_when_necessary1()
	{
		var waitUtility = new WaitUtility()
		{
			MinimumInterval = 150
		};

		var waitCount = 0;
		bool waitCondition()
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return incrementedValue > 4;
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<bool>(
			waitCondition, initialDelay: 50, pollingInterval: 50, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 150 delay + (4 x 150) polling interval = 750
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(745).And.BeLessThan(850);

		result.Should().BeTrue();
		waitCount.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_minimum_interval_when_necessary2()
	{
		var waitUtility = new WaitUtility()
		{
			MinimumInterval = 150
		};

		var stateToUse = new WaitIntegerState();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<WaitIntegerState, bool>(
			StaticBoolWaitCondition, stateToUse, initialDelay: 50, pollingInterval: 50, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 150 delay + (4 x 150) polling interval = 750
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(745).And.BeLessThan(850);

		result.Should().BeTrue();
		stateToUse.CurrentValue.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_minimum_interval_when_necessary3()
	{
		var waitUtility = new WaitUtility()
		{
			MinimumInterval = 150
		};

		var waitCount = 0;
		Task<bool> waitConditionAsync(CancellationToken cancellationToken)
		{
			var incrementedValue = Interlocked.Increment(ref waitCount);
			return Task.FromResult(incrementedValue > 4);
		}

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<bool>(
			waitConditionAsync, initialDelay: 50, pollingInterval: 50, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 150 delay + (4 x 150) polling interval = 750
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(745).And.BeLessThan(900);

		result.Should().BeTrue();
		waitCount.Should().Be(5);
	}

	[TestMethod]
	public async Task WaitUntilAsync_uses_the_minimum_interval_when_necessary4()
	{
		var waitUtility = new WaitUtility()
		{
			MinimumInterval = 150
		};

		var stateToUse = new WaitIntegerState();

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var result = await waitUtility.WaitUntilAsync<WaitIntegerState, bool>(
			StaticBoolWaitConditionAsync, stateToUse, initialDelay: 50, pollingInterval: 50, cancellationToken: CancellationTokenForTest);
		stopwatch.Stop();

		// 150 delay + (4 x 150) polling interval = 750
		stopwatch.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(745).And.BeLessThan(900);

		result.Should().BeTrue();
		stateToUse.CurrentValue.Should().Be(5);
	}



	/******     TEST SETUP     *****************************
	 *******************************************************/
	public CancellationToken CancellationTokenForTest => TestContext?.CancellationTokenSource.Token ?? CancellationToken.None;

	public TestContext TestContext { get; set; } = null!;

	internal class WaitIntegerState
	{
		private int _value;

		public int CurrentValue => _value;

		public int IncrementValue() => Interlocked.Increment(ref _value);
	}

	internal static bool StaticBoolWaitCondition(WaitIntegerState state)
	{
		var incrementedValue = state.IncrementValue();
		return incrementedValue > 4;
	}

	internal static Task<bool> StaticBoolWaitConditionAsync(WaitIntegerState state, CancellationToken cancellationToken)
	{
		var incrementedValue = state.IncrementValue();
		return Task.FromResult(incrementedValue > 4);
	}
}
