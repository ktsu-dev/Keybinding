// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// Tests that a chord field rejects a phrase string or a key containing whitespace or a comma, rather than
/// inventing a key such as "K, CTRL" that can never be pressed.
/// </summary>
[TestClass]
public class ChordRejectsPhraseTests
{
	private static KeybindingService CreateService() => new(new CommandRegistry(), new ProfileManager());

	[TestMethod]
	[DataRow("Ctrl+K, Ctrl+C")]
	[DataRow("Ctrl+K,Ctrl+C")]
	[DataRow("A, B")]
	[DataRow("Ctrl+,,")]
	public void ChordParse_PhraseString_Throws(string value)
	{
		Assert.ThrowsExactly<ArgumentException>(() => Chord.Parse(value));
		Assert.ThrowsExactly<ArgumentException>(() => CreateService().ParseChord(value));
	}

	[TestMethod]
	[DataRow("Page Up")]
	[DataRow("Ctrl+Page Up")]
	[DataRow("Ctrl+,x")]
	public void ChordParse_KeyWithWhitespaceOrComma_Throws(string value)
	{
		Assert.ThrowsExactly<ArgumentException>(() => Chord.Parse(value));
		Assert.ThrowsExactly<ArgumentException>(() => CreateService().ParseChord(value));
	}

	[TestMethod]
	[DataRow("K, CTRL")]
	[DataRow("PAGE UP")]
	[DataRow("A\tB")]
	[DataRow("A+B")]
	public void Note_KeyWithWhitespaceOrSeparator_Throws(string key)
	{
		Assert.ThrowsExactly<ArgumentException>(() => new Note(key));
	}

	[TestMethod]
	[DataRow(",", ",")]
	[DataRow("+", "+")]
	[DataRow(" , ", ",")]
	public void Note_SeparatorKeyOnItsOwn_IsValid(string key, string expected)
	{
		Assert.AreEqual(expected, new Note(key).Key.ToString());
	}

	[TestMethod]
	public void ChordParse_CtrlComma_StillParses()
	{
		Chord chord = Chord.Parse("Ctrl+,");

		Assert.HasCount(2, chord.Notes);
		Assert.AreEqual(chord, CreateService().ParseChord("Ctrl+,"));
	}

	[TestMethod]
	[DataRow("Ctrl+Alt+S")]
	[DataRow("Ctrl+,")]
	[DataRow("Ctrl++")]
	[DataRow("Shift+F5")]
	[DataRow(",")]
	[DataRow("+")]
	public void ChordParse_ToStringOfValidChord_RoundTrips(string value)
	{
		Chord chord = Chord.Parse(value);

		Assert.AreEqual(chord, Chord.Parse(chord.ToString()));
	}

	[TestMethod]
	public void PhraseParse_PhraseString_StillSplitsIntoChords()
	{
		Phrase phrase = Phrase.Parse("Ctrl+K, Ctrl+C");

		Assert.HasCount(2, phrase.Sequence);
		Assert.AreEqual(Chord.Parse("Ctrl+K"), phrase.Sequence[0]);
		Assert.AreEqual(Chord.Parse("Ctrl+C"), phrase.Sequence[1]);
	}
}
