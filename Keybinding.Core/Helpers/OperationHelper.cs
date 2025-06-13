// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Helpers;

/// <summary>
/// Helper class for common operation patterns to eliminate repeated code
/// </summary>
internal static class OperationHelper
{
	/// <summary>
	/// Executes an operation for each item and counts successful executions
	/// </summary>
	/// <typeparam name="T">The type of items to process</typeparam>
	/// <param name="items">The collection of items to process</param>
	/// <param name="operation">The operation to perform on each item</param>
	/// <param name="shouldContinueOnError">Whether to continue processing if an operation fails</param>
	/// <returns>Number of successful operations</returns>
	public static int ExecuteWithCount<T>(
		IEnumerable<T> items,
		Func<T, bool> operation,
		bool shouldContinueOnError = true)
	{
		ArgumentNullException.ThrowIfNull(items);
		ArgumentNullException.ThrowIfNull(operation);

		int successCount = 0;

		foreach (T item in items)
		{
			try
			{
				if (operation(item))
				{
					successCount++;
				}
			}
			catch (ArgumentException) when (shouldContinueOnError)
			{
				// Skip invalid items
			}
			catch (InvalidOperationException) when (shouldContinueOnError)
			{
				// Skip items that can't be processed due to state issues
			}
		}

		return successCount;
	}

	/// <summary>
	/// Executes an operation for each key-value pair and counts successful executions
	/// </summary>
	/// <typeparam name="TKey">The type of keys</typeparam>
	/// <typeparam name="TValue">The type of values</typeparam>
	/// <param name="items">The collection of key-value pairs to process</param>
	/// <param name="operation">The operation to perform on each key-value pair</param>
	/// <param name="shouldContinueOnError">Whether to continue processing if an operation fails</param>
	/// <returns>Number of successful operations</returns>
	public static int ExecuteWithCount<TKey, TValue>(
		IEnumerable<KeyValuePair<TKey, TValue>> items,
		Func<TKey, TValue, bool> operation,
		bool shouldContinueOnError = true)
	{
		ArgumentNullException.ThrowIfNull(items);
		ArgumentNullException.ThrowIfNull(operation);

		int successCount = 0;

		foreach (KeyValuePair<TKey, TValue> kvp in items)
		{
			try
			{
				if (operation(kvp.Key, kvp.Value))
				{
					successCount++;
				}
			}
			catch (ArgumentException) when (shouldContinueOnError)
			{
				// Skip invalid items
			}
			catch (InvalidOperationException) when (shouldContinueOnError)
			{
				// Skip items that can't be processed due to state issues
			}
		}

		return successCount;
	}
}
