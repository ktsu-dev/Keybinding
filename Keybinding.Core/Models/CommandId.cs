// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ktsu.Semantics;

/// <summary>
/// Represents a semantic command identifier with validation
/// </summary>
public partial record class CommandId : SemanticString<CommandId>
{
	[GeneratedRegex(@"^[a-z0-9]+(\.[a-z0-9]+)*$")]
	private static partial Regex CommandIdPattern();

	/// <summary>
	/// Validates that the command ID follows the required format (e.g., "edit.copy", "file.save")
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>ValidationResult indicating success or failure</returns>
	public static ValidationResult ValidateCommandId(string value) =>
		string.IsNullOrWhiteSpace(value) ? new ValidationResult("Command ID cannot be null or whitespace") :
		value.Length > 100 ? new ValidationResult("Command ID must be 100 characters or less") :
		!CommandIdPattern().IsMatch(value) ? new ValidationResult("Command ID must follow the format 'category.action' with lowercase letters, numbers, and dots") :
		ValidationResult.Success!;
}
