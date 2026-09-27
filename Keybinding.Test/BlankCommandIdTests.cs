// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Services;

[TestClass]
public class BlankCommandIdTests
{
	private ProfileManager _profiles = null!;
	private KeybindingService _service = null!;

	[TestInitialize]
	public void Setup()
	{
		_profiles = new ProfileManager();
		_profiles.CreateProfile("p", "Profile");
		_service = new KeybindingService(new CommandRegistry(), _profiles);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("  ")]
	[DataRow(null)]
	public void ActiveProfileOverloads_BlankCommandId_MatchTheProfileIdOverloads(string? commandId)
	{
		Assert.IsNull(_service.GetChord(commandId!));
		Assert.IsFalse(_service.UnbindChord(commandId!));
		Assert.IsFalse(_service.HasChordBinding(commandId!));

		_profiles.SetActiveProfile("p");

		Assert.IsNull(_service.GetChord(commandId!), "GetChord should not depend on whether a profile is active.");
		Assert.IsFalse(_service.UnbindChord(commandId!), "UnbindChord should not depend on whether a profile is active.");
		Assert.IsFalse(_service.HasChordBinding(commandId!), "HasChordBinding should not depend on whether a profile is active.");

		Assert.AreEqual(_service.GetChord("p", commandId!), _service.GetChord(commandId!));
		Assert.AreEqual(_service.UnbindChord("p", commandId!), _service.UnbindChord(commandId!));
		Assert.AreEqual(_service.HasChordBinding("p", commandId!), _service.HasChordBinding(commandId!));
	}
}
