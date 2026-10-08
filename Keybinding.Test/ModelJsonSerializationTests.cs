// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Text.Json;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class ModelJsonSerializationTests
{
	private static T? RoundTrip<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));

	[TestMethod]
	public void Chord_RoundTrip_ReturnsEqualChord()
	{
		Chord original = Chord.Parse("Ctrl+Shift+S");
		Assert.AreEqual(original, RoundTrip(original));
	}

	[TestMethod]
	public void Phrase_RoundTrip_ReturnsEqualPhrase()
	{
		Phrase original = Phrase.Parse("Ctrl+K, Ctrl+C");
		Assert.AreEqual(original, RoundTrip(original));
	}

	[TestMethod]
	public void Command_SerializesSemanticStringsAsStrings()
	{
		Command command = new("file.save", "Save", "Saves the file", "File");
		using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(command));
		JsonElement root = document.RootElement;

		Assert.AreEqual("file.save", root.GetProperty(nameof(Command.Id)).GetString());
		Assert.AreEqual("Save", root.GetProperty(nameof(Command.Name)).GetString());
		Assert.AreEqual("Saves the file", root.GetProperty(nameof(Command.Description)).GetString());
		Assert.AreEqual("File", root.GetProperty(nameof(Command.Category)).GetString());
	}

	[TestMethod]
	public void Command_RoundTrip_PreservesEveryField()
	{
		Command original = new("file.save", "Save", "Saves the file", "File");
		Command? roundTripped = RoundTrip(original);

		Assert.IsNotNull(roundTripped);
		Assert.AreEqual(original, roundTripped);
		Assert.AreEqual(original.Name, roundTripped.Name);
		Assert.AreEqual(original.Description, roundTripped.Description);
		Assert.AreEqual(original.Category, roundTripped.Category);
	}

	[TestMethod]
	public void Command_RoundTrip_KeepsMissingDescriptionAndCategoryNull()
	{
		Command? roundTripped = RoundTrip(new Command("file.save", "Save"));

		Assert.IsNotNull(roundTripped);
		Assert.IsNull(roundTripped.Description);
		Assert.IsNull(roundTripped.Category);
	}

	[TestMethod]
	public void Profile_RoundTrip_KeepsBoundChords()
	{
		Profile original = new("default", "Default", "The default profile");
		original.SetChord("file.save", Chord.Parse("Ctrl+S"));
		original.SetChord("edit.copy", Chord.Parse("Ctrl+C"));

		Profile? roundTripped = RoundTrip(original);

		Assert.IsNotNull(roundTripped);
		Assert.AreEqual(original, roundTripped);
		Assert.AreEqual(original.Name, roundTripped.Name);
		Assert.AreEqual(original.Description, roundTripped.Description);
		Assert.AreEqual(2, roundTripped.ChordCount);
		Assert.AreEqual(Chord.Parse("Ctrl+S"), roundTripped.GetChord("file.save"));
		Assert.AreEqual(Chord.Parse("Ctrl+C"), roundTripped.GetChord("edit.copy"));
	}
}
