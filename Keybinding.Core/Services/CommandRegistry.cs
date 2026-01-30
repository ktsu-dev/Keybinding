// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Services;

using System.Collections.Concurrent;
using System.Globalization;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Implementation of command registry for managing command registrations
/// </summary>
public sealed class CommandRegistry : ICommandRegistry
{
	private readonly ConcurrentDictionary<string, Command> _commands = new();
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	/// <inheritdoc/>
	public bool RegisterCommand(Command command)
	{
		Ensure.NotNull(command);

		return _commands.TryAdd(command.Id, command);
	}

	/// <inheritdoc/>
	public bool UnregisterCommand(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: _commands.TryRemove(commandId.Trim(), out _);
	}

	/// <inheritdoc/>
	public Command? GetCommand(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: _commands.TryGetValue(commandId.Trim(), out Command? command) ? command : null;
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
			string? normalizedCategory = category?.Trim();
			return _commands.Values
				.Where(c => string.Equals(c.Category, normalizedCategory, StringComparison.OrdinalIgnoreCase))
				.ToList()
				.AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<Command> SearchCommands(string searchTerm, CaseSensitivity caseSensitivity = CaseSensitivity.CaseInsensitive)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			throw new ArgumentException("Search term cannot be null or whitespace", nameof(searchTerm));
		}

		bool isCaseSensitive = caseSensitivity == CaseSensitivity.CaseSensitive;
		StringComparison comparison = isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		string normalizedSearchTerm = searchTerm.Trim();

		lock (_lock)
		{
			return _commands.Values
				.Where(c =>
					c.Name.Contains(normalizedSearchTerm, comparison) ||
					c.Id.Contains(normalizedSearchTerm, comparison) ||
					(!string.IsNullOrEmpty(c.Description) && c.Description.Contains(normalizedSearchTerm, comparison)) ||
					(!string.IsNullOrEmpty(c.Category) && c.Category.Contains(normalizedSearchTerm, comparison)))
				.OrderBy(c => c.Name, StringComparer.Create(CultureInfo.CurrentCulture, !isCaseSensitive))
				.ToList()
				.AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public bool IsCommandRegistered(string commandId)
	{
		return string.IsNullOrWhiteSpace(commandId)
			? throw new ArgumentException("Command ID cannot be null or whitespace", nameof(commandId))
			: _commands.ContainsKey(commandId.Trim());
	}

	/// <inheritdoc/>
	public void ClearCommands() => _commands.Clear();
}
