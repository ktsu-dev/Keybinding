// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ktsu.Keybinding.Core.Helpers;

[TestClass]
public class OperationHelperTests
{
	[TestMethod]
	public void ExecuteWithCount_WithAllSuccessfulOperations_ReturnsCorrectCount()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4, 5];
		static bool IsEven(int item) => item % 2 == 0;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, IsEven);

		// Assert
		Assert.AreEqual(2, successCount); // 2 and 4 are even
	}

	[TestMethod]
	public void ExecuteWithCount_WithAllFailingOperations_ReturnsZero()
	{
		// Arrange
		List<int> items = [1, 3, 5, 7, 9];
		static bool IsEven(int item) => item % 2 == 0;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, IsEven);

		// Assert
		Assert.AreEqual(0, successCount); // No even numbers
	}

	[TestMethod]
	public void ExecuteWithCount_WithOperationThrowingArgumentException_SkipsAndContinues()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4, 5];
		static bool ThrowingOperation(int item) => item == 3
			? throw new ArgumentException("Invalid item")
			: item % 2 == 0;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, ThrowingOperation);

		// Assert
		Assert.AreEqual(2, successCount); // 2 and 4 are even, 3 throws but is skipped
	}

	[TestMethod]
	public void ExecuteWithCount_WithOperationThrowingInvalidOperationException_SkipsAndContinues()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4, 5];
		static bool ThrowingOperation(int item) => item == 4
			? throw new InvalidOperationException("Invalid state")
			: item % 2 == 0;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, ThrowingOperation);

		// Assert
		Assert.AreEqual(1, successCount); // Only 2 is even, 4 throws but is skipped
	}

	[TestMethod]
	public void ExecuteWithCount_WithShouldContinueOnErrorFalse_StopsOnFirstException()
	{
		// Arrange
		List<int> items = [1, 2, 3, 4, 5];
		static bool ThrowingOperation(int item) => item == 3
			? throw new ArgumentException("Invalid item")
			: item % 2 == 0;

		// Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => OperationHelper.ExecuteWithCount(items, ThrowingOperation, shouldContinueOnError: false));

		Assert.AreEqual("Invalid item", exception.Message);
	}

	[TestMethod]
	public void ExecuteWithCount_WithEmptyCollection_ReturnsZero()
	{
		// Arrange
		List<int> items = [];
		static bool AlwaysTrue(int item) => true;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, AlwaysTrue);

		// Assert
		Assert.AreEqual(0, successCount);
	}

	[TestMethod]
	public void ExecuteWithCount_WithNullItems_ThrowsArgumentNullException()
	{
		// Arrange
		IEnumerable<int>? nullItems = null;
		static bool AlwaysTrue(int item) => true;

		// Act & Assert
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => OperationHelper.ExecuteWithCount(nullItems!, AlwaysTrue));

		Assert.AreEqual("items", exception.ParamName);
	}

	[TestMethod]
	public void ExecuteWithCount_WithNullOperation_ThrowsArgumentNullException()
	{
		// Arrange
		List<int> items = [1, 2, 3];
		Func<int, bool>? nullOperation = null;

		// Act & Assert
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => OperationHelper.ExecuteWithCount(items, nullOperation!));

		Assert.AreEqual("operation", exception.ParamName);
	}

	[TestMethod]
	public void ExecuteWithCount_KeyValuePair_WithAllSuccessfulOperations_ReturnsCorrectCount()
	{
		// Arrange
		Dictionary<string, int> items = new()
		{
			["key1"] = 2,
			["key2"] = 4,
			["key3"] = 1,
			["key4"] = 6
		};
		static bool IsValueEven(string key, int value) => value % 2 == 0;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, IsValueEven);

		// Assert
		Assert.AreEqual(3, successCount); // 2, 4, and 6 are even
	}

	[TestMethod]
	public void ExecuteWithCount_KeyValuePair_WithOperationThrowingException_SkipsAndContinues()
	{
		// Arrange
		Dictionary<string, int> items = new()
		{
			["key1"] = 2,
			["key2"] = 4,
			["key3"] = 1,
			["key4"] = 6
		};
		static bool ThrowingOperation(string key, int value) => key == "key2"
			? throw new ArgumentException("Invalid key")
			: value % 2 == 0;

		// Act
		int successCount = OperationHelper.ExecuteWithCount(items, ThrowingOperation);

		// Assert
		Assert.AreEqual(2, successCount); // 2 and 6 are even, key2 throws but is skipped
	}

	[TestMethod]
	public void ExecuteWithCount_KeyValuePair_WithShouldContinueOnErrorFalse_StopsOnFirstException()
	{
		// Arrange
		Dictionary<string, int> items = new()
		{
			["key1"] = 2,
			["key2"] = 4,
		};
		static bool ThrowingOperation(string key, int value) => key == "key1"
			? throw new ArgumentException("Invalid key")
			: value % 2 == 0;

		// Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => OperationHelper.ExecuteWithCount(items, ThrowingOperation, shouldContinueOnError: false));

		Assert.AreEqual("Invalid key", exception.Message);
	}

	[TestMethod]
	public void ExecuteWithCount_KeyValuePair_WithNullItems_ThrowsArgumentNullException()
	{
		// Arrange
		IEnumerable<KeyValuePair<string, int>>? nullItems = null;
		static bool AlwaysTrue(string key, int value) => true;

		// Act & Assert
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => OperationHelper.ExecuteWithCount(nullItems!, AlwaysTrue));

		Assert.AreEqual("items", exception.ParamName);
	}

	[TestMethod]
	public void ExecuteWithCount_KeyValuePair_WithNullOperation_ThrowsArgumentNullException()
	{
		// Arrange
		Dictionary<string, int> items = new() { ["key"] = 1 };
		Func<string, int, bool>? nullOperation = null;

		// Act & Assert
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => OperationHelper.ExecuteWithCount(items, nullOperation!));

		Assert.AreEqual("operation", exception.ParamName);
	}
}
