// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Serializes a <see cref="Note"/> as <c>{"Key":"CTRL"}</c>. Without it System.Text.Json treats the
/// <see cref="NoteName"/> key as a collection of chars, writing an array it cannot read back. Reading goes
/// through <see cref="Note(string)"/>, so lowercase and alias input comes back as the canonical note.
/// </summary>
internal sealed class NoteJsonConverter : JsonConverter<Note>
{
	private const string KeyPropertyName = nameof(Note.Key);

	/// <inheritdoc/>
	public override Note Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException($"Expected an object for {nameof(Note)} but found {reader.TokenType}.");
		}

		string? key = null;
		bool foundKey = false;
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				return foundKey
					? CreateNote(key!)
					: throw new JsonException($"{nameof(Note)} is missing its {KeyPropertyName} property.");
			}

			string propertyName = reader.GetString()!;
			reader.Read();
			if (string.Equals(propertyName, KeyPropertyName, StringComparison.OrdinalIgnoreCase))
			{
				if (reader.TokenType != JsonTokenType.String)
				{
					throw new JsonException($"Expected a string for {nameof(Note)}.{KeyPropertyName} but found {reader.TokenType}.");
				}

				key = reader.GetString();
				foundKey = true;
			}
			else
			{
				reader.Skip();
			}
		}

		throw new JsonException($"Unexpected end of JSON while reading a {nameof(Note)}.");
	}

	private static Note CreateNote(string key)
	{
		try
		{
			return new Note(key);
		}
		catch (ArgumentException ex)
		{
			throw new JsonException($"'{key}' is not a valid {nameof(Note)} key.", ex);
		}
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, Note value, JsonSerializerOptions options)
	{
		Ensure.NotNull(writer);
		Ensure.NotNull(value);
		Ensure.NotNull(options);

		writer.WriteStartObject();
		writer.WriteString(options.PropertyNamingPolicy?.ConvertName(KeyPropertyName) ?? KeyPropertyName, value.Key.ToString());
		writer.WriteEndObject();
	}
}
