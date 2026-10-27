// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class ChordConflictTests
{
	private string _testDataDirectory = null!;

	[TestInitialize]
	public void Setup() => _testDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_testDataDirectory))
		{
			Directory.Delete(_testDataDirectory, recursive: true);
		}
	}

	private string? RunRepro(bool unbindOpen)
	{
		using KeybindingManager manager = new(_testDataDirectory);
		foreach (string id in new[] { "open", "save", "saveAll" })
		{
			manager.Commands.RegisterCommand(new Command(id, id));
		}

		manager.CreateDefaultProfile();
		manager.Keybindings.BindChord("open", Chord.Parse("Ctrl+O"));
		manager.Keybindings.BindChord("save", Chord.Parse("Ctrl+S"));
		if (unbindOpen)
		{
			manager.Keybindings.UnbindChord("open");
		}

		Assert.IsTrue(manager.Keybindings.BindChord("saveAll", Chord.Parse("Ctrl+S")));
		return manager.Keybindings.ExecuteChord(Chord.Parse("Ctrl+S"));
	}

	[TestMethod]
	public void BindChord_ChordInUse_MovesItToTheNewCommand()
	{
		Assert.AreEqual("saveAll", RunRepro(unbindOpen: false));
		Assert.AreEqual("saveAll", RunRepro(unbindOpen: true), "Unbinding an unrelated command must not change what a chord runs.");
	}

	[TestMethod]
	public void BindChord_ChordInUse_RemovesTheOtherCommandsBinding()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.Commands.RegisterCommand(new Command("save", "Save"));
		manager.Commands.RegisterCommand(new Command("saveAll", "Save All"));
		manager.CreateDefaultProfile();

		manager.Keybindings.BindChord("save", Chord.Parse("Ctrl+S"));
		manager.Keybindings.BindChord("default", "saveAll", Chord.Parse("Ctrl+S"));

		Assert.IsFalse(manager.Keybindings.HasChordBinding("save"));
		Assert.AreEqual(Chord.Parse("Ctrl+S"), manager.Keybindings.GetChord("saveAll"));
	}

	[TestMethod]
	public void SetChords_TwoCommandsOneChord_KeepsOneBinding()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.Commands.RegisterCommand(new Command("a", "A"));
		manager.Commands.RegisterCommand(new Command("b", "B"));
		manager.CreateDefaultProfile();

		manager.SetChords(new Dictionary<string, Chord> { ["a"] = Chord.Parse("Ctrl+S"), ["b"] = Chord.Parse("Ctrl+S") });

		Assert.AreEqual(1, manager.Profiles.GetActiveProfile()!.ChordCount);
	}

	[TestMethod]
	public void BindChord_SameCommandSameChord_KeepsTheBinding()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.Commands.RegisterCommand(new Command("a", "A"));
		manager.CreateDefaultProfile();

		manager.Keybindings.BindChord("a", Chord.Parse("Ctrl+S"));
		manager.Keybindings.BindChord("a", Chord.Parse("Ctrl+S"));

		Assert.AreEqual("a", manager.Keybindings.ExecuteChord(Chord.Parse("Ctrl+S")));
	}
}
