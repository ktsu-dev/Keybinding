// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;
/// <summary>
/// Represents a command that can be executed via keybindings
/// </summary>
public sealed class Command : IEquatable<Command>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Command"/> class
	/// </summary>
	/// <param name="id">The unique identifier for the command</param>
	/// <param name="name">The display name of the command</param>
	/// <param name="description">Optional description of what the command does</param>
	/// <param name="category">Optional category for grouping commands</param>
	/// <exception cref="ArgumentException">Thrown when id or name is null or whitespace</exception>
	public Command(CommandId id, CommandName name, CommandDescription? description = null, CommandCategory? category = null)
	{
		ArgumentNullException.ThrowIfNull(id);
		ArgumentNullException.ThrowIfNull(name);

		Id = id;
		Name = name;
		Description = description;
		Category = category;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Command"/> class from strings
	/// </summary>
	/// <param name="id">The unique identifier for the command</param>
	/// <param name="name">The display name of the command</param>
	/// <param name="description">Optional description of what the command does</param>
	/// <param name="category">Optional category for grouping commands</param>
	/// <exception cref="ArgumentException">Thrown when id or name is null or whitespace</exception>
	public Command(string id, string name, string? description = null, string? category = null)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException("Command ID cannot be null or whitespace", nameof(id));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Command name cannot be null or whitespace", nameof(name));
		}

		Id = CommandId.Create(id.Trim());
		Name = CommandName.Create(name.Trim());
		Description = !string.IsNullOrWhiteSpace(description) ? CommandDescription.Create(description.Trim()) : null;
		Category = !string.IsNullOrWhiteSpace(category) ? CommandCategory.Create(category.Trim()) : null;
	}

	/// <summary>
	/// Gets the unique identifier for the command
	/// </summary>
	public CommandId Id { get; }

	/// <summary>
	/// Gets the display name of the command
	/// </summary>
	public CommandName Name { get; }

	/// <summary>
	/// Gets the description of what the command does
	/// </summary>
	public CommandDescription? Description { get; }

	/// <summary>
	/// Gets the category for grouping commands
	/// </summary>
	public CommandCategory? Category { get; }

	/// <summary>
	/// Returns a string representation of the command
	/// </summary>
	/// <returns>String representation in format "Id: Name"</returns>
	public override string ToString() => $"{Id}: {Name}";

	/// <inheritdoc/>
	public bool Equals(Command? other) =>
		other is not null
		&& (ReferenceEquals(this, other) || Id == other.Id);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Command other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => Id.GetHashCode();

	/// <summary>
	/// Equality operator
	/// </summary>
	public static bool operator ==(Command? left, Command? right) =>
		left?.Equals(right) ?? right is null;

	/// <summary>
	/// Inequality operator
	/// </summary>
	public static bool operator !=(Command? left, Command? right) =>
		!(left == right);
}
