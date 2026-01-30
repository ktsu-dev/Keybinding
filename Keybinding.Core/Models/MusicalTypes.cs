// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a musical note - a single key
/// </summary>
public sealed class Note : IEquatable<Note>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Note"/> class
	/// </summary>
	/// <param name="key">The key that this note represents</param>
	/// <exception cref="ArgumentException">Thrown when key is null or whitespace</exception>
	[JsonConstructor]
	public Note(NoteName key)
	{
		Ensure.NotNull(key);
		Key = key;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Note"/> class from a string
	/// </summary>
	/// <param name="key">The key string that this note represents</param>
	/// <exception cref="ArgumentException">Thrown when key is null or whitespace</exception>
	public Note(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
		}

		Key = NoteName.Create(key.Trim().ToUpperInvariant());
	}

	/// <summary>
	/// Gets the key that this note represents
	/// </summary>
	public NoteName Key { get; }

	/// <summary>
	/// Returns a string representation of the note
	/// </summary>
	/// <returns>The key name</returns>
	public override string ToString() => Key.ToString();

	/// <inheritdoc/>
	public bool Equals(Note? other) => other is not null && Key.Equals(other.Key);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Note other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => Key.GetHashCode();

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(Note? left, Note? right) => (left?.Equals(right)) ?? (right is null);

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(Note? left, Note? right) => !(left == right);

	/// <summary>
	/// Implicitly converts a string to a Note
	/// </summary>
	/// <param name="key">The key string</param>
	/// <returns>A new Note instance</returns>
	public static implicit operator Note(string key) => new(key);

	/// <summary>
	/// Creates a Note from a string (explicit alternative to implicit operator)
	/// </summary>
	/// <param name="key">The key string</param>
	/// <returns>A new Note instance</returns>
	public static Note FromString(string key) => new(key);
}

/// <summary>
/// Represents a musical chord - a combination of notes pressed simultaneously
/// </summary>
public sealed class Chord : IEquatable<Chord>
{
	private readonly List<Note> _notes;

	/// <summary>
	/// Initializes a new instance of the <see cref="Chord"/> class
	/// </summary>
	/// <param name="notes">The notes that make up this chord</param>
	/// <exception cref="ArgumentException">Thrown when notes collection is null or empty</exception>
	public Chord(IEnumerable<Note> notes)
	{
		Ensure.NotNull(notes);
		_notes = [.. notes.Distinct().OrderBy(n => n.Key.ToString())];

		if (_notes.Count == 0)
		{
			throw new ArgumentException("Chord must contain at least one note", nameof(notes));
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Chord"/> class with a single note
	/// </summary>
	/// <param name="note">The note</param>
	/// <exception cref="ArgumentNullException">Thrown when note is null</exception>
	public Chord(Note note)
	{
		Ensure.NotNull(note);
		_notes = [note];
	}

	/// <summary>
	/// Gets all notes in this chord
	/// </summary>
	public IReadOnlyList<Note> Notes => _notes.AsReadOnly();

	/// <summary>
	/// Returns a string representation of the chord
	/// </summary>
	/// <returns>String representation in format "Ctrl+Alt+Key" or "Note1+Note2+Note3"</returns>
	public override string ToString()
	{
		List<string> parts = [];

		// Group modifier notes and regular notes for consistent display
		List<Note> modifierNotes = [];
		List<Note> regularNotes = [];

		foreach (Note note in _notes)
		{
			if (IsModifierNote(note))
			{
				modifierNotes.Add(note);
			}
			else
			{
				regularNotes.Add(note);
			}
		}

		// Add modifiers first in a consistent order
		foreach (Note note in modifierNotes.OrderBy(n => GetModifierOrder(n.Key.ToString())))
		{
			parts.Add(FormatModifierForDisplay(note.Key.ToString()));
		}

		// Add regular notes
		foreach (Note note in regularNotes)
		{
			parts.Add(note.ToString());
		}

		return string.Join("+", parts);
	}

	/// <summary>
	/// Checks if a note represents a modifier key
	/// </summary>
	/// <param name="note">The note to check</param>
	/// <returns>True if the note is a modifier key</returns>
	private static bool IsModifierNote(Note note)
	{
		string key = note.Key.ToString().ToUpperInvariant();
		return key is "CTRL" or "CONTROL" or "ALT" or "SHIFT" or "META" or "WIN" or "WINDOWS" or "CMD" or "COMMAND";
	}

	/// <summary>
	/// Gets the display order for modifier keys
	/// </summary>
	/// <param name="modifier">The modifier key name</param>
	/// <returns>Order value for consistent display</returns>
	private static int GetModifierOrder(string modifier)
	{
		return modifier.ToUpperInvariant() switch
		{
			"CTRL" or "CONTROL" => 1,
			"ALT" => 2,
			"SHIFT" => 3,
			"META" or "WIN" or "WINDOWS" or "CMD" or "COMMAND" => 4,
			_ => 5
		};
	}

	/// <summary>
	/// Formats a modifier key for display
	/// </summary>
	/// <param name="modifier">The modifier key name</param>
	/// <returns>Formatted display name</returns>
	private static string FormatModifierForDisplay(string modifier)
	{
		return modifier.ToUpperInvariant() switch
		{
			"CTRL" or "CONTROL" => "Ctrl",
			"ALT" => "Alt",
			"SHIFT" => "Shift",
			"META" or "WIN" or "WINDOWS" or "CMD" or "COMMAND" => "Meta",
			_ => modifier
		};
	}

	/// <inheritdoc/>
	public bool Equals(Chord? other) =>
		other is not null &&
		(ReferenceEquals(this, other) ||
		 (_notes.Count == other._notes.Count &&
		  _notes.All(other._notes.Contains)));

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Chord other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		HashCode hashCode = new();
		foreach (Note note in _notes.OrderBy(n => n.Key.ToString()))
		{
			hashCode.Add(note);
		}
		return hashCode.ToHashCode();
	}

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(Chord? left, Chord? right) => (left?.Equals(right)) ?? (right is null);

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(Chord? left, Chord? right) => !(left == right);

	/// <summary>
	/// Implicitly converts a Note to a Chord (with no modifiers)
	/// </summary>
	/// <param name="note">The note</param>
	/// <returns>A new Chord instance with no modifiers</returns>
	public static implicit operator Chord(Note note) => new(note);

	/// <summary>
	/// Creates a Chord from a Note (explicit alternative to implicit operator)
	/// </summary>
	/// <param name="note">The note</param>
	/// <returns>A new Chord instance with no modifiers</returns>
	public static Chord FromNote(Note note) => new(note);

	/// <summary>
	/// Parses a string representation into a Chord
	/// </summary>
	/// <param name="value">String in format "Ctrl+Alt+Key" or "Note1+Note2+Note3"</param>
	/// <returns>A new Chord instance</returns>
	/// <exception cref="ArgumentException">Thrown when the format is invalid</exception>
	public static Chord Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value cannot be null or whitespace", nameof(value));
		}

		string[] parts = [.. value.Split('+', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())];

		if (parts.Length == 0)
		{
			throw new ArgumentException("Invalid chord format", nameof(value));
		}

		// Convert all parts to Notes - modifiers and regular keys are now treated the same
		List<Note> notes = [];

		foreach (string part in parts)
		{
			// Normalize modifier key names for consistency
			string normalizedPart = part.ToUpperInvariant() switch
			{
				"CONTROL" => "CTRL",
				"WIN" or "WINDOWS" or "CMD" or "COMMAND" => "META",
				_ => part.ToUpperInvariant()
			};

			notes.Add(new Note(normalizedPart));
		}

		return new Chord(notes);
	}
}

/// <summary>
/// Represents a musical phrase - a sequence of notes and/or chords
/// </summary>
public sealed class Phrase : IEquatable<Phrase>
{
	private readonly List<Chord> _sequence;

	/// <summary>
	/// Initializes a new instance of the <see cref="Phrase"/> class
	/// </summary>
	/// <param name="sequence">The sequence of chords that make up this phrase</param>
	/// <exception cref="ArgumentException">Thrown when sequence is null or empty</exception>
	public Phrase(IEnumerable<Chord> sequence)
	{
		Ensure.NotNull(sequence);
		_sequence = [.. sequence];

		if (_sequence.Count == 0)
		{
			throw new ArgumentException("Phrase must contain at least one chord", nameof(sequence));
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Phrase"/> class with a single chord
	/// </summary>
	/// <param name="chord">The single chord that makes up this phrase</param>
	public Phrase(Chord chord) : this([chord])
	{
	}

	/// <summary>
	/// Gets the sequence of chords in this phrase
	/// </summary>
	public IReadOnlyList<Chord> Sequence => _sequence.AsReadOnly();

	/// <summary>
	/// Gets a value indicating whether this phrase contains only a single chord
	/// </summary>
	public bool IsSingleChord => _sequence.Count == 1;

	/// <summary>
	/// Gets the length of the phrase (number of chords)
	/// </summary>
	public int Length => _sequence.Count;

	/// <summary>
	/// Returns a string representation of the phrase
	/// </summary>
	/// <returns>String representation with chords separated by ", "</returns>
	public override string ToString() => string.Join(", ", _sequence.Select(c => c.ToString()));

	/// <inheritdoc/>
	public bool Equals(Phrase? other) =>
		other is not null && _sequence.SequenceEqual(other._sequence);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Phrase other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		HashCode hash = new();
		foreach (Chord chord in _sequence)
		{
			hash.Add(chord);
		}
		return hash.ToHashCode();
	}

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(Phrase? left, Phrase? right) => (left?.Equals(right)) ?? (right is null);

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(Phrase? left, Phrase? right) => !(left == right);

	/// <summary>
	/// Implicitly converts a Chord to a Phrase (single chord phrase)
	/// </summary>
	/// <param name="chord">The chord</param>
	/// <returns>A new Phrase instance with a single chord</returns>
	public static implicit operator Phrase(Chord chord) => new(chord);

	/// <summary>
	/// Creates a Phrase from a Chord (explicit alternative to implicit operator)
	/// </summary>
	/// <param name="chord">The chord</param>
	/// <returns>A new Phrase instance with a single chord</returns>
	public static Phrase FromChord(Chord chord) => new(chord);

	/// <summary>
	/// Parses a string representation into a Phrase
	/// </summary>
	/// <param name="value">String in format "Ctrl+R, R" or "Ctrl+C"</param>
	/// <returns>A new Phrase instance</returns>
	/// <exception cref="ArgumentException">Thrown when the format is invalid</exception>
	public static Phrase Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value cannot be null or whitespace", nameof(value));
		}

		string[] chordStrings = [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(s => s.Trim())];

		return chordStrings.Length == 0
			? throw new ArgumentException("Invalid phrase format", nameof(value))
			: new Phrase(chordStrings.Select(Chord.Parse));
	}

	/// <summary>
	/// Creates a phrase from a sequence of chord strings
	/// </summary>
	/// <param name="chordStrings">Array of chord strings</param>
	/// <returns>A new Phrase instance</returns>
	public static Phrase FromStrings(params string[] chordStrings) =>
		new(chordStrings.Select(Chord.Parse));
}
