// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class KeyCombinationTests
{
	[TestMethod]
	public void Constructor_ValidInput_CreatesKeyCombination()
	{
		// Arrange & Act
		KeyCombination keyCombination = new("A", ModifierKeys.Ctrl);

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
		KeyCombination keyCombination = new("A", ModifierKeys.Ctrl);

		// Act
		string result = keyCombination.ToString();

		// Assert
		Assert.AreEqual("Ctrl+A", result);
	}

	[TestMethod]
	public void ToString_MultipleModifiers_ReturnsCorrectFormat()
	{
		// Arrange
		KeyCombination keyCombination = new("S", ModifierKeys.Ctrl | ModifierKeys.Alt);

		// Act
		string result = keyCombination.ToString();

		// Assert
		Assert.AreEqual("Ctrl+Alt+S", result);
	}

	[TestMethod]
	public void ToString_NoModifiers_ReturnsKeyOnly()
	{
		// Arrange
		KeyCombination keyCombination = new("Escape", ModifierKeys.None);

		// Act
		string result = keyCombination.ToString();

		// Assert
		Assert.AreEqual("Escape", result);
	}

	[TestMethod]
	public void Parse_ValidString_ReturnsKeyCombination()
	{
		// Act
		KeyCombination result = KeyCombination.Parse("Ctrl+Alt+S");

		// Assert
		Assert.AreEqual("S", result.Key);
		Assert.AreEqual(ModifierKeys.Ctrl | ModifierKeys.Alt, result.Modifiers);
	}

	[TestMethod]
	public void Parse_KeyOnly_ReturnsKeyCombinationWithNoModifiers()
	{
		// Act
		KeyCombination result = KeyCombination.Parse("Enter");

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
		bool success = KeyCombination.TryParse("Ctrl+C", out KeyCombination? result);

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
		bool success = KeyCombination.TryParse("Invalid+Format", out KeyCombination? result);

		// Assert
		Assert.IsFalse(success);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void Equals_SameKeyCombinations_ReturnsTrue()
	{
		// Arrange
		KeyCombination combo1 = new("A", ModifierKeys.Ctrl);
		KeyCombination combo2 = new("A", ModifierKeys.Ctrl);

		// Act & Assert
		Assert.AreEqual(combo1, combo2);
		Assert.IsTrue(combo1.Equals(combo2));
		Assert.IsTrue(combo1 == combo2);
	}

	[TestMethod]
	public void Equals_DifferentKeyCombinations_ReturnsFalse()
	{
		// Arrange
		KeyCombination combo1 = new("A", ModifierKeys.Ctrl);
		KeyCombination combo2 = new("B", ModifierKeys.Ctrl);

		// Act & Assert
		Assert.AreNotEqual(combo1, combo2);
		Assert.IsFalse(combo1.Equals(combo2));
		Assert.IsTrue(combo1 != combo2);
	}

	[TestMethod]
	public void GetHashCode_SameKeyCombinations_ReturnsSameHash()
	{
		// Arrange
		KeyCombination combo1 = new("A", ModifierKeys.Ctrl);
		KeyCombination combo2 = new("A", ModifierKeys.Ctrl);

		// Act & Assert
		Assert.AreEqual(combo1.GetHashCode(), combo2.GetHashCode());
	}
}
