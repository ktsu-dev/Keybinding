// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;

[TestClass]
public class KeyAliasEquivalenceTests
{
	[TestMethod]
	[DataRow("Ctrl+Plus", "Ctrl++")]
	[DataRow("Ctrl+OemPlus", "Ctrl++")]
	[DataRow("Ctrl+Comma", "Ctrl+,")]
	[DataRow("Ctrl+OemComma", "Ctrl+,")]
	[DataRow("Ctrl+Minus", "Ctrl+-")]
	[DataRow("Ctrl+OemMinus", "Ctrl+-")]
	[DataRow("Ctrl+Period", "Ctrl+.")]
	[DataRow("Ctrl+OemPeriod", "Ctrl+.")]
	[DataRow("Back", "Backspace")]
	[DataRow("Prior", "PageUp")]
	[DataRow("Next", "PageDown")]
	[DataRow("Capital", "CapsLock")]
	[DataRow("Ctrl+Spacebar", "Ctrl+Space")]
	[DataRow("Snapshot", "PrintScreen")]
	[DataRow("Scroll", "ScrollLock")]
	[DataRow("LWin+A", "Meta+A")]
	[DataRow("RWin+A", "Meta+A")]
	[DataRow("LeftWindows+A", "Meta+A")]
	[DataRow("RightWindows+A", "Meta+A")]
	[DataRow("LControlKey+A", "Ctrl+A")]
	[DataRow("RControlKey+A", "Ctrl+A")]
	[DataRow("ControlKey+A", "Ctrl+A")]
	[DataRow("LeftCtrl+A", "Ctrl+A")]
	[DataRow("RightCtrl+A", "Ctrl+A")]
	[DataRow("LShiftKey+A", "Shift+A")]
	[DataRow("RShiftKey+A", "Shift+A")]
	[DataRow("ShiftKey+A", "Shift+A")]
	[DataRow("LeftShift+A", "Shift+A")]
	[DataRow("RightShift+A", "Shift+A")]
	[DataRow("Menu+A", "Alt+A")]
	[DataRow("LMenu+A", "Alt+A")]
	[DataRow("RMenu+A", "Alt+A")]
	[DataRow("LeftAlt+A", "Alt+A")]
	[DataRow("RightAlt+A", "Alt+A")]
	// Browser KeyboardEvent.code, Silk.NET Key, ImGuiKey, GLFW and macOS spellings (ktsu-dev/Keybinding#205)
	[DataRow("ControlLeft+C", "Ctrl+C")]
	[DataRow("ControlRight+C", "Ctrl+C")]
	[DataRow("ShiftLeft+C", "Shift+C")]
	[DataRow("ShiftRight+C", "Shift+C")]
	[DataRow("AltLeft+C", "Alt+C")]
	[DataRow("AltRight+C", "Alt+C")]
	[DataRow("MetaLeft+C", "Meta+C")]
	[DataRow("MetaRight+C", "Meta+C")]
	[DataRow("SuperLeft+C", "Meta+C")]
	[DataRow("SuperRight+C", "Meta+C")]
	[DataRow("LeftSuper+C", "Meta+C")]
	[DataRow("RightSuper+C", "Meta+C")]
	[DataRow("OSLeft+C", "Meta+C")]
	[DataRow("OSRight+C", "Meta+C")]
	[DataRow("LeftCommand+C", "Meta+C")]
	[DataRow("RightCommand+C", "Meta+C")]
	[DataRow("LEFT_CONTROL+C", "Ctrl+C")]
	[DataRow("RIGHT_CONTROL+C", "Ctrl+C")]
	[DataRow("LEFT_SHIFT+C", "Shift+C")]
	[DataRow("RIGHT_SHIFT+C", "Shift+C")]
	[DataRow("LEFT_ALT+C", "Alt+C")]
	[DataRow("RIGHT_ALT+C", "Alt+C")]
	[DataRow("LEFT_SUPER+C", "Meta+C")]
	[DataRow("RIGHT_SUPER+C", "Meta+C")]
	public void Alias_EqualsAndHashesLikeItsCanonicalForm(string alias, string canonical)
	{
		Chord fromAlias = Chord.Parse(alias);
		Chord fromCanonical = Chord.Parse(canonical);

		Assert.AreEqual(fromCanonical, fromAlias);
		Assert.AreEqual(fromCanonical.GetHashCode(), fromAlias.GetHashCode());
		Assert.AreEqual(fromCanonical.ToString(), fromAlias.ToString());
	}

	[TestMethod]
	[DataRow("LWin+A", "Meta+A")]
	[DataRow("A+LeftCtrl+RightShift", "Ctrl+Shift+A")]
	[DataRow("RMenu+LWin+LControlKey+X", "Ctrl+Alt+Meta+X")]
	[DataRow("C+ControlLeft", "Ctrl+C")]
	[DataRow("C+SuperLeft+ShiftRight", "Shift+Meta+C")]
	[DataRow("X+LEFT_SUPER+LEFT_ALT+LEFT_CONTROL", "Ctrl+Alt+Meta+X")]
	public void LeftAndRightModifiers_SortAndDisplayAsModifiers(string value, string expected) =>
		Assert.AreEqual(expected, Chord.Parse(value).ToString());

	[TestMethod]
	[DataRow("Ctrl+Add", "Ctrl++")]
	[DataRow("Ctrl+Subtract", "Ctrl+-")]
	[DataRow("Ctrl+Decimal", "Ctrl+.")]
	[DataRow("Ctrl+Multiply", "Ctrl+*")]
	[DataRow("Ctrl+Divide", "Ctrl+/")]
	[DataRow("Ctrl+NumPad1", "Ctrl+1")]
	public void NumpadKey_StaysDistinctFromTheMainRowKey(string numpad, string mainRow)
	{
		Chord fromNumpad = Chord.Parse(numpad);
		Chord fromMainRow = Chord.Parse(mainRow);

		Assert.AreNotEqual(fromMainRow, fromNumpad);
		Assert.AreNotEqual(fromMainRow.ToString(), fromNumpad.ToString());
	}

	/// <summary>
	/// These OEM keys name a physical key whose character depends on the keyboard layout, so they do not fold
	/// to the character the key produces on a US layout (ktsu-dev/Keybinding#198).
	/// </summary>
	[TestMethod]
	[DataRow("Ctrl+OemQuestion", "Ctrl+/")]
	[DataRow("Ctrl+Oem2", "Ctrl+/")]
	[DataRow("Ctrl+OemSemicolon", "Ctrl+;")]
	[DataRow("Ctrl+Oem1", "Ctrl+;")]
	[DataRow("Ctrl+OemQuotes", "Ctrl+'")]
	[DataRow("Ctrl+Oem7", "Ctrl+'")]
	[DataRow("Ctrl+OemOpenBrackets", "Ctrl+[")]
	[DataRow("Ctrl+Oem4", "Ctrl+[")]
	[DataRow("Ctrl+OemCloseBrackets", "Ctrl+]")]
	[DataRow("Ctrl+Oem6", "Ctrl+]")]
	[DataRow("Ctrl+OemPipe", "Ctrl+\\")]
	[DataRow("Ctrl+Oem5", "Ctrl+\\")]
	[DataRow("Ctrl+Oemtilde", "Ctrl+`")]
	[DataRow("Ctrl+Oem3", "Ctrl+`")]
	public void LayoutDependentOemKey_StaysDistinctFromItsUsLayoutCharacter(string oem, string usCharacter)
	{
		Chord fromOem = Chord.Parse(oem);
		Chord fromCharacter = Chord.Parse(usCharacter);

		Assert.AreNotEqual(fromCharacter, fromOem);
		Assert.AreNotEqual(fromCharacter.ToString(), fromOem.ToString());
	}

	[TestMethod]
	public void NamedSeparator_ConstructedDirectly_MatchesTheSymbol()
	{
		Assert.AreEqual(new Note("+"), new Note("Plus"));
		Assert.AreEqual(new Note(","), new Note("comma"));
	}
}
