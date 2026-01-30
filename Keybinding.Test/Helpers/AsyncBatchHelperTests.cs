// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ktsu.Keybinding.Core.Helpers;

[TestClass]
public class AsyncBatchHelperTests
{
	[TestMethod]
	public async Task ForEachAsync_WithValidItems_ExecutesOperationForEach()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4, 5];
		List<int> processedItems = [];

		// Act
		await AsyncBatchHelper.ForEachAsync(items, async item =>
		{
			await Task.Delay(1).ConfigureAwait(false);
			processedItems.Add(item * 2);
		}).ConfigureAwait(false);

		// Assert
		Assert.HasCount(5, processedItems);
		CollectionAssert.AreEqual(new List<int> { 2, 4, 6, 8, 10 }, processedItems);
	}

	[TestMethod]
	public async Task ForEachAsync_WithEmptyCollection_DoesNothing()
	{
		// Arrange
		List<int> items = [];
		bool operationCalled = false;

		// Act
		await AsyncBatchHelper.ForEachAsync(items, async item =>
		{
			await Task.Delay(1).ConfigureAwait(false);
			operationCalled = true;
		}).ConfigureAwait(false);

		// Assert
		Assert.IsFalse(operationCalled, "Operation should not be called for empty collection");
	}

	[TestMethod]
	public async Task ForEachAsync_WithNullItems_ThrowsArgumentNullException()
	{
		// Arrange
		IEnumerable<int>? nullItems = null;

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(
			() => AsyncBatchHelper.ForEachAsync(nullItems!, async item =>
				await Task.Delay(1).ConfigureAwait(false)
			)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task ForEachAsync_WithNullOperation_ThrowsArgumentNullException()
	{
		// Arrange
		List<int> items = [1, 2, 3];
		Func<int, Task>? nullOperation = null;

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(
			() => AsyncBatchHelper.ForEachAsync(items, nullOperation!)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SelectAsync_WithValidItems_ReturnsTransformedResults()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4];

		// Act
		IReadOnlyCollection<string> results = await AsyncBatchHelper.SelectAsync(items, async item =>
		{
			await Task.Delay(1).ConfigureAwait(false);
			return $"Item_{item}";
		}).ConfigureAwait(false);

		// Assert
		Assert.HasCount(4, results);
		string[] expectedResults = ["Item_1", "Item_2", "Item_3", "Item_4"];
		CollectionAssert.AreEqual(expectedResults, results.ToArray());
	}

	[TestMethod]
	public async Task SelectAsync_WithEmptyCollection_ReturnsEmptyCollection()
	{
		// Arrange
		List<int> items = [];

		// Act
		IReadOnlyCollection<string> results = await AsyncBatchHelper.SelectAsync(items, async item =>
		{
			await Task.Delay(1).ConfigureAwait(false);
			return item.ToString();
		}).ConfigureAwait(false);

		// Assert
		Assert.IsEmpty(results);
	}

	[TestMethod]
	public async Task BatchSaveAsync_WithValidItems_ReturnsSuccessCount()
	{
		// Arrange
		List<string> items = ["item1", "item2", "item3"];
		List<string> savedItems = [];

		// Act
		int successCount = await AsyncBatchHelper.BatchSaveAsync(items, async item =>
		{
			await Task.Delay(1).ConfigureAwait(false);
			savedItems.Add(item);
		}).ConfigureAwait(false);

		// Assert
		Assert.AreEqual(3, successCount);
		Assert.HasCount(3, savedItems);
		CollectionAssert.AreEqual(items, savedItems);
	}

	[TestMethod]
	public async Task BatchSaveAsync_WithFailingItems_ContinuesAndReturnsPartialSuccess()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4, 5];
		List<int> savedItems = [];

		// Act
		int successCount = await AsyncBatchHelper.BatchSaveAsync(items, async item =>
		{
			await Task.Delay(1).ConfigureAwait(false);
			if (item == 3) // Simulate failure for item 3
			{
				throw new InvalidOperationException("Save failed");
			}
			savedItems.Add(item);
		}).ConfigureAwait(false);

		// Assert
		Assert.AreEqual(4, successCount); // 4 out of 5 succeeded
		Assert.HasCount(4, savedItems);
		CollectionAssert.AreEqual(new List<int> { 1, 2, 4, 5 }, savedItems);
	}

	[TestMethod]
	public async Task BatchSaveAsync_WithNullItems_ThrowsArgumentNullException()
	{
		// Arrange
		IEnumerable<int>? nullItems = null;

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(
			() => AsyncBatchHelper.BatchSaveAsync(nullItems!, async item =>
				await Task.Delay(1).ConfigureAwait(false)
			)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task BatchSaveAsync_WithNullOperation_ThrowsArgumentNullException()
	{
		// Arrange
		List<int> items = [1, 2, 3];
		Func<int, Task>? nullOperation = null;

		// Act & Assert
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(
			() => AsyncBatchHelper.BatchSaveAsync(items, nullOperation!)).ConfigureAwait(false);
	}
}
