// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Interface for keybinding management using the musical paradigm
/// </summary>
public interface IKeybindingService
{
	/// <summary>
	/// Gets the currently active keybinding profile
	/// </summary>
	public Profile? ActiveProfile { get; }

	/// <summary>
	/// Gets all available keybinding profiles
	/// </summary>
	/// <returns>Collection of available profiles</returns>
	public IReadOnlyCollection<Profile> GetAllProfiles();

	/// <summary>
	/// Gets a specific profile by ID
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <returns>The profile if found, null otherwise</returns>
	public Profile? GetProfile(string profileId);

	/// <summary>
	/// Creates a new keybinding profile
	/// </summary>
	/// <param name="id">The unique profile ID</param>
	/// <param name="name">The profile name</param>
	/// <param name="description">Optional profile description</param>
	/// <returns>The created profile</returns>
	public Profile CreateProfile(string id, string name, string? description = null);

	/// <summary>
	/// Deletes a keybinding profile
	/// </summary>
	/// <param name="profileId">The profile ID to delete</param>
	/// <returns>True if the profile was deleted, false if it didn't exist</returns>
	public bool DeleteProfile(string profileId);

	/// <summary>
	/// Sets the active keybinding profile
	/// </summary>
	/// <param name="profileId">The profile ID to activate</param>
	/// <returns>True if the profile was activated, false if the profile doesn't exist</returns>
	public bool SetActiveProfile(string profileId);

	/// <summary>
	/// Binds a chord to a command in the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <param name="chord">The chord to bind</param>
	/// <returns>True if the binding was successful, false if no active profile</returns>
	public bool BindChord(string commandId, Chord chord);

	/// <summary>
	/// Binds a chord to a command in a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <param name="chord">The chord to bind</param>
	/// <returns>True if the binding was successful, false if the profile doesn't exist</returns>
	public bool BindChord(string profileId, string commandId, Chord chord);

	/// <summary>
	/// Gets the chord binding for a command in the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>The chord if found, null otherwise</returns>
	public Chord? GetChord(string commandId);

	/// <summary>
	/// Gets the chord binding for a command in a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <returns>The chord if found, null otherwise</returns>
	public Chord? GetChord(string profileId, string commandId);

	/// <summary>
	/// Gets all chord bindings for the active profile
	/// </summary>
	/// <returns>Dictionary of command ID to chord mappings</returns>
	public IReadOnlyDictionary<string, Chord> GetAllChords();

	/// <summary>
	/// Gets all chord bindings for a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <returns>Dictionary of command ID to chord mappings</returns>
	public IReadOnlyDictionary<string, Chord> GetAllChords(string profileId);

	/// <summary>
	/// Removes a chord binding for a command from the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the binding was removed, false if it didn't exist or no active profile</returns>
	public bool UnbindChord(string commandId);

	/// <summary>
	/// Removes a chord binding for a command from a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the binding was removed, false if it didn't exist or profile doesn't exist</returns>
	public bool UnbindChord(string profileId, string commandId);

	/// <summary>
	/// Clears all chord bindings from the active profile
	/// </summary>
	/// <returns>True if bindings were cleared, false if no active profile</returns>
	public bool ClearAllChords();

	/// <summary>
	/// Clears all chord bindings from a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <returns>True if bindings were cleared, false if the profile doesn't exist</returns>
	public bool ClearAllChords(string profileId);

	/// <summary>
	/// Parses a phrase string into a sequence of chords
	/// </summary>
	/// <param name="phraseString">The phrase string to parse</param>
	/// <returns>Phrase containing the sequence of chords</returns>
	public Phrase ParsePhrase(string phraseString);

	/// <summary>
	/// Parses a chord string into a chord
	/// </summary>
	/// <param name="chordString">The chord string to parse</param>
	/// <returns>Chord containing the notes</returns>
	public Chord ParseChord(string chordString);

	/// <summary>
	/// Executes a command if it has a chord binding in the active profile
	/// </summary>
	/// <param name="chord">The chord that was pressed</param>
	/// <returns>The command ID that was executed, null if no binding found</returns>
	public string? ExecuteChord(Chord chord);

	/// <summary>
	/// Executes a command if it has a chord binding in a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="chord">The chord that was pressed</param>
	/// <returns>The command ID that was executed, null if no binding found</returns>
	public string? ExecuteChord(string profileId, Chord chord);

	/// <summary>
	/// Checks if a command has a chord binding in the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the command has a chord binding, false otherwise</returns>
	public bool HasChordBinding(string commandId);

	/// <summary>
	/// Checks if a command has a chord binding in a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the command has a chord binding, false otherwise</returns>
	public bool HasChordBinding(string profileId, string commandId);

	/// <summary>
	/// Finds the command ID bound to a specific chord in the active profile
	/// </summary>
	/// <param name="chord">The chord to search for</param>
	/// <returns>The command ID if found, null otherwise</returns>
	public string? FindCommandByChord(Chord chord);

	/// <summary>
	/// Finds the command ID bound to a specific chord in a specific profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="chord">The chord to search for</param>
	/// <returns>The command ID if found, null otherwise</returns>
	public string? FindCommandByChord(string profileId, Chord chord);
}
