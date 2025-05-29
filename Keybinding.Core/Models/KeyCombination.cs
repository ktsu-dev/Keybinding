// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;
/// <summary>
/// Represents the modifier keys for a key combination
/// </summary>
[Flags]
public enum ModifierKeys
{
	/// <summary>
	/// No modifier keys
	/// </summary>
	None = 0,

	/// <summary>
	/// Control key modifier
	/// </summary>
	Ctrl = 1,

	/// <summary>
	/// Alt key modifier
	/// </summary>
	Alt = 2,

	/// <summary>
	/// Shift key modifier
	/// </summary>
	Shift = 4,

	/// <summary>
	/// Windows/Meta key modifier
	/// </summary>
	Meta = 8
}

/// <summary>
/// Represents a key combination consisting of modifiers and a primary key
/// </summary>
public sealed class KeyCombination : IEquatable<KeyCombination>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="KeyCombination"/> class
	/// </summary>
	/// <param name="key">The primary key</param>
	/// <param name="modifiers">The modifier keys</param>
	/// <exception cref="ArgumentException">Thrown when key is null or whitespace</exception>
	public KeyCombination(string key, ModifierKeys modifiers = ModifierKeys.None)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
		}

		Key = key.Trim().ToUpperInvariant();
		Modifiers = modifiers;
	}

	/// <summary>
	/// Gets the primary key
	/// </summary>
	public string Key { get; }

	/// <summary>
	/// Gets the modifier keys
	/// </summary>
	public ModifierKeys Modifiers { get; }

	/// <summary>
	/// Returns a string representation of the key combination
	/// </summary>
	/// <returns>String representation in format "Ctrl+Alt+Key"</returns>
	public override string ToString()
	{
		List<string> parts = [];

		if (Modifiers.HasFlag(ModifierKeys.Ctrl))
		{
			parts.Add("Ctrl");
		}

		if (Modifiers.HasFlag(ModifierKeys.Alt))
		{
			parts.Add("Alt");
		}

		if (Modifiers.HasFlag(ModifierKeys.Shift))
		{
			parts.Add("Shift");
		}

		if (Modifiers.HasFlag(ModifierKeys.Meta))
		{
			parts.Add("Meta");
		}

		parts.Add(Key);

		return string.Join("+", parts);
	}

	/// <summary>
	/// Parses a string representation of a key combination
	/// </summary>
	/// <param name="value">String in format "Ctrl+Alt+Key"</param>
	/// <returns>Parsed KeyCombination</returns>
	/// <exception cref="ArgumentException">Thrown when value format is invalid</exception>
	public static KeyCombination Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value cannot be null or whitespace", nameof(value));
		}

		List<string> parts = [.. value.Split('+', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())];

		if (parts.Count == 0)
		{
			throw new ArgumentException("Invalid key combination format", nameof(value));
		}

		ModifierKeys modifiers = ModifierKeys.None;
		string key = parts.Last();

		for (int i = 0; i < parts.Count - 1; i++)
		{
			ModifierKeys modifier = parts[i].ToUpperInvariant() switch
			{
				"CTRL" or "CONTROL" => ModifierKeys.Ctrl,
				"ALT" => ModifierKeys.Alt,
				"SHIFT" => ModifierKeys.Shift,
				"META" or "WIN" or "WINDOWS" or "CMD" or "COMMAND" => ModifierKeys.Meta,
				_ => throw new ArgumentException($"Unknown modifier: {parts[i]}", nameof(value))
			};

			modifiers |= modifier;
		}

		return new KeyCombination(key, modifiers);
	}

	/// <summary>
	/// Tries to parse a string representation of a key combination
	/// </summary>
	/// <param name="value">String to parse</param>
	/// <param name="result">Parsed KeyCombination if successful</param>
	/// <returns>True if parsing was successful, false otherwise</returns>
	public static bool TryParse(string value, out KeyCombination? result)
	{
		try
		{
			result = Parse(value);
			return true;
		}
		catch
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc/>
	public bool Equals(KeyCombination? other) => other is not null && (ReferenceEquals(this, other) || (Key == other.Key && Modifiers == other.Modifiers));

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is KeyCombination other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Key, Modifiers);

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(KeyCombination? left, KeyCombination? right) =>
		left?.Equals(right) ?? right is null;

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(KeyCombination? left, KeyCombination? right) =>
		!(left == right);
}
