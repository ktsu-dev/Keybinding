// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Text.Json;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class NullElementModelTests
{
	[TestMethod]
	public void Chord_WithNullNote_ThrowsArgumentException() =>
		Assert.ThrowsExactly<ArgumentException>(() => new Chord([new Note("A"), null!]));

	[TestMethod]
	public void Phrase_WithNullChord_ThrowsArgumentException() =>
		Assert.ThrowsExactly<ArgumentException>(() => new Phrase([Chord.Parse("Ctrl+K"), null!]));

	[TestMethod]
	public void Phrase_FromNullChord_ThrowsArgumentNullException() =>
		Assert.ThrowsExactly<ArgumentNullException>(() => new Phrase((Chord)null!));

	[TestMethod]
	[DataRow("{\"Notes\":[null]}")]
	[DataRow("{\"Notes\":[null,{\"Key\":\"A\"}]}")]
	[DataRow("{\"Notes\":null}")]
	[DataRow("{\"Notes\":[]}")]
	[DataRow("{}")]
	public void Chord_InvalidNotesJson_ThrowsJsonException(string json) =>
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Chord>(json));

	[TestMethod]
	[DataRow("{\"Sequence\":[null]}")]
	[DataRow("{\"Sequence\":null}")]
	[DataRow("{\"Sequence\":[]}")]
	[DataRow("{}")]
	public void Phrase_InvalidSequenceJson_ThrowsJsonException(string json) =>
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Phrase>(json));

	[TestMethod]
	[DataRow("{\"Id\":\"a\",\"Name\":\"A\",\"Chords\":{\"x.y\":{\"Notes\":[null]}}}")]
	[DataRow("{\"Id\":\"a\",\"Name\":\"A\",\"Chords\":{\"x.y\":null}}")]
	[DataRow("{\"Id\":\" \",\"Name\":\"A\"}")]
	[DataRow("{\"Name\":\"A\"}")]
	public void Profile_InvalidJson_ThrowsJsonException(string json) =>
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Profile>(json));

	[TestMethod]
	public void ValidJson_StillRoundTrips()
	{
		Phrase phrase = Phrase.Parse("Ctrl+K, Ctrl+C");
		Assert.AreEqual(phrase, JsonSerializer.Deserialize<Phrase>(JsonSerializer.Serialize(phrase)));
	}
}
