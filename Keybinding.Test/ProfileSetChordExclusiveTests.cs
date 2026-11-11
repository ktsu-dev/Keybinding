// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Text.Json;
using System.Text.Json.Nodes;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// <see cref="Profile.SetChord"/> keeps the one-command-per-chord policy that <c>BindChord</c> keeps, while
/// stored data that already binds one chord to several commands still loads as it is.
/// </summary>
[TestClass]
public class ProfileSetChordExclusiveTests
{
	private static readonly Chord CtrlS = Chord.Parse("Ctrl+S");

	private string _dataDirectory = null!;

	[TestInitialize]
	public void Setup() => _dataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_dataDirectory))
		{
			Directory.Delete(_dataDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void SetChord_ChordHeldByAnotherCommand_TakesItFromThatCommand()
	{
		Profile profile = new("p", "Profile");
		profile.SetChord("file.save", CtrlS);

		profile.SetChord("file.saveAll", CtrlS);

		Assert.AreEqual(CtrlS, profile.GetChord("file.saveAll"));
		Assert.IsNull(profile.GetChord("file.save"));
		Assert.AreEqual(1, profile.ChordCount);
	}

	[TestMethod]
	public void SetChord_SameCommandSameChord_KeepsTheBinding()
	{
		Profile profile = new("p", "Profile");
		profile.SetChord("a", CtrlS);

		profile.SetChord(" a ", CtrlS);

		Assert.AreEqual(CtrlS, profile.GetChord("a"));
		Assert.AreEqual(1, profile.ChordCount);
	}

	[TestMethod]
	public void SetChord_ThroughTheActiveProfile_MakesTheChordRunTheNewCommand()
	{
		using KeybindingManager manager = new(_dataDirectory);
		manager.CreateDefaultProfile();
		manager.Commands.RegisterCommand(new Command("file.save", "Save"));
		manager.Commands.RegisterCommand(new Command("file.saveAll", "Save All"));
		Assert.IsTrue(manager.Keybindings.BindChord("file.save", CtrlS));
		Profile profile = manager.Profiles.GetActiveProfile()!;

		profile.SetChord("file.saveAll", CtrlS);

		Assert.IsNull(profile.GetChord("file.save"));
		Assert.AreEqual("file.saveAll", manager.Keybindings.ExecuteChord(CtrlS));
		Assert.AreEqual("file.saveAll", manager.Keybindings.FindCommandByChord(CtrlS));
	}

	[TestMethod]
	public void Deserialize_StoredConflict_KeepsBothBindings()
	{
		Profile single = new("p", "Profile");
		single.SetChord("a", CtrlS);
		JsonNode node = JsonNode.Parse(JsonSerializer.Serialize(single))!;
		JsonObject chords = node["Chords"]!.AsObject();
		chords["b"] = chords["a"]!.DeepClone();

		Profile? profile = JsonSerializer.Deserialize<Profile>(node.ToJsonString());

		Assert.IsNotNull(profile);
		Assert.AreEqual(2, profile.ChordCount);
		Assert.AreEqual(CtrlS, profile.GetChord("a"));
		Assert.AreEqual(CtrlS, profile.GetChord("b"));
	}

	[TestMethod]
	public async Task Load_StoredConflict_KeepsBothBindingsAndResolvesByOrdinalId()
	{
		using (KeybindingManager writer = new(_dataDirectory))
		{
			await writer.InitializeAsync().ConfigureAwait(false);
			writer.CreateDefaultProfile();
			Profile stored = writer.Profiles.GetProfile("default")!;
#pragma warning disable CS0618 // The obsolete raw dictionary is the only way left to store a conflict.
			stored.Chords["b"] = CtrlS;
			stored.Chords["a"] = CtrlS;
#pragma warning restore CS0618
			await writer.SaveAsync().ConfigureAwait(false);
		}

		using KeybindingManager reader = new(_dataDirectory);
		await reader.InitializeAsync().ConfigureAwait(false);
		reader.Commands.RegisterCommand(new Command("a", "A"));
		reader.Commands.RegisterCommand(new Command("b", "B"));
		Profile loaded = reader.Profiles.GetProfile("default")!;

		Assert.AreEqual(CtrlS, loaded.GetChord("a"));
		Assert.AreEqual(CtrlS, loaded.GetChord("b"));
		Assert.AreEqual("a", reader.Keybindings.ExecuteChord(CtrlS));

		Profile? copy = reader.Profiles.DuplicateProfile("default", "copy", "Copy");
		Assert.IsNotNull(copy);
		Assert.AreEqual(2, copy.ChordCount, "Duplicating copies the stored bindings as they are.");
	}
}
