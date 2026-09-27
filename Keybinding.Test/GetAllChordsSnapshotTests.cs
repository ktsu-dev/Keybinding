// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class GetAllChordsSnapshotTests
{
	private KeybindingService _service = null!;
	private ProfileManager _profiles = null!;

	[TestInitialize]
	public void Setup()
	{
		CommandRegistry registry = new();
		_profiles = new ProfileManager();
		_profiles.CreateProfile("p", "Profile");
		_profiles.SetActiveProfile("p");
		_service = new KeybindingService(registry, _profiles);

		registry.RegisterCommand(new Command("a", "A"));
		registry.RegisterCommand(new Command("b", "B"));
	}

	[TestMethod]
	public void GetAllChords_LaterBindAndUnbind_DoNotChangeTheReturnedDictionary()
	{
		_service.BindChord("a", Chord.Parse("Ctrl+A"));

		IReadOnlyDictionary<string, Chord> active = _service.GetAllChords();
		IReadOnlyDictionary<string, Chord> byId = _service.GetAllChords("p");

		_service.BindChord("b", Chord.Parse("Ctrl+B"));
		_service.UnbindChord("a");

		Assert.HasCount(1, active, "The active-profile snapshot should not see later bindings.");
		Assert.IsTrue(active.ContainsKey("a"), "The active-profile snapshot should not lose a later-unbound command.");
		Assert.HasCount(1, byId);
		Assert.IsTrue(byId.ContainsKey("a"));
	}

	[TestMethod]
	public void GetAllChords_BindingWhileEnumerating_DoesNotThrow()
	{
		_service.BindChord("a", Chord.Parse("Ctrl+A"));

		foreach (KeyValuePair<string, Chord> binding in _service.GetAllChords())
		{
			if (!_service.HasChordBinding("b"))
			{
				_service.BindChord("b", binding.Value);
			}
		}

		Assert.IsTrue(_service.HasChordBinding("b"));
	}

	[TestMethod]
	public void BoundCommands_LaterBind_DoesNotChangeTheReturnedCollection()
	{
		_service.BindChord("a", Chord.Parse("Ctrl+A"));
		Profile profile = _profiles.GetProfile("p")!;

		IReadOnlyCollection<string> bound = profile.BoundCommands;
		_service.BindChord("b", Chord.Parse("Ctrl+B"));

		Assert.HasCount(1, bound);
	}
}
