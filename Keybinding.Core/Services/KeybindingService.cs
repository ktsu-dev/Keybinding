// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Services;

/// <summary>
/// Implementation of the main keybinding service that coordinates commands, profiles, and keybindings
/// </summary>
public sealed class KeybindingService : IKeybindingService
{
	private readonly ICommandRegistry _commandRegistry;
	private readonly IProfileManager _profileManager;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingService"/> class
	/// </summary>
	/// <param name="commandRegistry">The command registry instance</param>
	/// <param name="profileManager">The profile manager instance</param>
	public KeybindingService(ICommandRegistry commandRegistry, IProfileManager profileManager)
	{
		_commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
		_profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
	}

	/// <inheritdoc/>
	public bool SetKeybinding(string profileId, string commandId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		if (string.IsNullOrWhiteSpace(commandId))
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		ArgumentNullException.ThrowIfNull(keyCombination);

		var profile = _profileManager.GetProfile(profileId);
		if (profile is null)
			throw new InvalidOperationException($"Profile '{profileId}' does not exist");

		if (!_commandRegistry.IsCommandRegistered(commandId))
			throw new InvalidOperationException($"Command '{commandId}' is not registered");

		profile.SetKeybinding(commandId, keyCombination);
		return true;
	}

	/// <inheritdoc/>
	public bool SetKeybinding(string commandId, KeyCombination keyCombination)
	{
		var activeProfile = _profileManager.GetActiveProfile();
		if (activeProfile is null)
			throw new InvalidOperationException("No active profile is set");

		return SetKeybinding(activeProfile.Id, commandId, keyCombination);
	}

	/// <inheritdoc/>
	public bool RemoveKeybinding(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		if (string.IsNullOrWhiteSpace(commandId))
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));

		var profile = _profileManager.GetProfile(profileId);
		return profile?.RemoveKeybinding(commandId) ?? false;
	}

	/// <inheritdoc/>
	public bool RemoveKeybinding(string commandId)
	{
		var activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.RemoveKeybinding(commandId) ?? false;
	}

	/// <inheritdoc/>
	public KeyCombination? GetKeybinding(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		if (string.IsNullOrWhiteSpace(commandId))
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));

		var profile = _profileManager.GetProfile(profileId);
		return profile?.GetKeybinding(commandId);
	}

	/// <inheritdoc/>
	public KeyCombination? GetKeybinding(string commandId)
	{
		var activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.GetKeybinding(commandId);
	}

	/// <inheritdoc/>
	public string? FindCommandByKeybinding(string profileId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		ArgumentNullException.ThrowIfNull(keyCombination);

		var profile = _profileManager.GetProfile(profileId);
		if (profile is null)
			return null;

		return profile.Keybindings
			.FirstOrDefault(kvp => kvp.Value.Equals(keyCombination))
			.Key;
	}

	/// <inheritdoc/>
	public string? FindCommandByKeybinding(KeyCombination keyCombination)
	{
		var activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null ? FindCommandByKeybinding(activeProfile.Id, keyCombination) : null;
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, KeyCombination> GetAllKeybindings(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		var profile = _profileManager.GetProfile(profileId);
		return profile?.Keybindings.AsReadOnly() ?? new Dictionary<string, KeyCombination>().AsReadOnly();
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, KeyCombination> GetAllKeybindings()
	{
		var activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.Keybindings.AsReadOnly() ?? new Dictionary<string, KeyCombination>().AsReadOnly();
	}

	/// <inheritdoc/>
	public bool IsKeyCombinationBound(string profileId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		ArgumentNullException.ThrowIfNull(keyCombination);

		var profile = _profileManager.GetProfile(profileId);
		return profile?.Keybindings.Values.Any(kc => kc.Equals(keyCombination)) ?? false;
	}

	/// <inheritdoc/>
	public bool IsKeyCombinationBound(KeyCombination keyCombination)
	{
		var activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null && IsKeyCombinationBound(activeProfile.Id, keyCombination);
	}

	/// <inheritdoc/>
	public bool ValidateKeybinding(string profileId, string commandId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			return false;
		if (string.IsNullOrWhiteSpace(commandId))
			return false;
		if (keyCombination is null)
			return false;

		// Check if profile exists
		if (!_profileManager.ProfileExists(profileId))
			return false;

		// Check if command is registered
		if (!_commandRegistry.IsCommandRegistered(commandId))
			return false;

		// Additional validation can be added here (e.g., checking for forbidden key combinations)

		return true;
	}

	/// <inheritdoc/>
	public bool CopyKeybindings(string sourceProfileId, string targetProfileId, bool overwriteExisting = false)
	{
		if (string.IsNullOrWhiteSpace(sourceProfileId))
			throw new ArgumentException("Source profile ID cannot be null or whitespace", nameof(sourceProfileId));
		if (string.IsNullOrWhiteSpace(targetProfileId))
			throw new ArgumentException("Target profile ID cannot be null or whitespace", nameof(targetProfileId));

		var sourceProfile = _profileManager.GetProfile(sourceProfileId);
		var targetProfile = _profileManager.GetProfile(targetProfileId);

		if (sourceProfile is null || targetProfile is null)
			return false;

		foreach (var kvp in sourceProfile.Keybindings)
		{
			var commandId = kvp.Key;
			var keyCombination = kvp.Value;

			// Only copy if command exists and either we're overwriting or the target doesn't have this command bound
			if (_commandRegistry.IsCommandRegistered(commandId) &&
			    (overwriteExisting || !targetProfile.HasKeybinding(commandId)))
			{
				targetProfile.SetKeybinding(commandId, keyCombination);
			}
		}

		return true;
	}
}
