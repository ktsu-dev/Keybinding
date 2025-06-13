// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ktsu.Semantics;

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
	/// <returns>ValidationResult indicating success or failure</returns>
	public static ValidationResult ValidateChord(string value) =>
		string.IsNullOrWhiteSpace(value) ? new ValidationResult("Chord string cannot be null or whitespace") :
		value.Length > 200 ? new ValidationResult("Chord string must be 200 characters or less") :
		!ChordPattern().IsMatch(value) ? new ValidationResult("Chord string must contain only letters, numbers, +, spaces, hyphens, and underscores") :
		ValidationResult.Success!;
}
