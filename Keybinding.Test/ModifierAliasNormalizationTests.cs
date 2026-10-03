// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ModifierAliasNormalizationTests
{
	private string _testDataDirectory = null!;

	[TestInitialize]
	public void Setup()
	{
		_testDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testDataDirectory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_testDataDirectory))
		{
			Directory.Delete(_testDataDirectory, recursive: true);
		}
	}

	private static KeybindingService CreateService()
	{
		CommandRegistry registry = new();
		registry.RegisterCommand(new Command("save", "Save"));
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Profile");
		profiles.SetActiveProfile("p");
		return new KeybindingService(registry, profiles);
	}

	[TestMethod]
	[DataRow("Control", "Ctrl")]
	[DataRow("Win", "Meta")]
	[DataRow("Windows", "Meta")]
	[DataRow("Cmd", "Meta")]
	[DataRow("Command", "Meta")]
	public void EveryConstructionPath_NormalizesModifierAliases(string alias, string canonical)
	{
		ArgumentNullException.ThrowIfNull(alias);
		Chord expected = Chord.Parse($"{canonical}+S");
		KeybindingService service = CreateService();

		Chord[] built =
		[
			service.ParseChord($"{alias}+S"),
			Chord.Parse($"{alias}+S"),
			new Chord([new Note(alias), new Note("S")]),
			new Chord([new Note(NoteName.Create(alias.ToUpperInvariant())), new Note("S")]),
			new Chord([new Note(NoteName.Create(alias)), new Note(NoteName.Create("s"))]),
			new Chord([new Note(NoteName.Create(alias.ToLowerInvariant())), new Note("S")]),
		];

		foreach (Chord chord in built)
		{
			Assert.AreEqual(expected, chord, $"{chord} should equal {expected}");
			Assert.AreEqual(expected.GetHashCode(), chord.GetHashCode(), $"{chord} should hash like {expected}");
		}
	}

	[TestMethod]
	[DataRow("Esc", "Escape")]
	[DataRow("Return", "Enter")]
	[DataRow("Del", "Delete")]
	[DataRow("Ins", "Insert")]
	[DataRow("PgUp", "PageUp")]
	[DataRow("PgDn", "PageDown")]
	[DataRow("Up", "ArrowUp")]
	[DataRow("UpArrow", "ArrowUp")]
	[DataRow("Down", "ArrowDown")]
	[DataRow("DownArrow", "ArrowDown")]
	[DataRow("Left", "ArrowLeft")]
	[DataRow("LeftArrow", "ArrowLeft")]
	[DataRow("Right", "ArrowRight")]
	[DataRow("RightArrow", "ArrowRight")]
	[DataRow("Option", "Alt")]
	[DataRow("Super", "Meta")]
	[DataRow("D0", "0")]
	[DataRow("D1", "1")]
	[DataRow("D2", "2")]
	[DataRow("D3", "3")]
	[DataRow("D4", "4")]
	[DataRow("D5", "5")]
	[DataRow("D6", "6")]
	[DataRow("D7", "7")]
	[DataRow("D8", "8")]
	[DataRow("D9", "9")]
	public void ChordParse_KeyAliasesMatchCanonicalName(string alias, string canonical)
	{
		ArgumentNullException.ThrowIfNull(alias);
		Chord expected = Chord.Parse($"Ctrl+{canonical}");
		Chord actual = Chord.Parse($"Ctrl+{alias}");

		Assert.AreEqual(expected, actual);
		Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode());
		Assert.AreEqual(new Note(canonical), new Note(NoteName.Create(alias.ToUpperInvariant())));
	}

	[TestMethod]
	[DataRow("control")]
	[DataRow("Control")]
	[DataRow("ctrl")]
	[DataRow("s")]
	[DataRow("f5")]
	[DataRow(" cmd ")]
	public void NoteNameConstructor_MatchesStringConstructor(string key)
	{
		Note fromString = new(key);
		Note fromNoteName = new(NoteName.Create(key));

		Assert.AreEqual(fromString, fromNoteName);
		Assert.AreEqual(fromString.GetHashCode(), fromNoteName.GetHashCode());
		Assert.AreEqual(fromString.Key.ToString(), fromNoteName.Key.ToString());
	}

	[TestMethod]
	public void ParseChord_AliasAndCanonicalName_CollapseToOneNote()
	{
		Chord chord = CreateService().ParseChord("Ctrl+Control");

		Assert.AreEqual(1, chord.Notes.Count);
		Assert.AreEqual("Ctrl", chord.ToString());
	}

	[TestMethod]
	public void BindChord_WithAliasSpelling_IsFoundByCanonicalChord()
	{
		KeybindingService service = CreateService();
		Assert.IsTrue(service.BindChord("save", service.ParseChord("Control+S")));

		Assert.AreEqual("save", service.FindCommandByChord(Chord.Parse("Ctrl+S")));
	}

	[TestMethod]
	public async Task StoredProfileWithAliasSpelling_MatchesAfterReload()
	{
		string json = """
			[
			  {
			    "id": "p",
			    "name": "Profile",
			    "chords": {
			      "save": { "notes": ["CONTROL", "S"] },
			      "find": { "notes": ["CMD", "F"] }
			    }
			  }
			]
			""";
		await File.WriteAllTextAsync(Path.Combine(_testDataDirectory, Constants.Files.ProfilesFileName), json).ConfigureAwait(false);

		JsonKeybindingRepository repository = new(_testDataDirectory);
		Profile? profile = await repository.LoadProfileAsync("p").ConfigureAwait(false);

		Assert.IsNotNull(profile);
		Assert.AreEqual(Chord.Parse("Ctrl+S"), profile.GetChord("save"));
		Assert.AreEqual(Chord.Parse("Meta+F"), profile.GetChord("find"));
	}

	[TestMethod]
	public async Task StoredProfileWithKeyAlias_ExecutesCanonicalChordAfterLoad()
	{
		string json = """
			[{"id":"p","name":"Profile","chords":{"save":{"notes":["CTRL","ESC"]}}}]
			""";
		await File.WriteAllTextAsync(Path.Combine(_testDataDirectory, Constants.Files.ProfilesFileName), json).ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		manager.Commands.RegisterCommand(new Command("save", "Save"));
		manager.Profiles.SetActiveProfile("p");

		Assert.AreEqual("save", manager.Keybindings.ExecuteChord(Chord.Parse("Ctrl+Escape")));
	}
}
