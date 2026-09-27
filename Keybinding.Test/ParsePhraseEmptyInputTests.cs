// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Services;

[TestClass]
public class ParsePhraseEmptyInputTests
{
	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	[DataRow(null)]
	public void ParsePhrase_BlankInput_ThrowsNamingPhraseString(string? phraseString)
	{
		KeybindingService service = new(new CommandRegistry(), new ProfileManager());

		ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() => service.ParsePhrase(phraseString!));
		Assert.AreEqual("phraseString", ex.ParamName, "The exception should name the caller's argument, not the Phrase constructor's.");
	}
}
