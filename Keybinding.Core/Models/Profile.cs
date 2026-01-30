// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using System.Collections.Generic;

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
		Chords = [];
	}

	/// <summary>
	/// Gets the unique profile identifier
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Gets the profile name
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the profile description
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets the chord bindings for this profile (command ID to chord mapping)
	/// </summary>
	public Dictionary<string, Chord> Chords { get; }

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

		Chords[commandId.Trim()] = chord;
	}

	/// <summary>
	/// Gets the chord binding for a command in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>The chord if found, null otherwise</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public Chord? GetChord(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: Chords.TryGetValue(commandId.Trim(), out Chord? chord)
			? chord
			: null;
	}

	/// <summary>
	/// Gets all chord bindings for this profile
	/// </summary>
	/// <returns>Dictionary of command ID to chord mappings</returns>
	public IReadOnlyDictionary<string, Chord> GetAllChords() => Chords.AsReadOnly();

	/// <summary>
	/// Checks if a command has a chord binding in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the command has a chord binding, false otherwise</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public bool HasChord(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: Chords.ContainsKey(commandId.Trim());
	}

	/// <summary>
	/// Removes a chord binding for a command from this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the chord binding was removed, false if it didn't exist</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public bool RemoveChord(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: Chords.Remove(commandId.Trim());
	}

	/// <summary>
	/// Gets all command IDs that have chord bindings in this profile
	/// </summary>
	/// <returns>Collection of command IDs</returns>
	public IReadOnlyCollection<string> BoundCommands => Chords.Keys;

	/// <summary>
	/// Clears all chord bindings from this profile
	/// </summary>
	public void ClearChords() => Chords.Clear();

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
