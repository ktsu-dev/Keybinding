// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Models;

using System.Text.Json;

/// <summary>
/// Checks the values System.Text.Json hands to a model's [JsonConstructor] before the public constructor sees
/// them, so malformed JSON fails with a <see cref="JsonException"/>, as <see cref="NoteJsonConverter"/> does,
/// rather than with an argument exception a caller catching <see cref="JsonException"/> would miss.
/// </summary>
internal static class JsonArguments
{
	/// <summary>
	/// Requires a deserialized list to be present, non-empty and free of nulls.
	/// </summary>
	/// <typeparam name="T">The element type</typeparam>
	/// <param name="items">The deserialized list</param>
	/// <param name="typeName">The model being deserialized</param>
	/// <param name="propertyName">The property the list came from</param>
	/// <returns>The list, unchanged</returns>
	/// <exception cref="JsonException">Thrown when the list is missing, empty or contains a null</exception>
	internal static IReadOnlyList<T> RequireItems<T>(IReadOnlyList<T>? items, string typeName, string propertyName) where T : class
	{
		if (items is null)
		{
			throw new JsonException($"{typeName} is missing its {propertyName} property.");
		}

		if (items.Count == 0)
		{
			throw new JsonException($"{typeName}.{propertyName} must contain at least one entry.");
		}

		return items.Any(item => item is null)
			? throw new JsonException($"{typeName}.{propertyName} cannot contain null.")
			: items;
	}

	/// <summary>
	/// Requires a deserialized string to be present and not blank.
	/// </summary>
	/// <param name="value">The deserialized string</param>
	/// <param name="typeName">The model being deserialized</param>
	/// <param name="propertyName">The property the string came from</param>
	/// <returns>The string, unchanged</returns>
	/// <exception cref="JsonException">Thrown when the string is missing or blank</exception>
	internal static string RequireText(string? value, string typeName, string propertyName) =>
		string.IsNullOrWhiteSpace(value)
			? throw new JsonException($"{typeName}.{propertyName} cannot be null or whitespace.")
			: value;
}
