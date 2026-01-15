// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using ktsu.Semantics.Strings;

/// <summary>
/// Represents a semantic command description
/// </summary>
public partial record class CommandDescription : SemanticString<CommandDescription>
{
	/// <summary>
	/// Validates that the command description follows the required format
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>System.ComponentModel.DataAnnotations.ValidationResult indicating success or failure</returns>
	public static System.ComponentModel.DataAnnotations.ValidationResult ValidateCommandDescription(string value) =>
		string.IsNullOrWhiteSpace(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Command description cannot be null or whitespace") :
		value.Length > 500 ? new System.ComponentModel.DataAnnotations.ValidationResult("Command description must be 500 characters or less") :
		value.Length < 3 ? new System.ComponentModel.DataAnnotations.ValidationResult("Command description must be at least 3 characters") :
		System.ComponentModel.DataAnnotations.ValidationResult.Success!;
}
