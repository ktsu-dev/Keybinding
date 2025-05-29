// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

/// <summary>
/// Represents a keybinding profile containing command-to-keybinding mappings
/// </summary>
public sealed class Profile : IEquatable<Profile>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Profile"/> class
	/// </summary>
	/// <param name="id">The unique identifier for the profile</param>
	/// <param name="name">The display name of the profile</param>
	/// <param name="description">Optional description of the profile</param>
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
		Keybindings = [];
	}

	/// <summary>
	/// Gets the unique identifier for the profile
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Gets the display name of the profile
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the description of the profile
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets the command-to-keybinding mappings for this profile
	/// </summary>
	public Dictionary<string, KeyCombination> Keybindings { get; }

	/// <summary>
	/// Sets a keybinding for a command in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <param name="keyCombination">The key combination to bind</param>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	/// <exception cref="ArgumentNullException">Thrown when keyCombination is null</exception>
	public void SetKeybinding(string commandId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		ArgumentNullException.ThrowIfNull(keyCombination);

		Keybindings[commandId.Trim()] = keyCombination;
	}

	/// <summary>
	/// Removes a keybinding for a command from this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the keybinding was removed, false if it didn't exist</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public bool RemoveKeybinding(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: Keybindings.Remove(commandId.Trim());
	}

	/// <summary>
	/// Gets the keybinding for a command in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>The key combination if found, null otherwise</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public KeyCombination? GetKeybinding(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: Keybindings.TryGetValue(commandId.Trim(), out KeyCombination? keyCombination)
			? keyCombination
			: null;
	}

	/// <summary>
	/// Checks if a command has a keybinding in this profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the command has a keybinding, false otherwise</returns>
	/// <exception cref="ArgumentException">Thrown when commandId is null or whitespace</exception>
	public bool HasKeybinding(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: Keybindings.ContainsKey(commandId.Trim());
	}

	/// <summary>
	/// Gets all command IDs that have keybindings in this profile
	/// </summary>
	/// <returns>Collection of command IDs</returns>
	public IReadOnlyCollection<string> BoundCommands => Keybindings.Keys;

	/// <summary>
	/// Clears all keybindings from this profile
	/// </summary>
	public void ClearKeybindings() => Keybindings.Clear();

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
		left?.Equals(right) ?? right is null;

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(Profile? left, Profile? right) =>
		!(left == right);
}
