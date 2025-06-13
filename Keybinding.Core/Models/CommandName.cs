// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ktsu.Semantics;

/// <summary>
/// Represents a semantic command name with validation
/// </summary>
public partial record class CommandName : SemanticString<CommandName>
{
	[GeneratedRegex(@"^[a-zA-Z0-9\s\-_]+$")]
	private static partial Regex CommandNamePattern();

	/// <summary>
	/// Validates that the command name follows the required format
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>ValidationResult indicating success or failure</returns>
	public static ValidationResult ValidateCommandName(string value) =>
		string.IsNullOrWhiteSpace(value) ? new ValidationResult("Command name cannot be null or whitespace") :
		value.Length > 100 ? new ValidationResult("Command name must be 100 characters or less") :
		value.Length < 2 ? new ValidationResult("Command name must be at least 2 characters") :
		!CommandNamePattern().IsMatch(value) ? new ValidationResult("Command name must contain only letters, numbers, spaces, hyphens, and underscores") :
		ValidationResult.Success!;
}
