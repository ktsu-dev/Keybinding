// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ExecuteChordTests
{
	private CommandRegistry _registry = null!;
	private KeybindingService _service = null!;

	[TestInitialize]
	public void Setup()
	{
		_registry = new CommandRegistry();
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Profile");
		profiles.SetActiveProfile("p");
		_service = new KeybindingService(_registry, profiles);

		_registry.RegisterCommand(new Command("a", "A"));
		_registry.RegisterCommand(new Command("b", "B"));
	}

	[TestMethod]
	public void ExecuteChord_FirstBindingUnregistered_RunsTheRegisteredCommandSharingTheChord()
	{
		Chord chord = Chord.Parse("Ctrl+S");
		Assert.IsTrue(_service.BindChord("a", chord));
		Assert.IsTrue(_service.BindChord("b", chord));

		_registry.UnregisterCommand("a");

		Assert.AreEqual("b", _service.ExecuteChord(chord), "The only registered command bound to the chord should run.");
		Assert.AreEqual("b", _service.ExecuteChord("p", chord));
	}

	[TestMethod]
	public void ExecuteChord_BothRegistered_RunsTheFirstBinding()
	{
		Chord chord = Chord.Parse("Ctrl+S");
		_service.BindChord("a", chord);
		_service.BindChord("b", chord);

		Assert.AreEqual(_service.FindCommandByChord(chord), _service.ExecuteChord(chord));
	}

	[TestMethod]
	public void ExecuteChord_OnlyBindingUnregistered_ReturnsNull()
	{
		Chord chord = Chord.Parse("Ctrl+S");
		_service.BindChord("a", chord);

		_registry.UnregisterCommand("a");

		Assert.IsNull(_service.ExecuteChord(chord));
	}

	[TestMethod]
	public void ExecuteChord_UnknownProfile_ReturnsNull()
	{
		Chord chord = Chord.Parse("Ctrl+S");
		_service.BindChord("a", chord);

		Assert.IsNull(_service.ExecuteChord("missing", chord));
		Assert.IsNull(_service.ExecuteChord(" ", chord));
	}
}
