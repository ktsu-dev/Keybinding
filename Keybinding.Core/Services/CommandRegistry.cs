// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Services;

/// <summary>
/// Implementation of command registry for managing command registrations
/// </summary>
public sealed class CommandRegistry : ICommandRegistry
{
	private readonly ConcurrentDictionary<string, Command> _commands = new();
	private readonly object _lock = new();

	/// <inheritdoc/>
	public bool RegisterCommand(Command command)
	{
		ArgumentNullException.ThrowIfNull(command);

		return _commands.TryAdd(command.Id, command);
	}

	/// <inheritdoc/>
	public bool UnregisterCommand(string commandId)
	{
		if (string.IsNullOrWhiteSpace(commandId))
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));

		return _commands.TryRemove(commandId.Trim(), out _);
	}

	/// <inheritdoc/>
	public Command? GetCommand(string commandId)
	{
		if (string.IsNullOrWhiteSpace(commandId))
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));

		return _commands.TryGetValue(commandId.Trim(), out var command) ? command : null;
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<Command> GetAllCommands()
	{
		lock (_lock)
		{
			return _commands.Values.ToList().AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<Command> GetCommandsByCategory(string? category)
	{
		lock (_lock)
		{
			var normalizedCategory = category?.Trim();
			return _commands.Values
				.Where(c => string.Equals(c.Category, normalizedCategory, StringComparison.OrdinalIgnoreCase))
				.ToList()
				.AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<Command> SearchCommands(string searchTerm, bool caseSensitive = false)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
			throw new ArgumentException("Search term cannot be null or whitespace", nameof(searchTerm));

		var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		var normalizedSearchTerm = searchTerm.Trim();

		lock (_lock)
		{
			return _commands.Values
				.Where(c =>
					c.Name.Contains(normalizedSearchTerm, comparison) ||
					c.Id.Contains(normalizedSearchTerm, comparison) ||
					(!string.IsNullOrEmpty(c.Description) && c.Description.Contains(normalizedSearchTerm, comparison)) ||
					(!string.IsNullOrEmpty(c.Category) && c.Category.Contains(normalizedSearchTerm, comparison)))
				.OrderBy(c => c.Name, StringComparer.Create(CultureInfo.CurrentCulture, !caseSensitive))
				.ToList()
				.AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public bool IsCommandRegistered(string commandId)
	{
		if (string.IsNullOrWhiteSpace(commandId))
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId));

		return _commands.ContainsKey(commandId.Trim());
	}

	/// <inheritdoc/>
	public void ClearCommands()
	{
		_commands.Clear();
	}
}
