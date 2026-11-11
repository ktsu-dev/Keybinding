// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// "Menu" is the context-menu key in GLFW, Silk.NET, ImGui and SDL, and Alt only in Win32/WinForms, so the bare
/// name must not fold into the Alt modifier (ktsu-dev/Keybinding#222). LMenu and RMenu only ever mean Alt.
/// </summary>
[TestClass]
public class MenuKeyTests
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

	[TestMethod]
	public void ShiftMenu_IsNotAltShift()
	{
		Assert.AreNotEqual(Chord.Parse("Alt+Shift"), Chord.Parse("Shift+Menu"));
		Assert.AreNotEqual(Chord.Parse("Alt+A"), Chord.Parse("Menu+A"));
	}

	[TestMethod]
	public void Menu_StaysAKeyOfItsOwn()
	{
		Assert.AreEqual("MENU", new Note("Menu").Key.ToString());
		Assert.AreEqual("Shift+MENU", Chord.Parse("Shift+Menu").ToString());
	}

	[TestMethod]
	[DataRow("LMenu+A")]
	[DataRow("RMenu+A")]
	public void SidedMenu_IsStillAlt(string chord) => Assert.AreEqual(Chord.Parse("Alt+A"), Chord.Parse(chord));

	[TestMethod]
	public void BindingShiftAlt_DoesNotDisplaceAShiftMenuBinding()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.Commands.RegisterCommand(new Command("context.menu", "Context menu"));
		manager.Commands.RegisterCommand(new Command("menu.bar", "Menu bar"));
		manager.CreateDefaultProfile();

		Assert.IsTrue(manager.Keybindings.BindChord("context.menu", Chord.Parse("Shift+Menu")));
		Assert.IsTrue(manager.Keybindings.BindChord("menu.bar", Chord.Parse("Shift+Alt")));

		Assert.AreEqual("context.menu", manager.Keybindings.FindCommandByChord(Chord.Parse("Shift+Menu")));
		Assert.AreEqual("menu.bar", manager.Keybindings.FindCommandByChord(Chord.Parse("Shift+Alt")));
	}
}
