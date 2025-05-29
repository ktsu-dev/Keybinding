// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Services;

/// <summary>
/// Implementation of profile manager for managing keybinding profiles
/// </summary>
public sealed class ProfileManager : IProfileManager
{
	private readonly ConcurrentDictionary<string, Profile> _profiles = new();
	private readonly object _lock = new();
	private volatile string? _activeProfileId;

	/// <inheritdoc/>
	public bool CreateProfile(Profile profile)
	{
		ArgumentNullException.ThrowIfNull(profile);

		return _profiles.TryAdd(profile.Id, profile);
	}

	/// <inheritdoc/>
	public bool DeleteProfile(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		var normalizedId = profileId.Trim();
		var removed = _profiles.TryRemove(normalizedId, out _);

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
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		return _profiles.TryGetValue(profileId.Trim(), out var profile) ? profile : null;
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
		var activeId = _activeProfileId;
		return activeId is not null ? GetProfile(activeId) : null;
	}

	/// <inheritdoc/>
	public bool SetActiveProfile(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		var normalizedId = profileId.Trim();

		if (!_profiles.ContainsKey(normalizedId))
			return false;

		_activeProfileId = normalizedId;
		return true;
	}

	/// <inheritdoc/>
	public void ClearActiveProfile()
	{
		_activeProfileId = null;
	}

	/// <inheritdoc/>
	public bool ProfileExists(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		return _profiles.ContainsKey(profileId.Trim());
	}

	/// <inheritdoc/>
	public Profile? DuplicateProfile(string sourceProfileId, string newProfileId, string newProfileName, string? newDescription = null)
	{
		if (string.IsNullOrWhiteSpace(sourceProfileId))
			throw new ArgumentException("Source profile ID cannot be null or whitespace", nameof(sourceProfileId));
		if (string.IsNullOrWhiteSpace(newProfileId))
			throw new ArgumentException("New profile ID cannot be null or whitespace", nameof(newProfileId));
		if (string.IsNullOrWhiteSpace(newProfileName))
			throw new ArgumentException("New profile name cannot be null or whitespace", nameof(newProfileName));

		var sourceProfile = GetProfile(sourceProfileId);
		if (sourceProfile is null)
			return null;

		var normalizedNewId = newProfileId.Trim();
		if (_profiles.ContainsKey(normalizedNewId))
			return null;

		var newProfile = new Profile(normalizedNewId, newProfileName.Trim(), newDescription);

		// Copy all keybindings from source profile
		foreach (var kvp in sourceProfile.Keybindings)
		{
			newProfile.SetKeybinding(kvp.Key, kvp.Value);
		}

		return CreateProfile(newProfile) ? newProfile : null;
	}

	/// <inheritdoc/>
	public bool RenameProfile(string profileId, string newName, string? newDescription = null)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		if (string.IsNullOrWhiteSpace(newName))
			throw new ArgumentException("New name cannot be null or whitespace", nameof(newName));

		var profile = GetProfile(profileId);
		if (profile is null)
			return false;

		// Create a new profile with the same ID but new name/description
		var updatedProfile = new Profile(profile.Id, newName.Trim(), newDescription);

		// Copy all keybindings
		foreach (var kvp in profile.Keybindings)
		{
			updatedProfile.SetKeybinding(kvp.Key, kvp.Value);
		}

		// Replace the profile (this will preserve the same ID)
		_profiles.TryRemove(profile.Id, out _);
		return _profiles.TryAdd(profile.Id, updatedProfile);
	}
}
