// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.Text.RegularExpressions;
using ktsu.Semantics.Strings;

/// <summary>
/// Represents a semantic command category for grouping
/// </summary>
public partial record class CommandCategory : SemanticString<CommandCategory>
{
	[GeneratedRegex(@"^[a-zA-Z0-9\s\-_]+$")]
	private static partial Regex CommandCategoryPattern();

	/// <summary>
	/// Validates that the command category follows the required format
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>System.ComponentModel.DataAnnotations.ValidationResult indicating success or failure</returns>
	public static System.ComponentModel.DataAnnotations.ValidationResult ValidateCommandCategory(string value) =>
		string.IsNullOrWhiteSpace(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Command category cannot be null or whitespace") :
		value.Length > 50 ? new System.ComponentModel.DataAnnotations.ValidationResult("Command category must be 50 characters or less") :
		value.Length < 2 ? new System.ComponentModel.DataAnnotations.ValidationResult("Command category must be at least 2 characters") :
		!CommandCategoryPattern().IsMatch(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Command category must contain only letters, numbers, spaces, hyphens, and underscores") :
		System.ComponentModel.DataAnnotations.ValidationResult.Success!;
}
