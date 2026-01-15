// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.Text.RegularExpressions;
using ktsu.Semantics.Strings;

/// <summary>
/// Represents a semantic note name - a single key identifier
/// </summary>
public partial record class NoteName : SemanticString<NoteName>
{
	[GeneratedRegex(@"^[A-Z0-9_]+$")]
	private static partial Regex NoteNamePattern();

	/// <summary>
	/// Validates that the note name follows the required format (uppercase letters, numbers, and underscores)
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>System.ComponentModel.DataAnnotations.ValidationResult indicating success or failure</returns>
	public static System.ComponentModel.DataAnnotations.ValidationResult ValidateNoteName(string value) =>
		string.IsNullOrWhiteSpace(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Note name cannot be null or whitespace") :
		value.Length > 50 ? new System.ComponentModel.DataAnnotations.ValidationResult("Note name must be 50 characters or less") :
		value.Length < 1 ? new System.ComponentModel.DataAnnotations.ValidationResult("Note name must be at least 1 character") :
		!NoteNamePattern().IsMatch(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Note name must contain only uppercase letters, numbers, and underscores") :
		System.ComponentModel.DataAnnotations.ValidationResult.Success!;
}
