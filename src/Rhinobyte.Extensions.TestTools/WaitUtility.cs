using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.TestTools
{
	/// <summary>
	/// Test helper utility to perform asynchronous delays and wait loops that poll for a conditionl match
	/// <para>Includes support for a configuration object that can be used to supercede the values passed via method parameter</para>
	/// </summary>
	/// <remarks>
	/// <para>This utility type is generally intended for functional and end-to-end testing scenarios where waiting for a condition before proceeding is common</para>
	/// <para>
	/// The configuration object support is intended to allow local test runners to override delay/wait until values so fast running automated tests are easier to follow
	/// by a human, without requiring changes to the actual test code
	/// </para>
	/// </remarks>
	public class WaitUtility
	{
		/// <summary>
		/// An optional <see cref="WaitConfiguration"/> whose configuration item values will supercede the method value parameters passed to
		/// this utilitiies delay/wait methods
		/// </summary>
		public WaitConfiguration? Configuration { get; set; }

		/// <summary>
		/// The minimum value in milliseconds to use for any delay intervals or wait polling intervals
		/// </summary>
		public int MinimumInterval { get; set; } = 100;

		/// <summary>
		/// Check for an available delay configuration from the specified <paramref name="waitId"/>.
		/// <para>
		/// If a match is found, call <see cref="Task.Delay(int, CancellationToken)"/> for the configured
		/// <see cref="WaitConfigurationItem.Delay"/> amount of time
		/// </para>
		/// </summary>
		/// <param name="cancellationToken">The optional cancellation token for the asynchronous operations</param>
		/// <param name="ignoreDefaultConfigItem">Whether or not a delay value from a 'Default' config item key should be ignored</param>
		/// <param name="waitId">The waitId to check for a delay configuration value</param>
		public async Task ConfiguredDelayAsync(
			string waitId,
			bool ignoreDefaultConfigItem = true,
			CancellationToken cancellationToken = default)
		{
			var waitConfigurationValues = Configuration?.FindWaitConfigurationValues(waitId);
			if (waitConfigurationValues?.Delay is null || waitConfigurationValues.Delay < 1)
				return;

			if (ignoreDefaultConfigItem && "Default".Equals(waitConfigurationValues.DelayItemKey, StringComparison.OrdinalIgnoreCase))
				return;

			var delayValue = waitConfigurationValues.Delay.Value;
			if (delayValue < MinimumInterval)
				delayValue = MinimumInterval;

			await Task.Delay(delayValue, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Perform a Task.Delay for the configured or specified delay interval.
		/// <para>Any matched waitConfigurationItem.InitialDelay will supercede the passed in delay</para>
		/// </summary>
		/// <param name="cancellationToken">The optional cancellation token for the asynchronous operations</param>
		/// <param name="millisecondsDelay">The delay in milliseconds to use when no configuration match is found</param>
		/// <param name="waitId">The optional waitId to match against waitConfigurationItems with</param>
		public async Task DelayAsync(
			int millisecondsDelay,
			string? waitId = null,
			CancellationToken cancellationToken = default)
		{
			var waitConfigurationValues = Configuration?.FindWaitConfigurationValues(waitId);

			var delayValue = waitConfigurationValues?.Delay ?? millisecondsDelay;
			if (delayValue < MinimumInterval)
				delayValue = MinimumInterval;

			await Task.Delay(delayValue, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Wait until the provided <paramref name="condition"/> evaluates to true
		/// <para>Polls the status of the condition according to the provided <paramref name="pollingInterval"/></para>
		/// </summary>
		/// <returns>The first non-null, non-false condtion function result received</returns>
		/// <remarks>
		/// Uses Task.Delay between polling intervals.
		/// Prefer using this over using selenium's WebDriverWait class which uses Thread.Sleep internally.
		/// </remarks>
		public async Task<TResult> WaitUntilAsync<TResult>(
			Func<TResult?> condition,
			int initialDelay = 500,
			int pollingInterval = 500,
			int timeoutInterval = 60_000,
			string? waitId = null,
			CancellationToken cancellationToken = default)
		{
			_ = condition ?? throw new ArgumentNullException(nameof(condition));

			var waitConfigurationValues = Configuration?.FindWaitConfigurationValues(waitId);

			var delayIntervalToUse = waitConfigurationValues?.Delay ?? initialDelay;
			if (delayIntervalToUse > 0 && delayIntervalToUse < MinimumInterval)
				delayIntervalToUse = MinimumInterval;

			var pollingIntervalToUse = waitConfigurationValues?.PollingInterval ?? pollingInterval;
			if (pollingIntervalToUse < MinimumInterval)
				pollingIntervalToUse = MinimumInterval;

			return await WaitUntilInternalAsync(
					cancellationToken,
					condition,
					delayIntervalToUse,
					pollingIntervalToUse,
					waitConfigurationValues?.TimeoutInterval ?? timeoutInterval,
					waitId
				)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Wait until the provided <paramref name="asyncCondition"/> evaluates to true
		/// <para>Polls the status of the condition according to the provided <paramref name="pollingInterval"/></para>
		/// </summary>
		/// <returns>The first non-null, non-false condtion function result received</returns>
		/// <remarks>
		/// Uses Task.Delay between polling intervals.
		/// Prefer using this over using selenium's WebDriverWait class which uses Thread.Sleep internally.
		/// </remarks>
		public async Task<TResult> WaitUntilAsync<TResult>(
			Func<CancellationToken, Task<TResult?>> asyncCondition,
			int initialDelay = 500,
			int pollingInterval = 500,
			int timeoutInterval = 60_000,
			string? waitId = null,
			CancellationToken cancellationToken = default)
		{
			_ = asyncCondition ?? throw new ArgumentNullException(nameof(asyncCondition));

			var waitConfigurationValues = Configuration?.FindWaitConfigurationValues(waitId);

			var delayIntervalToUse = waitConfigurationValues?.Delay ?? initialDelay;
			if (delayIntervalToUse > 0 && delayIntervalToUse < MinimumInterval)
				delayIntervalToUse = MinimumInterval;

			var pollingIntervalToUse = waitConfigurationValues?.PollingInterval ?? pollingInterval;
			if (pollingIntervalToUse < MinimumInterval)
				pollingIntervalToUse = MinimumInterval;

			return await WaitUntilInternalAsync(
					asyncCondition,
					cancellationToken,
					delayIntervalToUse,
					pollingIntervalToUse,
					waitConfigurationValues?.TimeoutInterval ?? timeoutInterval,
					waitId
				)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Wait until the provided <paramref name="condition"/> evaluates to true
		/// <para>Polls the status of the condition according to the provided <paramref name="pollingInterval"/></para>
		/// </summary>
		/// <returns>The first non-null, non-false condtion function result received</returns>
		/// <remarks>
		/// Uses Task.Delay between polling intervals.
		/// Prefer using this over using selenium's WebDriverWait class which uses Thread.Sleep internally.
		/// <para>
		/// This overload supports a state parameter object for the condition function to avoid the need for lambda functions
		/// that require a closure
		/// </para>
		/// </remarks>
		public async Task<TResult> WaitUntilAsync<TState, TResult>(
			Func<TState, TResult?> condition,
			TState conditionState,
			int initialDelay = 500,
			int pollingInterval = 500,
			int timeoutInterval = 60_000,
			string? waitId = null,
			CancellationToken cancellationToken = default)
		{
			_ = condition ?? throw new ArgumentNullException(nameof(condition));

			var waitConfigurationValues = Configuration?.FindWaitConfigurationValues(waitId);

			var delayIntervalToUse = waitConfigurationValues?.Delay ?? initialDelay;
			if (delayIntervalToUse > 0 && delayIntervalToUse < MinimumInterval)
				delayIntervalToUse = MinimumInterval;

			var pollingIntervalToUse = waitConfigurationValues?.PollingInterval ?? pollingInterval;
			if (pollingIntervalToUse < MinimumInterval)
				pollingIntervalToUse = MinimumInterval;

			return await WaitUntilInternalAsync(
					cancellationToken,
					condition,
					conditionState,
					delayIntervalToUse,
					pollingIntervalToUse,
					waitConfigurationValues?.TimeoutInterval ?? timeoutInterval,
					waitId
				)
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Wait until the provided <paramref name="asyncCondition"/> evaluates to true
		/// <para>Polls the status of the condition according to the provided <paramref name="pollingInterval"/></para>
		/// </summary>
		/// <returns>The first non-null, non-false condtion function result received</returns>
		/// <remarks>
		/// Uses Task.Delay between polling intervals.
		/// Prefer using this over using selenium's WebDriverWait class which uses Thread.Sleep internally.
		/// <para>
		/// This overload supports a state parameter object for the condition function to avoid the need for lambda functions
		/// that require a closure
		/// </para>
		/// </remarks>
		public async Task<TResult> WaitUntilAsync<TState, TResult>(
			Func<TState, CancellationToken, Task<TResult?>> asyncCondition,
			TState conditionState,
			int initialDelay = 500,
			int pollingInterval = 500,
			int timeoutInterval = 60_000,
			string? waitId = null,
			CancellationToken cancellationToken = default)
		{
			_ = asyncCondition ?? throw new ArgumentNullException(nameof(asyncCondition));

			var waitConfigurationValues = Configuration?.FindWaitConfigurationValues(waitId);

			var delayIntervalToUse = waitConfigurationValues?.Delay ?? initialDelay;
			if (delayIntervalToUse > 0 && delayIntervalToUse < MinimumInterval)
				delayIntervalToUse = MinimumInterval;

			var pollingIntervalToUse = waitConfigurationValues?.PollingInterval ?? pollingInterval;
			if (pollingIntervalToUse < MinimumInterval)
				pollingIntervalToUse = MinimumInterval;

			return await WaitUntilInternalAsync(
					asyncCondition,
					cancellationToken,
					conditionState,
					delayIntervalToUse,
					pollingIntervalToUse,
					waitConfigurationValues?.TimeoutInterval ?? timeoutInterval,
					waitId
				)
				.ConfigureAwait(false);
		}

		private static async Task<TResult> WaitUntilInternalAsync<TResult>(
			CancellationToken cancellationToken,
			Func<TResult?> condition,
			int initialDelay,
			int pollingInterval,
			int timeoutInterval,
			string? waitId)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			if (initialDelay > 0)
				await Task.Delay(initialDelay, cancellationToken).ConfigureAwait(false);

			while (stopwatch.ElapsedMilliseconds < timeoutInterval)
			{
				var result = condition();
				if (result != null && !result.Equals(false))
				{
					stopwatch.Stop();
					return result;
				}

				await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
			}

			stopwatch.Stop();
			throw new TimeoutException($"The timeout threshold was reached before the {waitId} condition was met");
		}

		private static async Task<TResult> WaitUntilInternalAsync<TResult>(
			Func<CancellationToken, Task<TResult?>> asyncCondition,
			CancellationToken cancellationToken,
			int initialDelay,
			int pollingInterval,
			int timeoutInterval,
			string? waitId)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			if (initialDelay > 0)
				await Task.Delay(initialDelay, cancellationToken).ConfigureAwait(false);

			while (stopwatch.ElapsedMilliseconds < timeoutInterval)
			{
				var result = await asyncCondition(cancellationToken).ConfigureAwait(false);
				if (result != null && !result.Equals(false))
				{
					stopwatch.Stop();
					return result;
				}

				await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
			}

			stopwatch.Stop();
			throw new TimeoutException($"The timeout threshold was reached before the {waitId} condition was met");
		}

		private static async Task<TResult> WaitUntilInternalAsync<TResult, TState>(
			CancellationToken cancellationToken,
			Func<TState, TResult?> condition,
			TState conditionState,
			int initialDelay,
			int pollingInterval,
			int timeoutInterval,
			string? waitId)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			if (initialDelay > 0)
				await Task.Delay(initialDelay, cancellationToken).ConfigureAwait(false);

			while (stopwatch.ElapsedMilliseconds < timeoutInterval)
			{
				var result = condition(conditionState);
				if (result != null && !result.Equals(false))
				{
					stopwatch.Stop();
					return result;
				}

				await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
			}

			stopwatch.Stop();
			throw new TimeoutException($"The timeout threshold was reached before the {waitId} condition was met");
		}

		private static async Task<TResult> WaitUntilInternalAsync<TResult, TState>(
			Func<TState, CancellationToken, Task<TResult?>> asyncCondition,
			CancellationToken cancellationToken,
			TState conditionState,
			int initialDelay,
			int pollingInterval,
			int timeoutInterval,
			string? waitId)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			if (initialDelay > 0)
				await Task.Delay(initialDelay, cancellationToken).ConfigureAwait(false);

			while (stopwatch.ElapsedMilliseconds < timeoutInterval)
			{
				var result = await asyncCondition(conditionState, cancellationToken).ConfigureAwait(false);
				if (result != null && !result.Equals(false))
				{
					stopwatch.Stop();
					return result;
				}

				await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
			}

			stopwatch.Stop();
			throw new TimeoutException($"The timeout threshold was reached before the {waitId} condition was met");
		}
	}
}
