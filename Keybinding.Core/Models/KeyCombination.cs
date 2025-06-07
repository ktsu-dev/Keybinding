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
/// Represents special keys that require specific formatting
/// </summary>
public enum SpecialKeys
{
	/// <summary>Escape key</summary>
	Escape,
	/// <summary>Enter key</summary>
	Enter,
	/// <summary>Space key</summary>
	Space,
	/// <summary>Tab key</summary>
	Tab,
	/// <summary>Backspace key</summary>
	Backspace,
	/// <summary>Delete key</summary>
	Delete,
	/// <summary>Insert key</summary>
	Insert,
	/// <summary>Home key</summary>
	Home,
	/// <summary>End key</summary>
	End,
	/// <summary>Page Up key</summary>
	PageUp,
	/// <summary>Page Down key</summary>
	PageDown,
	/// <summary>Arrow Up key</summary>
	ArrowUp,
	/// <summary>Arrow Down key</summary>
	ArrowDown,
	/// <summary>Arrow Left key</summary>
	ArrowLeft,
	/// <summary>Arrow Right key</summary>
	ArrowRight,
	/// <summary>F1 function key</summary>
	F1,
	/// <summary>F2 function key</summary>
	F2,
	/// <summary>F3 function key</summary>
	F3,
	/// <summary>F4 function key</summary>
	F4,
	/// <summary>F5 function key</summary>
	F5,
	/// <summary>F6 function key</summary>
	F6,
	/// <summary>F7 function key</summary>
	F7,
	/// <summary>F8 function key</summary>
	F8,
	/// <summary>F9 function key</summary>
	F9,
	/// <summary>F10 function key</summary>
	F10,
	/// <summary>F11 function key</summary>
	F11,
	/// <summary>F12 function key</summary>
	F12,
	/// <summary>Caps Lock key</summary>
	CapsLock,
	/// <summary>Num Lock key</summary>
	NumLock,
	/// <summary>Scroll Lock key</summary>
	ScrollLock,
	/// <summary>Print Screen key</summary>
	PrintScreen,
	/// <summary>Pause key</summary>
	Pause
}

/// <summary>
/// Represents a key combination consisting of modifiers and a primary key
/// </summary>
public sealed class KeyCombination : IEquatable<KeyCombination>
{
	// Constants for modifier names
	private const string CtrlDisplayName = "Ctrl";
	private const string AltDisplayName = "Alt";
	private const string ShiftDisplayName = "Shift";
	private const string MetaDisplayName = "Meta";

	// Constants for modifier aliases in parsing
	private const string CtrlAlias = "CTRL";
	private const string ControlAlias = "CONTROL";
	private const string AltAlias = "ALT";
	private const string ShiftAlias = "SHIFT";
	private const string MetaAlias = "META";
	private const string WinAlias = "WIN";
	private const string WindowsAlias = "WINDOWS";
	private const string CmdAlias = "CMD";
	private const string CommandAlias = "COMMAND";

	// Constants for special key aliases
	private const string UpAlias = "UP";
	private const string DownAlias = "DOWN";
	private const string LeftAlias = "LEFT";
	private const string RightAlias = "RIGHT";

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
			parts.Add(CtrlDisplayName);
		}

		if (Modifiers.HasFlag(ModifierKeys.Alt))
		{
			parts.Add(AltDisplayName);
		}

		if (Modifiers.HasFlag(ModifierKeys.Shift))
		{
			parts.Add(ShiftDisplayName);
		}

		if (Modifiers.HasFlag(ModifierKeys.Meta))
		{
			parts.Add(MetaDisplayName);
		}

		parts.Add(FormatKeyForDisplay(Key));

		return string.Join("+", parts);
	}

	/// <summary>
	/// Formats a key for display purposes, converting special keys to proper case
	/// </summary>
	/// <param name="key">The key to format</param>
	/// <returns>Formatted key string</returns>
	private static string FormatKeyForDisplay(string key)
	{
		// Try to match the key to a special key enum
		string upperKey = key.ToUpperInvariant();

		// Handle special key aliases
		string normalizedKey = upperKey switch
		{
			UpAlias => nameof(SpecialKeys.ArrowUp).ToUpperInvariant(),
			DownAlias => nameof(SpecialKeys.ArrowDown).ToUpperInvariant(),
			LeftAlias => nameof(SpecialKeys.ArrowLeft).ToUpperInvariant(),
			RightAlias => nameof(SpecialKeys.ArrowRight).ToUpperInvariant(),
			_ => upperKey
		};

		// Try to parse as a special key enum
		if (Enum.TryParse(normalizedKey, true, out SpecialKeys specialKey))
		{
			return specialKey switch
			{
				SpecialKeys.Escape => nameof(SpecialKeys.Escape),
				SpecialKeys.Enter => nameof(SpecialKeys.Enter),
				SpecialKeys.Space => nameof(SpecialKeys.Space),
				SpecialKeys.Tab => nameof(SpecialKeys.Tab),
				SpecialKeys.Backspace => nameof(SpecialKeys.Backspace),
				SpecialKeys.Delete => nameof(SpecialKeys.Delete),
				SpecialKeys.Insert => nameof(SpecialKeys.Insert),
				SpecialKeys.Home => nameof(SpecialKeys.Home),
				SpecialKeys.End => nameof(SpecialKeys.End),
				SpecialKeys.PageUp => "PageUp",
				SpecialKeys.PageDown => "PageDown",
				SpecialKeys.ArrowUp => "ArrowUp",
				SpecialKeys.ArrowDown => "ArrowDown",
				SpecialKeys.ArrowLeft => "ArrowLeft",
				SpecialKeys.ArrowRight => "ArrowRight",
				SpecialKeys.F1 => nameof(SpecialKeys.F1),
				SpecialKeys.F2 => nameof(SpecialKeys.F2),
				SpecialKeys.F3 => nameof(SpecialKeys.F3),
				SpecialKeys.F4 => nameof(SpecialKeys.F4),
				SpecialKeys.F5 => nameof(SpecialKeys.F5),
				SpecialKeys.F6 => nameof(SpecialKeys.F6),
				SpecialKeys.F7 => nameof(SpecialKeys.F7),
				SpecialKeys.F8 => nameof(SpecialKeys.F8),
				SpecialKeys.F9 => nameof(SpecialKeys.F9),
				SpecialKeys.F10 => nameof(SpecialKeys.F10),
				SpecialKeys.F11 => nameof(SpecialKeys.F11),
				SpecialKeys.F12 => nameof(SpecialKeys.F12),
				SpecialKeys.CapsLock => "CapsLock",
				SpecialKeys.NumLock => "NumLock",
				SpecialKeys.ScrollLock => "ScrollLock",
				SpecialKeys.PrintScreen => "PrintScreen",
				SpecialKeys.Pause => nameof(SpecialKeys.Pause),
				_ => specialKey.ToString()
			};
		}

		// For single character keys and unrecognized keys, keep as-is
		return key;
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
				CtrlAlias or ControlAlias => ModifierKeys.Ctrl,
				AltAlias => ModifierKeys.Alt,
				ShiftAlias => ModifierKeys.Shift,
				MetaAlias or WinAlias or WindowsAlias or CmdAlias or CommandAlias => ModifierKeys.Meta,
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
		catch (ArgumentException)
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
