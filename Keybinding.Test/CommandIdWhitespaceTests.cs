// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class CommandIdWhitespaceTests
{
	private static Command CreatePaddedCommand() =>
		new(CommandId.Create(" file.save "), CommandName.Create("Save"));

	[TestMethod]
	public void Constructor_CommandIdWithWhitespace_TrimsId()
	{
		Assert.AreEqual("file.save", CreatePaddedCommand().Id.ToString());
	}

	[TestMethod]
	public void Constructor_CommandIdWithWhitespace_EqualsStringConstructedCommand()
	{
		Assert.AreEqual(new Command(" file.save ", "Save"), CreatePaddedCommand());
	}

	[TestMethod]
	public void Constructor_WhitespaceOnlyCommandId_Throws()
	{
		Assert.ThrowsExactly<ArgumentException>(() => new Command(CommandId.Create("   "), CommandName.Create("Save")));
	}

	[TestMethod]
	public void Register_CommandIdWithWhitespace_CanBeFoundBoundAndUnregistered()
	{
		CommandRegistry registry = new();
		Assert.IsTrue(registry.RegisterCommand(CreatePaddedCommand()));

		Assert.IsTrue(registry.IsCommandRegistered("file.save"));
		Assert.IsTrue(registry.IsCommandRegistered(" file.save "));
		Assert.IsNotNull(registry.GetCommand("file.save"));

		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Profile");
		profiles.SetActiveProfile("p");
		KeybindingService service = new(registry, profiles);
		Assert.IsTrue(service.BindChord(" file.save ", Chord.Parse("Ctrl+S")));

		Assert.IsTrue(registry.UnregisterCommand(" file.save "));
		Assert.IsFalse(registry.IsCommandRegistered("file.save"));
	}
}
