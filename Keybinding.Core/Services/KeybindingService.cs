// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Services;

using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Implementation of the keybinding service using the musical paradigm
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="KeybindingService"/> class
/// </remarks>
/// <param name="commandRegistry">The command registry instance</param>
/// <param name="profileManager">The profile manager instance</param>
public sealed class KeybindingService(ICommandRegistry commandRegistry, IProfileManager profileManager) : IKeybindingService
{
	private readonly ICommandRegistry _commandRegistry = Ensure.NotNull(commandRegistry);
	private readonly IProfileManager _profileManager = Ensure.NotNull(profileManager);

	/// <inheritdoc/>
	public Profile? ActiveProfile => _profileManager.GetActiveProfile();

	/// <inheritdoc/>
	public IReadOnlyCollection<Profile> GetAllProfiles() => _profileManager.GetAllProfiles();

	/// <inheritdoc/>
	public Profile? GetProfile(string profileId) =>
		!string.IsNullOrWhiteSpace(profileId) ? _profileManager.GetProfile(profileId) : null;

	/// <inheritdoc/>
	public Profile CreateProfile(string id, string name, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(id));
		}

		ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

		return _profileManager.CreateProfile(id, name, description);
	}

	/// <inheritdoc/>
	public bool DeleteProfile(string profileId) =>
		!string.IsNullOrWhiteSpace(profileId) && _profileManager.DeleteProfile(profileId);

	/// <inheritdoc/>
	public bool SetActiveProfile(string profileId) =>
		!string.IsNullOrWhiteSpace(profileId) && _profileManager.SetActiveProfile(profileId);

	/// <inheritdoc/>
	public bool BindChord(string commandId, Chord chord)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null && BindChord(activeProfile.Id, commandId, chord);
	}

	/// <inheritdoc/>
	public bool BindChord(string profileId, string commandId, Chord chord)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(commandId))
		{
			return false;
		}

		Ensure.NotNull(chord);

		Profile? profile = _profileManager.GetProfile(profileId);
		if (profile is null)
		{
			return false;
		}

		if (!_commandRegistry.IsCommandRegistered(commandId))
		{
			return false;
		}

		profile.SetChord(commandId, chord);
		return true;
	}

	/// <inheritdoc/>
	public Chord? GetChord(string commandId)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.GetChord(commandId);
	}

	/// <inheritdoc/>
	public Chord? GetChord(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(commandId))
		{
			return null;
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.GetChord(commandId);
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, Chord> GetAllChords()
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.GetAllChords() ?? new Dictionary<string, Chord>().AsReadOnly();
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, Chord> GetAllChords(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			return new Dictionary<string, Chord>().AsReadOnly();
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.GetAllChords() ?? new Dictionary<string, Chord>().AsReadOnly();
	}

	/// <inheritdoc/>
	public bool UnbindChord(string commandId)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.RemoveChord(commandId) ?? false;
	}

	/// <inheritdoc/>
	public bool UnbindChord(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(commandId))
		{
			return false;
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.RemoveChord(commandId) ?? false;
	}

	/// <inheritdoc/>
	public bool ClearAllChords()
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		if (activeProfile is null)
		{
			return false;
		}

		activeProfile.ClearChords();
		return true;
	}

	/// <inheritdoc/>
	public bool ClearAllChords(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			return false;
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		if (profile is null)
		{
			return false;
		}

		profile.ClearChords();
		return true;
	}

	/// <inheritdoc/>
	public Phrase ParsePhrase(string phraseString)
	{
		if (string.IsNullOrWhiteSpace(phraseString))
		{
			return new Phrase([]);
		}

		// Split by comma to get individual chord strings
		string[] chordStrings = phraseString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		List<Chord> chords = [];

		foreach (string chordString in chordStrings)
		{
			Chord chord = ParseChord(chordString);
			chords.Add(chord);
		}

		return new Phrase(chords);
	}

	/// <inheritdoc/>
	public Chord ParseChord(string chordString)
	{
		if (string.IsNullOrWhiteSpace(chordString))
		{
			throw new ArgumentException("Chord string cannot be null or whitespace", nameof(chordString));
		}

		// Split by + to get individual note strings
		string[] noteStrings = chordString.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		List<Note> notes = [];

		foreach (string noteString in noteStrings)
		{
			Note note = new(noteString.Trim());
			notes.Add(note);
		}

		return notes.Count == 0 ? throw new ArgumentException("Chord must contain at least one note", nameof(chordString)) : new Chord(notes);
	}

	/// <inheritdoc/>
	public string? ExecuteChord(Chord chord)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null ? ExecuteChord(activeProfile.Id, chord) : null;
	}

	/// <inheritdoc/>
	public string? ExecuteChord(string profileId, Chord chord)
	{
		Ensure.NotNull(chord);

		string? commandId = FindCommandByChord(profileId, chord);
		if (commandId is not null && _commandRegistry.IsCommandRegistered(commandId))
		{
			// In a real implementation, this would trigger command execution
			// For now, we just return the command ID that would be executed
			return commandId;
		}

		return null;
	}

	/// <inheritdoc/>
	public bool HasChordBinding(string commandId)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.HasChord(commandId) ?? false;
	}

	/// <inheritdoc/>
	public bool HasChordBinding(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(commandId))
		{
			return false;
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.HasChord(commandId) ?? false;
	}

	/// <inheritdoc/>
	public string? FindCommandByChord(Chord chord)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null ? FindCommandByChord(activeProfile.Id, chord) : null;
	}

	/// <inheritdoc/>
	public string? FindCommandByChord(string profileId, Chord chord)
	{
		Ensure.NotNull(chord);

		if (string.IsNullOrWhiteSpace(profileId))
		{
			return null;
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.Chords
			.FirstOrDefault(kvp => kvp.Value.Equals(chord))
			.Key;
	}
}
