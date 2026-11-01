// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;

[TestClass]
public class BlankNoteNameTests
{
	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("\t")]
	public void NoteFromBlankNoteName_ThrowsArgumentException(string key)
	{
		// The string constructor already refused a blank key; the NoteName one built an empty note that
		// displayed as "Ctrl+", executed, and was then dropped when the profile was loaded again (#212).
		NoteName name = NoteName.Create(key);
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => new Note(name));
		Assert.AreEqual("key", exception.ParamName);
		Assert.StartsWith("Key cannot be null or whitespace", exception.Message);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("\t")]
	public void NoteFromBlankString_ThrowsTheSameArgumentException(string key)
	{
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => new Note(key));
		Assert.AreEqual("key", exception.ParamName);
		Assert.StartsWith("Key cannot be null or whitespace", exception.Message);
	}
}
