// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ParserUnificationTests
{
	private static KeybindingService CreateService() => new(new CommandRegistry(), new ProfileManager());

	[TestMethod]
	[DataRow("Ctrl+S")]
	[DataRow("Control+S")]
	[DataRow("cmd+shift+p")]
	[DataRow("Ctrl++")]
	[DataRow("Ctrl+,")]
	[DataRow("Ctrl+Plus")]
	[DataRow("Esc")]
	public void ParseChord_MatchesChordParse(string value) =>
		Assert.AreEqual(Chord.Parse(value), CreateService().ParseChord(value));

	[TestMethod]
	[DataRow("Ctrl+R, R")]
	[DataRow("Ctrl+K, Ctrl+C")]
	[DataRow("Ctrl+,")]
	[DataRow("Control+K, Esc")]
	public void ParsePhrase_MatchesPhraseParse(string value) =>
		Assert.AreEqual(Phrase.Parse(value), CreateService().ParsePhrase(value));

	[TestMethod]
	[DataRow("Ctrl+K, Ctrl+C")]
	[DataRow("Page Up")]
	[DataRow("Ctrl+")]
	public void ParseChord_InvalidInput_ThrowsLikeChordParse(string value)
	{
		Type expected = CaptureExceptionType(() => Chord.Parse(value));
		Type actual = CaptureExceptionType(() => CreateService().ParseChord(value));
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("Ctrl+K,, Ctrl+C")]
	[DataRow("Page Up")]
	public void ParsePhrase_InvalidInput_ThrowsLikePhraseParse(string value)
	{
		Type expected = CaptureExceptionType(() => Phrase.Parse(value));
		Type actual = CaptureExceptionType(() => CreateService().ParsePhrase(value));
		Assert.AreEqual(expected, actual);
	}

	private static Type CaptureExceptionType(Action action)
	{
		try
		{
			action();
		}
		catch (ArgumentException ex)
		{
			return ex.GetType();
		}

		Assert.Fail("Expected the input to be rejected.");
		return typeof(void);
	}
}
