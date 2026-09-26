// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// Tests that the '+' and ',' keys can be bound, and that input with a missing key is rejected rather than dropped.
/// </summary>
[TestClass]
public class SeparatorKeyParsingTests
{
	private static KeybindingManager CreateManager() =>
		new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

	[TestMethod]
	public void ChordParse_CtrlPlus_IsCtrlAndPlusKey()
	{
		Chord chord = Chord.Parse("Ctrl++");

		Assert.AreNotEqual(Chord.Parse("Ctrl"), chord);
		Assert.HasCount(2, chord.Notes);
		Assert.IsTrue(chord.Notes.Any(n => n.Key.ToString() == "+"));
	}

	[TestMethod]
	public void ChordParse_CtrlComma_IsCtrlAndCommaKey()
	{
		Chord chord = Chord.Parse("Ctrl+,");

		Assert.HasCount(2, chord.Notes);
		Assert.IsTrue(chord.Notes.Any(n => n.Key.ToString() == ","));
	}

	[TestMethod]
	public void ChordParse_PlusAlone_IsPlusKey()
	{
		Chord chord = Chord.Parse("+");

		Assert.HasCount(1, chord.Notes);
		Assert.AreEqual("+", chord.Notes.Single().Key.ToString());
	}

	[TestMethod]
	public void PhraseParse_CtrlComma_IsSingleChordWithCommaKey()
	{
		Phrase phrase = Phrase.Parse("Ctrl+,");

		Assert.HasCount(1, phrase.Sequence);
		Assert.AreEqual(Chord.Parse("Ctrl+,"), phrase.Sequence[0]);
		Assert.AreNotEqual(Phrase.Parse("Ctrl"), phrase);
	}

	[TestMethod]
	public void PhraseParse_CommaKeyFollowedByChord_SplitsCorrectly()
	{
		Phrase phrase = Phrase.Parse("Ctrl+,, R");

		Assert.HasCount(2, phrase.Sequence);
		Assert.AreEqual(Chord.Parse("Ctrl+,"), phrase.Sequence[0]);
		Assert.AreEqual(Chord.Parse("R"), phrase.Sequence[1]);
	}

	[TestMethod]
	public void PhraseParse_PlusKeyInSequence_SplitsCorrectly()
	{
		Phrase phrase = Phrase.Parse("Ctrl++, Ctrl+R");

		Assert.HasCount(2, phrase.Sequence);
		Assert.AreEqual(Chord.Parse("Ctrl++"), phrase.Sequence[0]);
		Assert.AreEqual(Chord.Parse("Ctrl+R"), phrase.Sequence[1]);
	}

	[TestMethod]
	public void Parse_RoundTripsThroughToString()
	{
		Phrase phrase = Phrase.Parse("Ctrl+,, Ctrl++");

		Assert.AreEqual(phrase, Phrase.Parse(phrase.ToString()));
	}

	[TestMethod]
	[DataRow("Ctrl+")]
	[DataRow("A++B")]
	[DataRow("Ctrl++A+")]
	public void ChordParse_MissingKey_Throws(string value) =>
		Assert.ThrowsExactly<ArgumentException>(() => Chord.Parse(value));

	[TestMethod]
	[DataRow("Ctrl+R,")]
	[DataRow("Ctrl+R,,R")]
	[DataRow(", R")]
	public void PhraseParse_MissingChord_Throws(string value) =>
		Assert.ThrowsExactly<ArgumentException>(() => Phrase.Parse(value));

	[TestMethod]
	public void ServiceParseChord_CtrlPlus_MatchesChordParse()
	{
		using KeybindingManager manager = CreateManager();
		IKeybindingService service = manager.Keybindings;

		Chord chord = service.ParseChord("Ctrl++");

		Assert.HasCount(2, chord.Notes);
		Assert.AreEqual(Chord.Parse("Ctrl++"), chord);
	}

	[TestMethod]
	public void ServiceParsePhrase_CtrlComma_MatchesPhraseParse()
	{
		using KeybindingManager manager = CreateManager();
		IKeybindingService service = manager.Keybindings;

		Phrase phrase = service.ParsePhrase("Ctrl+,");

		Assert.AreEqual(Phrase.Parse("Ctrl+,"), phrase);
	}

	[TestMethod]
	public void ServiceParseChord_MissingKey_Throws()
	{
		using KeybindingManager manager = CreateManager();
		IKeybindingService service = manager.Keybindings;

		Assert.ThrowsExactly<ArgumentException>(() => service.ParseChord("Ctrl+"));
	}
}
