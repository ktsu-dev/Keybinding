// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Contract for managing command registrations
/// </summary>
public interface ICommandRegistry
{
	/// <summary>
	/// Registers a new command
	/// </summary>
	/// <param name="command">The command to register</param>
	/// <returns>True if the command was registered, false if it already exists</returns>
	bool RegisterCommand(Command command);

	/// <summary>
	/// Unregisters a command
	/// </summary>
	/// <param name="commandId">The ID of the command to unregister</param>
	/// <returns>True if the command was unregistered, false if it didn't exist</returns>
	bool UnregisterCommand(string commandId);

	/// <summary>
	/// Gets a command by its ID
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>The command if found, null otherwise</returns>
	Command? GetCommand(string commandId);

	/// <summary>
	/// Gets all registered commands
	/// </summary>
	/// <returns>Collection of all registered commands</returns>
	IReadOnlyCollection<Command> GetAllCommands();

	/// <summary>
	/// Gets commands by category
	/// </summary>
	/// <param name="category">The category to filter by</param>
	/// <returns>Collection of commands in the specified category</returns>
	IReadOnlyCollection<Command> GetCommandsByCategory(string? category);

	/// <summary>
	/// Searches for commands by name or description
	/// </summary>
	/// <param name="searchTerm">The term to search for</param>
	/// <param name="caseSensitive">Whether the search should be case sensitive</param>
	/// <returns>Collection of matching commands</returns>
	IReadOnlyCollection<Command> SearchCommands(string searchTerm, bool caseSensitive = false);

	/// <summary>
	/// Checks if a command is registered
	/// </summary>
	/// <param name="commandId">The command ID</param>
	/// <returns>True if the command is registered, false otherwise</returns>
	bool IsCommandRegistered(string commandId);

	/// <summary>
	/// Clears all registered commands
	/// </summary>
	void ClearCommands();
}
