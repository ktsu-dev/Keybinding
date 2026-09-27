// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Text.Json;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class NoteJsonSerializationTests
{
	private static readonly JsonSerializerOptions CamelCaseOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

	[TestMethod]
	public void Serialize_WritesKeyAsString()
	{
		Assert.AreEqual("{\"Key\":\"CTRL\"}", JsonSerializer.Serialize(new Note("Ctrl")));
	}

	[TestMethod]
	public void Serialize_HonorsPropertyNamingPolicy()
	{
		Assert.AreEqual("{\"key\":\"CTRL\"}", JsonSerializer.Serialize(new Note("Ctrl"), CamelCaseOptions));
	}

	[TestMethod]
	public void RoundTrip_ReturnsEqualNote()
	{
		Note original = new("F5");
		Note? roundTripped = JsonSerializer.Deserialize<Note>(JsonSerializer.Serialize(original));
		Assert.AreEqual(original, roundTripped);
	}

	[TestMethod]
	public void Deserialize_LowercaseKey_ReturnsCanonicalNote()
	{
		Assert.AreEqual(new Note("Ctrl"), JsonSerializer.Deserialize<Note>("{\"Key\":\"ctrl\"}"));
	}

	[TestMethod]
	public void Deserialize_AliasKey_ReturnsCanonicalNote()
	{
		Note? note = JsonSerializer.Deserialize<Note>("{\"Key\":\"Control\"}");
		Assert.AreEqual("CTRL", note?.ToString());
	}

	[TestMethod]
	public void Deserialize_CamelCasePropertyName_ReturnsNote()
	{
		Assert.AreEqual(new Note("Alt"), JsonSerializer.Deserialize<Note>("{\"key\":\"alt\"}"));
	}

	[TestMethod]
	public void Deserialize_IgnoresUnknownProperties()
	{
		Assert.AreEqual(new Note("S"), JsonSerializer.Deserialize<Note>("{\"Other\":[1,{\"a\":2}],\"Key\":\"s\"}"));
	}

	[TestMethod]
	public void Deserialize_Null_ReturnsNull()
	{
		Assert.IsNull(JsonSerializer.Deserialize<Note>("null"));
	}

	[TestMethod]
	public void Deserialize_InsideCollection_RoundTrips()
	{
		List<Note> notes = [new("Ctrl"), new("Shift"), new("S")];
		List<Note>? roundTripped = JsonSerializer.Deserialize<List<Note>>(JsonSerializer.Serialize(notes));
		CollectionAssert.AreEqual(notes, roundTripped);
	}

	[TestMethod]
	public void Deserialize_MissingKey_ThrowsJsonException()
	{
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Note>("{}"));
	}

	[TestMethod]
	public void Deserialize_BlankKey_ThrowsJsonException()
	{
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Note>("{\"Key\":\"  \"}"));
	}

	[TestMethod]
	public void Deserialize_NonObject_ThrowsJsonException()
	{
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Note>("[\"CTRL\"]"));
	}
}
