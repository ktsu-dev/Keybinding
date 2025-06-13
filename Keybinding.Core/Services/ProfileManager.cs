// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Services;
using System.Collections.Concurrent;
using System.Threading;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Implementation of profile manager for managing keybinding profiles
/// </summary>
public sealed class ProfileManager : IProfileManager
{
	private readonly ConcurrentDictionary<string, Profile> _profiles = new();
	private readonly Lock _lock = new();
	private volatile string? _activeProfileId;

	/// <inheritdoc/>
	public bool CreateProfile(Profile profile)
	{
		ArgumentNullException.ThrowIfNull(profile);

		return _profiles.TryAdd(profile.Id, profile);
	}

	/// <inheritdoc/>
	public Profile CreateProfile(string id, string name, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(id));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Profile name cannot be null or whitespace", nameof(name));
		}

		string normalizedId = id.Trim();

		// Return existing profile if it already exists
		if (_profiles.TryGetValue(normalizedId, out Profile? existingProfile))
		{
			return existingProfile;
		}

		// Create new profile
		Profile newProfile = new(normalizedId, name.Trim(), description);
		_profiles.TryAdd(normalizedId, newProfile);
		return newProfile;
	}

	/// <inheritdoc/>
	public bool DeleteProfile(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		string normalizedId = profileId.Trim();
		bool removed = _profiles.TryRemove(normalizedId, out _);

		// Clear active profile if it was the one being deleted
		if (removed && _activeProfileId == normalizedId)
		{
			_activeProfileId = null;
		}

		return removed;
	}

	/// <inheritdoc/>
	public Profile? GetProfile(string profileId)
	{
		return string.IsNullOrWhiteSpace(profileId)
			? throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId))
			: _profiles.TryGetValue(profileId.Trim(), out Profile? profile) ? profile : null;
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<Profile> GetAllProfiles()
	{
		lock (_lock)
		{
			return _profiles.Values.ToList().AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public Profile? GetActiveProfile()
	{
		string? activeId = _activeProfileId;
		return activeId is not null ? GetProfile(activeId) : null;
	}

	/// <inheritdoc/>
	public bool SetActiveProfile(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		string normalizedId = profileId.Trim();

		if (!_profiles.ContainsKey(normalizedId))
		{
			return false;
		}

		_activeProfileId = normalizedId;
		return true;
	}

	/// <inheritdoc/>
	public void ClearActiveProfile() => _activeProfileId = null;

	/// <inheritdoc/>
	public bool ProfileExists(string profileId)
	{
		return string.IsNullOrWhiteSpace(profileId)
			? throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId))
			: _profiles.ContainsKey(profileId.Trim());
	}

	/// <inheritdoc/>
	public Profile? DuplicateProfile(string sourceProfileId, string newProfileId, string newProfileName, string? newDescription = null)
	{
		if (string.IsNullOrWhiteSpace(sourceProfileId))
		{
			throw new ArgumentException("Source profile ID cannot be null or whitespace", nameof(sourceProfileId));
		}

		if (string.IsNullOrWhiteSpace(newProfileId))
		{
			throw new ArgumentException("New profile ID cannot be null or whitespace", nameof(newProfileId));
		}

		if (string.IsNullOrWhiteSpace(newProfileName))
		{
			throw new ArgumentException("New profile name cannot be null or whitespace", nameof(newProfileName));
		}

		Profile? sourceProfile = GetProfile(sourceProfileId);
		if (sourceProfile is null)
		{
			return null;
		}

		string normalizedNewId = newProfileId.Trim();
		if (_profiles.ContainsKey(normalizedNewId))
		{
			return null;
		}

		Profile newProfile = new(normalizedNewId, newProfileName.Trim(), newDescription);

		// Copy all chords from source profile
		foreach (KeyValuePair<string, Chord> kvp in sourceProfile.Chords)
		{
			newProfile.SetChord(kvp.Key, kvp.Value);
		}

		return CreateProfile(newProfile) ? newProfile : null;
	}

	/// <inheritdoc/>
	public bool RenameProfile(string profileId, string newName, string? newDescription = null)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		if (string.IsNullOrWhiteSpace(newName))
		{
			throw new ArgumentException("New name cannot be null or whitespace", nameof(newName));
		}

		Profile? profile = GetProfile(profileId);
		if (profile is null)
		{
			return false;
		}

		// Create a new profile with the same ID but new name/description
		Profile updatedProfile = new(profile.Id, newName.Trim(), newDescription);

		// Copy all chords
		foreach (KeyValuePair<string, Chord> kvp in profile.Chords)
		{
			updatedProfile.SetChord(kvp.Key, kvp.Value);
		}

		// Replace the profile (this will preserve the same ID)
		_profiles.TryRemove(profile.Id, out _);
		return _profiles.TryAdd(profile.Id, updatedProfile);
	}
}
