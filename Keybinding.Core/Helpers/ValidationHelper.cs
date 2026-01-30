// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Helpers;

/// <summary>
/// Helper class for common validation operations to eliminate repeated code
/// </summary>
internal static class ValidationHelper
{
	/// <summary>
	/// Validates that a string parameter is not null or whitespace
	/// </summary>
	/// <param name="value">The string value to validate</param>
	/// <param name="parameterName">The parameter name for the exception</param>
	/// <param name="customMessage">Optional custom message</param>
	/// <exception cref="ArgumentNullException">Thrown when the string is null</exception>
	/// <exception cref="ArgumentException">Thrown when the string is whitespace</exception>
	public static void ThrowIfNullOrWhiteSpace(string? value, string parameterName, string? customMessage = null)
	{
		Ensure.NotNull(value, parameterName);

		if (string.IsNullOrWhiteSpace(value))
		{
			string message = customMessage ?? $"{parameterName} cannot be null or whitespace";
			throw new ArgumentException(message, parameterName);
		}
	}

	/// <summary>
	/// Validates that a string parameter is not null or whitespace and returns the trimmed value
	/// </summary>
	/// <param name="value">The string value to validate</param>
	/// <param name="parameterName">The parameter name for the exception</param>
	/// <param name="customMessage">Optional custom message</param>
	/// <returns>The trimmed string value</returns>
	/// <exception cref="ArgumentException">Thrown when the string is null or whitespace</exception>
	public static string ValidateAndTrim(string? value, string parameterName, string? customMessage = null)
	{
		ThrowIfNullOrWhiteSpace(value, parameterName, customMessage);
		return value!.Trim();
	}

	/// <summary>
	/// Validates multiple string parameters at once
	/// </summary>
	/// <param name="validations">Tuples of (value, parameterName, customMessage)</param>
	/// <exception cref="ArgumentException">Thrown when any string is null or whitespace</exception>
	public static void ThrowIfAnyNullOrWhiteSpace(params (string? value, string parameterName, string? customMessage)[] validations)
	{
		foreach ((string? value, string parameterName, string? customMessage) in validations)
		{
			ThrowIfNullOrWhiteSpace(value, parameterName, customMessage);
		}
	}

	/// <summary>
	/// Checks if any of the provided strings are null or whitespace
	/// </summary>
	/// <param name="values">The string values to check</param>
	/// <returns>True if any string is null or whitespace, false otherwise</returns>
	public static bool AnyNullOrWhiteSpace(params string?[] values) => values.Any(string.IsNullOrWhiteSpace);
}
