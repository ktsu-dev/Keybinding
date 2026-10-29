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

	[TestMethod]
	public void NamedSeparator_ConstructedDirectly_MatchesTheSymbol()
	{
		Assert.AreEqual(new Note("+"), new Note("Plus"));
		Assert.AreEqual(new Note(","), new Note("comma"));
	}
}
