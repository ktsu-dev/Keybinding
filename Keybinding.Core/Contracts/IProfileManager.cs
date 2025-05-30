// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Contract for managing keybinding profiles
/// </summary>
public interface IProfileManager
{
	/// <summary>
	/// Creates a new profile
	/// </summary>
	/// <param name="profile">The profile to create</param>
	/// <returns>True if the profile was created, false if it already exists</returns>
	bool CreateProfile(Profile profile);

	/// <summary>
	/// Deletes a profile
	/// </summary>
	/// <param name="profileId">The ID of the profile to delete</param>
	/// <returns>True if the profile was deleted, false if it didn't exist</returns>
	bool DeleteProfile(string profileId);

	/// <summary>
	/// Gets a profile by its ID
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <returns>The profile if found, null otherwise</returns>
	Profile? GetProfile(string profileId);

	/// <summary>
	/// Gets all profiles
	/// </summary>
	/// <returns>Collection of all profiles</returns>
	IReadOnlyCollection<Profile> GetAllProfiles();

	/// <summary>
	/// Gets the currently active profile
	/// </summary>
	/// <returns>The active profile, or null if none is set</returns>
	Profile? GetActiveProfile();

	/// <summary>
	/// Sets the active profile
	/// </summary>
	/// <param name="profileId">The ID of the profile to make active</param>
	/// <returns>True if the profile was set as active, false if it doesn't exist</returns>
	bool SetActiveProfile(string profileId);

	/// <summary>
	/// Clears the active profile (no profile will be active)
	/// </summary>
	void ClearActiveProfile();

	/// <summary>
	/// Checks if a profile exists
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <returns>True if the profile exists, false otherwise</returns>
	bool ProfileExists(string profileId);

	/// <summary>
	/// Duplicates an existing profile with a new ID and name
	/// </summary>
	/// <param name="sourceProfileId">The ID of the profile to duplicate</param>
	/// <param name="newProfileId">The ID for the new profile</param>
	/// <param name="newProfileName">The name for the new profile</param>
	/// <param name="newDescription">Optional description for the new profile</param>
	/// <returns>The duplicated profile if successful, null if source doesn't exist or new ID already exists</returns>
	Profile? DuplicateProfile(string sourceProfileId, string newProfileId, string newProfileName, string? newDescription = null);

	/// <summary>
	/// Renames a profile
	/// </summary>
	/// <param name="profileId">The ID of the profile to rename</param>
	/// <param name="newName">The new name for the profile</param>
	/// <param name="newDescription">Optional new description for the profile</param>
	/// <returns>True if the profile was renamed, false if it doesn't exist</returns>
	bool RenameProfile(string profileId, string newName, string? newDescription = null);
}
