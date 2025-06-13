// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ktsu.Semantics;

/// <summary>
/// Represents a semantic profile identifier
/// </summary>
public partial record class ProfileId : SemanticString<ProfileId>
{
	[GeneratedRegex(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")]
	private static partial Regex ProfileIdPattern();

	/// <summary>
	/// Validates that the profile ID follows the required format (kebab-case identifiers)
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>ValidationResult indicating success or failure</returns>
	public static ValidationResult ValidateProfileId(string value) =>
		string.IsNullOrWhiteSpace(value) ? new ValidationResult("Profile ID cannot be null or whitespace") :
		value.Length > 50 ? new ValidationResult("Profile ID must be 50 characters or less") :
		value.Length < 2 ? new ValidationResult("Profile ID must be at least 2 characters") :
		!ProfileIdPattern().IsMatch(value) ? new ValidationResult("Profile ID must use kebab-case format with lowercase letters, numbers, and hyphens") :
		ValidationResult.Success!;
}
