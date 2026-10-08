// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Models;

using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.Semantics.Strings;

/// <summary>
/// Serializes a semantic string as a plain JSON string. Without it System.Text.Json treats the
/// semantic string as a collection of chars, writing an array it cannot read back. Reading goes
/// through <see cref="SemanticString{TDerived}.Create(string)"/>, so the type's validation still applies.
/// </summary>
/// <typeparam name="T">The semantic string type</typeparam>
internal sealed class SemanticStringJsonConverter<T> : JsonConverter<T>
	where T : SemanticString<T>
{
	/// <inheritdoc/>
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
		{
			throw new JsonException($"Expected a string for {typeof(T).Name} but found {reader.TokenType}.");
		}

		string value = reader.GetString()!;
		try
		{
			return SemanticString<T>.Create(value);
		}
		catch (ArgumentException ex)
		{
			throw new JsonException($"'{value}' is not a valid {typeof(T).Name}.", ex);
		}
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		Ensure.NotNull(writer);
		Ensure.NotNull(value);

		writer.WriteStringValue(value.ToString());
	}
}
