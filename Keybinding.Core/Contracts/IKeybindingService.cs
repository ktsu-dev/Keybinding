// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Contract for the main keybinding service that coordinates commands, profiles, and keybindings
/// </summary>
public interface IKeybindingService
{
	/// <summary>
	/// Sets a keybinding for a command in the specified profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <param name="keyCombination">The key combination to bind</param>
	/// <returns>True if the keybinding was set successfully</returns>
	/// <exception cref="InvalidOperationException">Thrown when profile or command doesn't exist</exception>
	bool SetKeybinding(string profileId, string commandId, KeyCombination keyCombination);

	/// <summary>
	/// Sets a keybinding for a command in the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <param name="keyCombination">The key combination to bind</param>
	/// <returns>True if the keybinding was set successfully</returns>
	/// <exception cref="InvalidOperationException">Thrown when no active profile is set or command doesn't exist</exception>
	bool SetKeybinding(string commandId, KeyCombination keyCombination);

	/// <summary>
	/// Removes a keybinding for a command from the specified profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the keybinding was removed successfully</returns>
	bool RemoveKeybinding(string profileId, string commandId);

	/// <summary>
	/// Removes a keybinding for a command from the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the keybinding was removed successfully</returns>
	bool RemoveKeybinding(string commandId);

	/// <summary>
	/// Gets the keybinding for a command in the specified profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <returns>The key combination if found, null otherwise</returns>
	KeyCombination? GetKeybinding(string profileId, string commandId);

	/// <summary>
	/// Gets the keybinding for a command in the active profile
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>The key combination if found, null otherwise</returns>
	KeyCombination? GetKeybinding(string commandId);

	/// <summary>
	/// Finds the command associated with a key combination in the specified profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="keyCombination">The key combination to search for</param>
	/// <returns>The command ID if found, null otherwise</returns>
	string? FindCommandByKeybinding(string profileId, KeyCombination keyCombination);

	/// <summary>
	/// Finds the command associated with a key combination in the active profile
	/// </summary>
	/// <param name="keyCombination">The key combination to search for</param>
	/// <returns>The command ID if found, null otherwise</returns>
	string? FindCommandByKeybinding(KeyCombination keyCombination);

	/// <summary>
	/// Gets all keybindings for the specified profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <returns>Dictionary of command ID to key combination mappings</returns>
	IReadOnlyDictionary<string, KeyCombination> GetAllKeybindings(string profileId);

	/// <summary>
	/// Gets all keybindings for the active profile
	/// </summary>
	/// <returns>Dictionary of command ID to key combination mappings</returns>
	IReadOnlyDictionary<string, KeyCombination> GetAllKeybindings();

	/// <summary>
	/// Checks if a key combination is already bound to any command in the specified profile
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="keyCombination">The key combination to check</param>
	/// <returns>True if the key combination is already bound, false otherwise</returns>
	bool IsKeyCombinationBound(string profileId, KeyCombination keyCombination);

	/// <summary>
	/// Checks if a key combination is already bound to any command in the active profile
	/// </summary>
	/// <param name="keyCombination">The key combination to check</param>
	/// <returns>True if the key combination is already bound, false otherwise</returns>
	bool IsKeyCombinationBound(KeyCombination keyCombination);

	/// <summary>
	/// Validates that a keybinding can be set (command exists, profile exists, etc.)
	/// </summary>
	/// <param name="profileId">The profile ID</param>
	/// <param name="commandId">The command ID</param>
	/// <param name="keyCombination">The key combination to validate</param>
	/// <returns>True if the keybinding is valid, false otherwise</returns>
	bool ValidateKeybinding(string profileId, string commandId, KeyCombination keyCombination);

	/// <summary>
	/// Copies all keybindings from one profile to another
	/// </summary>
	/// <param name="sourceProfileId">The source profile ID</param>
	/// <param name="targetProfileId">The target profile ID</param>
	/// <param name="overwriteExisting">Whether to overwrite existing keybindings in the target profile</param>
	/// <returns>True if the operation was successful</returns>
	bool CopyKeybindings(string sourceProfileId, string targetProfileId, bool overwriteExisting = false);
}
