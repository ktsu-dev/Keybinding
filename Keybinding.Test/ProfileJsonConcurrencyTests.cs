// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Text.Json;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class ProfileJsonConcurrencyTests
{
	private static readonly string[] ProfileJsonProperties = ["Id", "Name", "Description", "Chords", "ChordCount", "BoundCommands"];

	// SetChord takes a chord from whichever command held it, so each of the 200 bindings needs a chord of its
	// own: 25 keys under 8 modifier sets. Q is left out because the writer below binds Ctrl+Q, which would
	// otherwise take it from one of these.
	private const string Keys = "ABCDEFGHIJKLMNOPRSTUVWXYZ";
	private static readonly string[] Modifiers = ["Ctrl", "Alt", "Shift", "Meta", "Ctrl+Alt", "Ctrl+Shift", "Alt+Shift", "Ctrl+Alt+Shift"];

	[TestMethod]
	[Timeout(60000, CooperativeCancellation = true)]
	public void Serialize_WhileAnotherThreadChangesBindings_WritesASnapshot()
	{
		Profile profile = new("p", "Profile");
		for (int i = 0; i < 200; i++)
		{
			profile.SetChord($"cmd{i}", Chord.Parse($"{Modifiers[i / Keys.Length]}+{Keys[i % Keys.Length]}"));
		}

		using CancellationTokenSource stop = new();
		Task writer = Task.Run(() =>
		{
			int i = 0;
			while (!stop.IsCancellationRequested)
			{
				string id = $"extra{i++ % 50}";
				profile.SetChord(id, Chord.Parse("Ctrl+Q"));
				profile.RemoveChord(id);
			}
		});

		try
		{
			for (int i = 0; i < 2000; i++)
			{
				string json = JsonSerializer.Serialize(profile);
				using JsonDocument document = JsonDocument.Parse(json);
				JsonElement chords = document.RootElement.GetProperty("Chords");
				int count = chords.EnumerateObject().Count();
				Assert.IsTrue(count is >= 200 and <= 250, $"Serialized {count} bindings.");
			}
		}
		finally
		{
			stop.Cancel();
			writer.Wait();
		}
	}

	[TestMethod]
	public void Serialize_KeepsTheJsonShape()
	{
		Profile profile = new("p", "Profile", "Description");
		profile.SetChord("file.save", Chord.Parse("Ctrl+S"));

		using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(profile));
		JsonElement root = document.RootElement;

		CollectionAssert.AreEquivalent(
			ProfileJsonProperties,
			root.EnumerateObject().Select(p => p.Name).ToArray());
		Assert.AreEqual(1, root.GetProperty("Chords").EnumerateObject().Count());
		Assert.AreEqual(JsonSerializer.Serialize(Chord.Parse("Ctrl+S")), root.GetProperty("Chords").GetProperty("file.save").GetRawText());
	}
}
