// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Models;

using System.Text;

/// <summary>
/// Splits chord and phrase strings into tokens, treating a separator character that appears where a key is
/// expected as the literal key, so "Ctrl++" is Ctrl and Plus and "Ctrl+," is Ctrl and Comma.
/// </summary>
internal static class KeyStringTokenizer
{
	private const char NoteSeparator = '+';
	private const char ChordSeparator = ',';
	private const string ChordSeparators = "+";
	private const string PhraseSeparators = "+,";

	/// <summary>
	/// Splits a chord string such as "Ctrl+Alt+S" or "Ctrl++" into note strings.
	/// </summary>
	/// <param name="value">The chord string.</param>
	/// <returns>The trimmed note strings, or an empty array for whitespace input.</returns>
	/// <exception cref="ArgumentException">Thrown when a key is missing, as in "Ctrl+" or "A++B".</exception>
	internal static string[] SplitChord(string value) => Split(value, NoteSeparator, ChordSeparators);

	/// <summary>
	/// Splits a phrase string such as "Ctrl+R, R" or "Ctrl+,, R" into chord strings.
	/// </summary>
	/// <param name="value">The phrase string.</param>
	/// <returns>The trimmed chord strings, or an empty array for whitespace input.</returns>
	/// <exception cref="ArgumentException">Thrown when a key or chord is missing, as in "Ctrl+" or "A,,B".</exception>
	internal static string[] SplitPhrase(string value) => Split(value, ChordSeparator, PhraseSeparators);

	private static string[] Split(string value, char splitOn, string separators)
	{
		List<string> tokens = [];
		StringBuilder current = new();
		bool expectKey = true;
		bool sawContent = false;

		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];

			if (separators.Contains(c) && !expectKey)
			{
				// A separator after a key ends the note, and ends the token when it is the one being split on
				if (c == splitOn)
				{
					tokens.Add(current.ToString().Trim());
					current.Clear();
				}
				else
				{
					current.Append(c);
				}

				expectKey = true;
				continue;
			}

			if (separators.Contains(c))
			{
				EnsureSeparatorIsKey(value, i, separators);
			}

			current.Append(c);
			if (!char.IsWhiteSpace(c))
			{
				expectKey = false;
				sawContent = true;
			}
		}

		if (!sawContent)
		{
			return [];
		}

		if (expectKey)
		{
			throw new ArgumentException($"Missing key at the end of \"{value}\"", nameof(value));
		}

		tokens.Add(current.ToString().Trim());
		return [.. tokens];
	}

	/// <summary>
	/// A separator where a key is expected is the key itself, but only when nothing else follows it before the next
	/// separator. Otherwise the input has an empty key, which must not be dropped.
	/// </summary>
	private static void EnsureSeparatorIsKey(string value, int index, string separators)
	{
		int next = index + 1;
		while (next < value.Length && char.IsWhiteSpace(value[next]))
		{
			next++;
		}

		if (next < value.Length && !separators.Contains(value[next]))
		{
			throw new ArgumentException($"Missing key before '{value[index]}' at position {index} in \"{value}\"", nameof(value));
		}
	}
}
