// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Services;

using System.Collections.Concurrent;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Implementation of profile manager for managing keybinding profiles
/// </summary>
public sealed class ProfileManager : IProfileManager
{
	private readonly ConcurrentDictionary<string, Profile> _profiles = new();
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private volatile string? _activeProfileId;

	/// <inheritdoc/>
	public bool CreateProfile(Profile profile)
	{
		Ensure.NotNull(profile);

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

		// Return the existing profile, or store a new one, in one atomic step: a separate lookup
		// and add let two concurrent callers each return their own Profile while only one of
		// them was stored (ktsu-dev/Keybinding#112)
		return _profiles.GetOrAdd(normalizedId, key => new Profile(key, name.Trim(), description));
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

		lock (_lock)
		{
			Profile? profile = GetProfile(profileId);
			if (profile is null)
			{
				return false;
			}

			// Rename the stored instance rather than swapping in a copy. A copy would detach every
			// reference a caller already holds (from CreateProfile, GetActiveProfile and the like), so
			// chords set through it would never reach GetProfile or SaveAsync. It would also leave a
			// window between removing and re-adding in which the profile did not exist at all.
			// A description that is not passed is kept, as the optional parameter implies.
			profile.Rename(newName, newDescription ?? profile.Description);
			return true;
		}
	}
}
