// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ktsu.Semantics;

/// <summary>
/// Represents a semantic profile name
/// </summary>
public partial record class ProfileName : SemanticString<ProfileName>
{
	[GeneratedRegex(@"^[a-zA-Z0-9\s\-_]+$")]
	private static partial Regex ProfileNamePattern();

	/// <summary>
	/// Validates that the profile name follows the required format
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>ValidationResult indicating success or failure</returns>
	public static ValidationResult ValidateProfileName(string value) =>
		string.IsNullOrWhiteSpace(value) ? new ValidationResult("Profile name cannot be null or whitespace") :
		value.Length > 100 ? new ValidationResult("Profile name must be 100 characters or less") :
		value.Length < 2 ? new ValidationResult("Profile name must be at least 2 characters") :
		!ProfileNamePattern().IsMatch(value) ? new ValidationResult("Profile name must contain only letters, numbers, spaces, hyphens, and underscores") :
		ValidationResult.Success!;
}
