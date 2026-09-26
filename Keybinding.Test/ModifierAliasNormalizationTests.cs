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
		];

		foreach (Chord chord in built)
		{
			Assert.AreEqual(expected, chord, $"{chord} should equal {expected}");
			Assert.AreEqual(expected.GetHashCode(), chord.GetHashCode(), $"{chord} should hash like {expected}");
		}
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
}
