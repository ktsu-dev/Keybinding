// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Contracts;

using ktsu.Keybinding.Core.Models;

/// <summary>
/// Contract for persisting keybinding data
/// </summary>
public interface IKeybindingRepository
{
	/// <summary>
	/// Saves a profile to persistent storage
	/// </summary>
	/// <param name="profile">The profile to save</param>
	/// <returns>Task representing the async operation</returns>
	public Task SaveProfileAsync(Profile profile);

	/// <summary>
	/// Loads a profile from persistent storage
	/// </summary>
	/// <param name="profileId">The profile ID to load</param>
	/// <returns>The loaded profile, or null if not found</returns>
	public Task<Profile?> LoadProfileAsync(string profileId);

	/// <summary>
	/// Loads all profiles from persistent storage
	/// </summary>
	/// <returns>Collection of all loaded profiles</returns>
	public Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync();

	/// <summary>
	/// Deletes a profile from persistent storage
	/// </summary>
	/// <param name="profileId">The profile ID to delete</param>
	/// <returns>Task representing the async operation</returns>
	public Task DeleteProfileAsync(string profileId);

	/// <summary>
	/// Saves commands to persistent storage
	/// </summary>
	/// <param name="commands">The commands to save</param>
	/// <returns>Task representing the async operation</returns>
	public Task SaveCommandsAsync(IEnumerable<Command> commands);

	/// <summary>
	/// Loads all commands from persistent storage
	/// </summary>
	/// <returns>Collection of all loaded commands</returns>
	public Task<IReadOnlyCollection<Command>> LoadCommandsAsync();

	/// <summary>
	/// Saves the active profile ID to persistent storage
	/// </summary>
	/// <param name="profileId">The active profile ID, or null if no profile is active</param>
	/// <returns>Task representing the async operation</returns>
	public Task SaveActiveProfileAsync(string? profileId);

	/// <summary>
	/// Loads the active profile ID from persistent storage
	/// </summary>
	/// <returns>The active profile ID, or null if no profile is active</returns>
	public Task<string?> LoadActiveProfileAsync();

	/// <summary>
	/// Checks if the repository has been initialized
	/// </summary>
	/// <returns>True if initialized, false otherwise</returns>
	public Task<bool> IsInitializedAsync();

	/// <summary>
	/// Initializes the repository (creates necessary files/directories)
	/// </summary>
	/// <returns>Task representing the async operation</returns>
	public Task InitializeAsync();
}
