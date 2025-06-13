// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using ktsu.Semantics;

/// <summary>
/// Represents a semantic command description
/// </summary>
public partial record class CommandDescription : SemanticString<CommandDescription>
{
	/// <summary>
	/// Validates that the command description follows the required format
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>ValidationResult indicating success or failure</returns>
	public static ValidationResult ValidateCommandDescription(string value) =>
		string.IsNullOrWhiteSpace(value) ? new ValidationResult("Command description cannot be null or whitespace") :
		value.Length > 500 ? new ValidationResult("Command description must be 500 characters or less") :
		value.Length < 3 ? new ValidationResult("Command description must be at least 3 characters") :
		ValidationResult.Success!;
}
