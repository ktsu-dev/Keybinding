// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Services;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Implementation of the main keybinding service that coordinates commands, profiles, and keybindings
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="KeybindingService"/> class
/// </remarks>
/// <param name="commandRegistry">The command registry instance</param>
/// <param name="profileManager">The profile manager instance</param>
public sealed class KeybindingService(ICommandRegistry commandRegistry, IProfileManager profileManager) : IKeybindingService
{
	private readonly ICommandRegistry _commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
	private readonly IProfileManager _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));

	/// <inheritdoc/>
	public bool SetKeybinding(string profileId, string commandId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		ArgumentNullException.ThrowIfNull(keyCombination);

		Profile profile = _profileManager.GetProfile(profileId) ?? throw new InvalidOperationException($"Profile '{profileId}' does not exist");

		if (!_commandRegistry.IsCommandRegistered(commandId))
		{
			throw new InvalidOperationException($"Command '{commandId}' is not registered");
		}

		profile.SetKeybinding(commandId, keyCombination);
		return true;
	}

	/// <inheritdoc/>
	public bool SetKeybinding(string commandId, KeyCombination keyCombination)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is null
			? throw new InvalidOperationException("No active profile is set")
			: SetKeybinding(activeProfile.Id, commandId, keyCombination);
	}

	/// <inheritdoc/>
	public bool RemoveKeybinding(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.RemoveKeybinding(commandId) ?? false;
	}

	/// <inheritdoc/>
	public bool RemoveKeybinding(string commandId)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.RemoveKeybinding(commandId) ?? false;
	}

	/// <inheritdoc/>
	public KeyCombination? GetKeybinding(string profileId, string commandId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		if (string.IsNullOrWhiteSpace(commandId))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.GetKeybinding(commandId);
	}

	/// <inheritdoc/>
	public KeyCombination? GetKeybinding(string commandId)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.GetKeybinding(commandId);
	}

	/// <inheritdoc/>
	public string? FindCommandByKeybinding(string profileId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		ArgumentNullException.ThrowIfNull(keyCombination);

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.Keybindings
			.FirstOrDefault(kvp => kvp.Value.Equals(keyCombination))
			.Key;
	}

	/// <inheritdoc/>
	public string? FindCommandByKeybinding(KeyCombination keyCombination)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null ? FindCommandByKeybinding(activeProfile.Id, keyCombination) : null;
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, KeyCombination> GetAllKeybindings(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.Keybindings.AsReadOnly() ?? new Dictionary<string, KeyCombination>().AsReadOnly();
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, KeyCombination> GetAllKeybindings()
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile?.Keybindings.AsReadOnly() ?? new Dictionary<string, KeyCombination>().AsReadOnly();
	}

	/// <inheritdoc/>
	public bool IsKeyCombinationBound(string profileId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		ArgumentNullException.ThrowIfNull(keyCombination);

		Profile? profile = _profileManager.GetProfile(profileId);
		return profile?.Keybindings.Values.Any(kc => kc.Equals(keyCombination)) ?? false;
	}

	/// <inheritdoc/>
	public bool IsKeyCombinationBound(KeyCombination keyCombination)
	{
		Profile? activeProfile = _profileManager.GetActiveProfile();
		return activeProfile is not null && IsKeyCombinationBound(activeProfile.Id, keyCombination);
	}

	/// <inheritdoc/>
	public bool ValidateKeybinding(string profileId, string commandId, KeyCombination keyCombination)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(commandId))
		{
			return false;
		}

		if (keyCombination is null)
		{
			return false;
		}

		// Check if profile exists
		if (!_profileManager.ProfileExists(profileId))
		{
			return false;
		}

		// Check if command is registered
		if (!_commandRegistry.IsCommandRegistered(commandId))
		{
			return false;
		}

		// Additional validation can be added here (e.g., checking for forbidden key combinations)

		return true;
	}

	/// <inheritdoc/>
	public bool CopyKeybindings(string sourceProfileId, string targetProfileId, bool overwriteExisting = false)
	{
		if (string.IsNullOrWhiteSpace(sourceProfileId))
		{
			throw new ArgumentException("Source profile ID cannot be null or whitespace", nameof(sourceProfileId));
		}

		if (string.IsNullOrWhiteSpace(targetProfileId))
		{
			throw new ArgumentException("Target profile ID cannot be null or whitespace", nameof(targetProfileId));
		}

		Profile? sourceProfile = _profileManager.GetProfile(sourceProfileId);
		Profile? targetProfile = _profileManager.GetProfile(targetProfileId);

		if (sourceProfile is null || targetProfile is null)
		{
			return false;
		}

		foreach (KeyValuePair<string, KeyCombination> kvp in sourceProfile.Keybindings)
		{
			string commandId = kvp.Key;
			KeyCombination keyCombination = kvp.Value;

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
