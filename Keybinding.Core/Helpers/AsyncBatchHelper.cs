// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Helpers;

/// <summary>
/// Helper class for batch asynchronous operations to eliminate repeated code
/// </summary>
internal static class AsyncBatchHelper
{
	/// <summary>
	/// Executes an async operation for each item in a collection
	/// </summary>
	/// <typeparam name="T">The type of items in the collection</typeparam>
	/// <param name="items">The collection of items to process</param>
	/// <param name="operation">The async operation to perform on each item</param>
	/// <returns>Task representing the completion of all operations</returns>
	public static async Task ForEachAsync<T>(IEnumerable<T> items, Func<T, Task> operation)
	{
		Ensure.NotNull(items);
		Ensure.NotNull(operation);

		foreach (T item in items)
		{
			await operation(item).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Executes an async operation for each item in a collection and returns the results
	/// </summary>
	/// <typeparam name="TInput">The type of input items</typeparam>
	/// <typeparam name="TOutput">The type of output items</typeparam>
	/// <param name="items">The collection of items to process</param>
	/// <param name="operation">The async operation to perform on each item</param>
	/// <returns>Collection of results from the operations</returns>
	public static async Task<IReadOnlyCollection<TOutput>> SelectAsync<TInput, TOutput>(
		IEnumerable<TInput> items,
		Func<TInput, Task<TOutput>> operation)
	{
		Ensure.NotNull(items);
		Ensure.NotNull(operation);

		List<TOutput> results = [];

		foreach (TInput item in items)
		{
			TOutput result = await operation(item).ConfigureAwait(false);
			results.Add(result);
		}

		return results.AsReadOnly();
	}

	/// <summary>
	/// Executes a batch save operation with consistent error handling
	/// </summary>
	/// <typeparam name="T">The type of items to save</typeparam>
	/// <param name="items">The items to save</param>
	/// <param name="saveOperation">The save operation for each item</param>
	/// <param name="operationName">The name of the operation for error messages</param>
	/// <returns>Number of items successfully saved</returns>
	public static async Task<int> BatchSaveAsync<T>(
		IEnumerable<T> items,
		Func<T, Task> saveOperation,
		string operationName = "save")
	{
		Ensure.NotNull(items);
		Ensure.NotNull(saveOperation);

		int successCount = 0;

		foreach (T item in items)
		{
			try
			{
				await saveOperation(item).ConfigureAwait(false);
				successCount++;
			}
			catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
			{
				// Log error but continue with other items
				System.Diagnostics.Debug.WriteLine($"Failed to {operationName} item: {ex.Message}");
			}
		}

		return successCount;
	}
}
