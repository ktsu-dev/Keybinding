// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ktsu.Keybinding.Core.Helpers;

[TestClass]
public class ValidationHelperTests
{
	[TestMethod]
	public void ThrowIfNullOrWhiteSpace_WithValidString_DoesNotThrow()
	{
		// Arrange
		string validString = "valid";

		// Act & Assert
		ValidationHelper.ThrowIfNullOrWhiteSpace(validString, nameof(validString));
		// No exception should be thrown
	}

	[TestMethod]
	public void ThrowIfNullOrWhiteSpace_WithNullString_ThrowsArgumentNullException()
	{
		// Arrange
		string? nullString = null;

		// Act & Assert
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => ValidationHelper.ThrowIfNullOrWhiteSpace(nullString, nameof(nullString)));

		Assert.AreEqual(nameof(nullString), exception.ParamName);
	}

	[TestMethod]
	public void ThrowIfNullOrWhiteSpace_WithEmptyString_ThrowsArgumentException()
	{
		// Arrange
		string emptyString = "";

		// Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => ValidationHelper.ThrowIfNullOrWhiteSpace(emptyString, nameof(emptyString)));

		Assert.AreEqual(nameof(emptyString), exception.ParamName);
		Assert.IsTrue(exception.Message.Contains("cannot be null or whitespace"), "Exception message should indicate null or whitespace error");
	}

	[TestMethod]
	public void ThrowIfNullOrWhiteSpace_WithWhitespaceString_ThrowsArgumentException()
	{
		// Arrange
		string whitespaceString = "   ";

		// Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => ValidationHelper.ThrowIfNullOrWhiteSpace(whitespaceString, nameof(whitespaceString)));

		Assert.AreEqual(nameof(whitespaceString), exception.ParamName);
	}

	[TestMethod]
	public void ThrowIfNullOrWhiteSpace_WithCustomMessage_UsesCustomMessage()
	{
		// Arrange
		string emptyString = "";
		string customMessage = "Custom error message";

		// Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => ValidationHelper.ThrowIfNullOrWhiteSpace(emptyString, nameof(emptyString), customMessage));

		Assert.IsTrue(exception.Message.Contains(customMessage), "Exception message should contain the custom message");
	}

	[TestMethod]
	public void ValidateAndTrim_WithValidString_ReturnsTrimmedValue()
	{
		// Arrange
		string stringWithSpaces = "  valid  ";

		// Act
		string result = ValidationHelper.ValidateAndTrim(stringWithSpaces, nameof(stringWithSpaces));

		// Assert
		Assert.AreEqual("valid", result);
	}

	[TestMethod]
	public void ValidateAndTrim_WithNullString_ThrowsArgumentNullException()
	{
		// Arrange
		string? nullString = null;

		// Act & Assert
		ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(
			() => ValidationHelper.ValidateAndTrim(nullString, nameof(nullString)));

		Assert.AreEqual(nameof(nullString), exception.ParamName);
	}

	[TestMethod]
	public void ThrowIfAnyNullOrWhiteSpace_WithAllValidStrings_DoesNotThrow()
	{
		// Arrange & Act & Assert
		ValidationHelper.ThrowIfAnyNullOrWhiteSpace(
			("valid1", "param1", null),
			("valid2", "param2", null),
			("valid3", "param3", null));
		// No exception should be thrown
	}

	[TestMethod]
	public void ThrowIfAnyNullOrWhiteSpace_WithOneInvalidString_ThrowsException()
	{
		// Arrange & Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => ValidationHelper.ThrowIfAnyNullOrWhiteSpace(
				("valid", "param1", null),
				("", "param2", null),
				("valid", "param3", null)));

		Assert.AreEqual("param2", exception.ParamName);
	}

	[TestMethod]
	public void ThrowIfAnyNullOrWhiteSpace_WithCustomMessages_UsesCorrectMessage()
	{
		// Arrange
		string customMessage = "Custom validation error";

		// Act & Assert
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => ValidationHelper.ThrowIfAnyNullOrWhiteSpace(
				("valid", "param1", null),
				("", "param2", customMessage)));

		Assert.IsTrue(exception.Message.Contains(customMessage), "Exception message should contain the custom message");
		Assert.AreEqual("param2", exception.ParamName);
	}

	[TestMethod]
	public void AnyNullOrWhiteSpace_WithAllValidStrings_ReturnsFalse()
	{
		// Arrange
		string[] validStrings = ["valid1", "valid2", "valid3"];

		// Act
		bool result = ValidationHelper.AnyNullOrWhiteSpace(validStrings);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void AnyNullOrWhiteSpace_WithOneInvalidString_ReturnsTrue()
	{
		// Arrange
		string?[] mixedStrings = ["valid1", null, "valid3"];

		// Act
		bool result = ValidationHelper.AnyNullOrWhiteSpace(mixedStrings);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void AnyNullOrWhiteSpace_WithEmptyArray_ReturnsFalse()
	{
		// Arrange
		string[] emptyArray = [];

		// Act
		bool result = ValidationHelper.AnyNullOrWhiteSpace(emptyArray);

		// Assert
		Assert.IsFalse(result);
	}
}
