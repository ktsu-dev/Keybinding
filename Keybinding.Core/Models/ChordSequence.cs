// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ktsu.Semantics;

/// <summary>
/// Represents a semantic phrase string for parsing (a sequence of chords)
/// </summary>
public partial record class PhraseString : SemanticString<PhraseString>
{
	[GeneratedRegex(@"^[a-zA-Z0-9+,\s\-_]+$")]
	private static partial Regex PhrasePattern();

	/// <summary>
	/// Validates that the phrase string follows the required format (comma-separated chord strings)
	/// </summary>
	/// <param name="value">The value to validate</param>
	/// <returns>ValidationResult indicating success or failure</returns>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3358:Ternary operators should not be nested", Justification = "<Pending>")]
	public static ValidationResult ValidatePhrase(string value) =>
		string.IsNullOrWhiteSpace(value)
		? new ValidationResult("Phrase string cannot be null or whitespace")
		: value.Length > 500
		? new ValidationResult("Phrase string must be 500 characters or less")
		: !PhrasePattern().IsMatch(value)
		? new ValidationResult("Phrase string must contain only letters, numbers, +, commas, spaces, hyphens, and underscores")
		: ValidationResult.Success!;
}
