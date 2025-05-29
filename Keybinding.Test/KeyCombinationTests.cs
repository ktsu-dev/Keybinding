// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Test;

[TestClass]
public class KeyCombinationTests
{
	[TestMethod]
	public void Constructor_ValidInput_CreatesKeyCombination()
	{
		// Arrange & Act
		var keyCombination = new KeyCombination("A", ModifierKeys.Ctrl);

		// Assert
		Assert.AreEqual("A", keyCombination.Key);
		Assert.AreEqual(ModifierKeys.Ctrl, keyCombination.Modifiers);
	}

	[TestMethod]
	public void Constructor_NullKey_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentException>(() => new KeyCombination(null!, ModifierKeys.None));
	}

	[TestMethod]
	public void Constructor_WhitespaceKey_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentException>(() => new KeyCombination("   ", ModifierKeys.None));
	}

	[TestMethod]
	public void ToString_SingleModifier_ReturnsCorrectFormat()
	{
		// Arrange
		var keyCombination = new KeyCombination("A", ModifierKeys.Ctrl);

		// Act
		var result = keyCombination.ToString();

		// Assert
		Assert.AreEqual("Ctrl+A", result);
	}

	[TestMethod]
	public void ToString_MultipleModifiers_ReturnsCorrectFormat()
	{
		// Arrange
		var keyCombination = new KeyCombination("S", ModifierKeys.Ctrl | ModifierKeys.Alt);

		// Act
		var result = keyCombination.ToString();

		// Assert
		Assert.AreEqual("Ctrl+Alt+S", result);
	}

	[TestMethod]
	public void ToString_NoModifiers_ReturnsKeyOnly()
	{
		// Arrange
		var keyCombination = new KeyCombination("Escape", ModifierKeys.None);

		// Act
		var result = keyCombination.ToString();

		// Assert
		Assert.AreEqual("Escape", result);
	}

	[TestMethod]
	public void Parse_ValidString_ReturnsKeyCombination()
	{
		// Act
		var result = KeyCombination.Parse("Ctrl+Alt+S");

		// Assert
		Assert.AreEqual("S", result.Key);
		Assert.AreEqual(ModifierKeys.Ctrl | ModifierKeys.Alt, result.Modifiers);
	}

	[TestMethod]
	public void Parse_KeyOnly_ReturnsKeyCombinationWithNoModifiers()
	{
		// Act
		var result = KeyCombination.Parse("Enter");

		// Assert
		Assert.AreEqual("ENTER", result.Key);
		Assert.AreEqual(ModifierKeys.None, result.Modifiers);
	}

	[TestMethod]
	public void Parse_InvalidModifier_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentException>(() => KeyCombination.Parse("Invalid+A"));
	}

	[TestMethod]
	public void TryParse_ValidString_ReturnsTrue()
	{
		// Act
		var success = KeyCombination.TryParse("Ctrl+C", out var result);

		// Assert
		Assert.IsTrue(success);
		Assert.IsNotNull(result);
		Assert.AreEqual("C", result.Key);
		Assert.AreEqual(ModifierKeys.Ctrl, result.Modifiers);
	}

	[TestMethod]
	public void TryParse_InvalidString_ReturnsFalse()
	{
		// Act
		var success = KeyCombination.TryParse("Invalid+Format", out var result);

		// Assert
		Assert.IsFalse(success);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void Equals_SameKeyCombinations_ReturnsTrue()
	{
		// Arrange
		var combo1 = new KeyCombination("A", ModifierKeys.Ctrl);
		var combo2 = new KeyCombination("A", ModifierKeys.Ctrl);

		// Act & Assert
		Assert.AreEqual(combo1, combo2);
		Assert.IsTrue(combo1.Equals(combo2));
		Assert.IsTrue(combo1 == combo2);
	}

	[TestMethod]
	public void Equals_DifferentKeyCombinations_ReturnsFalse()
	{
		// Arrange
		var combo1 = new KeyCombination("A", ModifierKeys.Ctrl);
		var combo2 = new KeyCombination("B", ModifierKeys.Ctrl);

		// Act & Assert
		Assert.AreNotEqual(combo1, combo2);
		Assert.IsFalse(combo1.Equals(combo2));
		Assert.IsTrue(combo1 != combo2);
	}

	[TestMethod]
	public void GetHashCode_SameKeyCombinations_ReturnsSameHash()
	{
		// Arrange
		var combo1 = new KeyCombination("A", ModifierKeys.Ctrl);
		var combo2 = new KeyCombination("A", ModifierKeys.Ctrl);

		// Act & Assert
		Assert.AreEqual(combo1.GetHashCode(), combo2.GetHashCode());
	}
}
