// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.Text.RegularExpressions;
using ktsu.Semantics.Strings;

/// <summary>
/// Represents a semantic chord string (key combination)
/// </summary>
public partial record class ChordString : SemanticString<ChordString>
{
	[GeneratedRegex(@"^[a-zA-Z0-9+\s\-_]+$")]
	private static partial Regex ChordPattern();

	/// <summary>
	/// Validates that the chord string follows the required format (plus-separated key names)
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>System.ComponentModel.DataAnnotations.ValidationResult indicating success or failure</returns>
	public static System.ComponentModel.DataAnnotations.ValidationResult ValidateChord(string value) =>
		string.IsNullOrWhiteSpace(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Chord string cannot be null or whitespace") :
		value.Length > 200 ? new System.ComponentModel.DataAnnotations.ValidationResult("Chord string must be 200 characters or less") :
		!ChordPattern().IsMatch(value) ? new System.ComponentModel.DataAnnotations.ValidationResult("Chord string must contain only letters, numbers, +, spaces, hyphens, and underscores") :
		System.ComponentModel.DataAnnotations.ValidationResult.Success!;
}
