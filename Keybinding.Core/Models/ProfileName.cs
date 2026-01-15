// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.Text.RegularExpressions;
using ktsu.Semantics.Strings;

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
	/// <returns>System.ComponentModel.DataAnnotations.ValidationResult indicating success or failure</returns>
	public static System.ComponentModel.DataAnnotations.ValidationResult ValidateProfileName(string value) =>
		string.IsNullOrWhiteSpace(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Profile name cannot be null or whitespace") :
		value.Length > 100 ? new System.ComponentModel.DataAnnotations.ValidationResult("Profile name must be 100 characters or less") :
		value.Length < 2 ? new System.ComponentModel.DataAnnotations.ValidationResult("Profile name must be at least 2 characters") :
		!ProfileNamePattern().IsMatch(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Profile name must contain only letters, numbers, spaces, hyphens, and underscores") :
		System.ComponentModel.DataAnnotations.ValidationResult.Success!;
}
