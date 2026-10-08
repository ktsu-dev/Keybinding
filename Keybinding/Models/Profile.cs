// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a keybinding profile with command to chord mappings
/// </summary>
public sealed class Profile : IEquatable<Profile>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Profile"/> class
	/// </summary>
	/// <param name="id">The unique profile identifier</param>
	/// <param name="name">The profile name</param>
	/// <param name="description">Optional profile description</param>
	/// <exception cref="ArgumentException">Thrown when id or name is null or whitespace</exception>
	public Profile(string id, string name, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(id));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Profile name cannot be null or whitespace", nameof(name));
		}

		Id = id.Trim();
		Name = name.Trim();
		Description = description?.Trim();
	}

	// The Chords property is get-only, so the serializer can only restore the bindings through a
	// constructor parameter of the same name and type. Without it a round-tripped profile comes back empty.
	[JsonConstructor]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by System.Text.Json through [JsonConstructor].")]
	private Profile(string id, string name, string? description, Dictionary<string, Chord>? chords) : this(id, name, description)
	{
		if (chords is not null)
		{
			foreach (KeyValuePair<string, Chord> binding in chords)
			{
				SetChord(binding.Key, binding.Value);
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "The only property over this field is obsolete, and the field is what the lock guards.")]
	private readonly Dictionary<string, Chord> _chords = [];

	// Every read and write of _chords takes this lock, so bindings can change from any thread.
	private readonly Lock _chordsLock = new();

	/// <summary>
	/// Gets the unique profile identifier
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Gets the profile name
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the profile description
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Renames this profile in place, so every reference already held to it stays attached to the
	/// manager that stores it.
	/// </summary>
	/// <param name="name">The new profile name</param>
	/// <param name="description">The new profile description</param>
	/// <exception cref="ArgumentException">Thrown when name is null or whitespace</exception>
	internal void Rename(string name, string? description)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Profile name cannot be null or whitespace", nameof(name));
		}

		Name = name.Trim();
		Description = description?.Trim();
	}

	/// <summary>
	/// Gets the live chord bindings for this profile (command ID to chord mapping)
	/// </summary>
	/// <remarks>
	/// Reading or changing this dictionary bypasses the profile's synchronization, so it is not
	/// thread-safe. Use <see cref="GetAllChords"/>, <see cref="SetChord"/>, <see cref="RemoveChord"/>
	/// and <see cref="ClearChords"/> instead.
	/// </remarks>
	[Obsolete("Chords is not thread-safe. Use GetAllChords, SetChord, RemoveChord or ClearChords instead.")]
	public Dictionary<string, Chord> Chords => _chords;

	/// <summary>
	/// Gets the number of chord bindings in this profile
	/// </summary>
	public int ChordCount
	{
		get
		{
			lock (_chordsLock)
			{
				return _chords.Count;
			}
		}
	}

	/// <summary>
	/// Sets a chord binding for a command in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <param name="chord">The chord to bind</param>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	/// <exception cref="ArgumentNullException">Thrown when chord is null</exception>
	public void SetChord(string commandId, Chord chord)
	{
		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		Ensure.NotNull(chord);

		lock (_chordsLock)
		{
			_chords[commandId.Trim()] = chord;
		}
	}

	/// <summary>
	/// Gets the chord binding for a command in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>The chord if found, null otherwise</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public Chord? GetChord(string commandId)
	{
		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		lock (_chordsLock)
		{
			return _chords.TryGetValue(commandId.Trim(), out Chord? chord) ? chord : null;
		}
	}

	/// <summary>
	/// Gets all chord bindings for this profile
	/// </summary>
	/// <returns>Dictionary of command ID to chord mappings</returns>
	public IReadOnlyDictionary<string, Chord> GetAllChords()
	{
		lock (_chordsLock)
		{
			return new Dictionary<string, Chord>(_chords).AsReadOnly();
		}
	}

	/// <summary>
	/// Finds the first command bound to a chord in this profile that satisfies a condition
	/// </summary>
	/// <param name="chord">The chord to look up</param>
	/// <param name="predicate">An optional condition the command ID must satisfy</param>
	/// <returns>The command ID if found, null otherwise</returns>
	internal string? FindCommand(Chord chord, Func<string, bool>? predicate = null)
	{
		foreach (KeyValuePair<string, Chord> binding in GetAllChords())
		{
			if (binding.Value.Equals(chord) && (predicate is null || predicate(binding.Key)))
			{
				return binding.Key;
			}
		}

		return null;
	}

	/// <summary>
	/// Checks if a command has a chord binding in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the command has a chord binding, false otherwise</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public bool HasChord(string commandId)
	{
		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		lock (_chordsLock)
		{
			return _chords.ContainsKey(commandId.Trim());
		}
	}

	/// <summary>
	/// Removes a chord binding for a command from this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the chord binding was removed, false if it didn't exist</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public bool RemoveChord(string commandId)
	{
		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		lock (_chordsLock)
		{
			return _chords.Remove(commandId.Trim());
		}
	}

	/// <summary>
	/// Gets all command IDs that have chord bindings in this profile
	/// </summary>
	/// <returns>Collection of command IDs</returns>
	public IReadOnlyCollection<string> BoundCommands
	{
		get
		{
			lock (_chordsLock)
			{
				return [.. _chords.Keys];
			}
		}
	}

	/// <summary>
	/// Clears all chord bindings from this profile
	/// </summary>
	public void ClearChords()
	{
		lock (_chordsLock)
		{
			_chords.Clear();
		}
	}

	/// <summary>
	/// Returns a string representation of the profile
	/// </summary>
	/// <returns>String representation in format "Id: Name"</returns>
	public override string ToString() => $"{Id}: {Name}";

	/// <inheritdoc/>
	public bool Equals(Profile? other) => other is not null && (ReferenceEquals(this, other) || Id == other.Id);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Profile other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => Id.GetHashCode();

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(Profile? left, Profile? right) =>
		(left?.Equals(right)) ?? (right is null);

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(Profile? left, Profile? right) =>
		!(left == right);
}
