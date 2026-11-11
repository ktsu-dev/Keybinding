// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class NullChordArgumentTests
{
	private ProfileManager _profiles = null!;
	private CommandRegistry _commands = null!;
	private KeybindingService _service = null!;

	[TestInitialize]
	public void Setup()
	{
		_profiles = new ProfileManager();
		_commands = new CommandRegistry();
		_commands.RegisterCommand(new Command("save", "Save"));
		_service = new KeybindingService(_commands, _profiles);
	}

	[TestMethod]
	public void ActiveProfileOverloads_NullChord_ThrowWithNoActiveProfile()
	{
		Assert.IsNull(_service.ActiveProfile);

		Assert.ThrowsExactly<ArgumentNullException>(() => _service.BindChord("save", null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.ExecuteChord(null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.FindCommandByChord(null!));
	}

	[TestMethod]
	public void ActiveProfileOverloads_NullChord_ThrowWithAnActiveProfile()
	{
		_profiles.CreateProfile("default", "Default");
		_profiles.SetActiveProfile("default");
		Assert.IsNotNull(_service.ActiveProfile);

		Assert.ThrowsExactly<ArgumentNullException>(() => _service.BindChord("save", null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.ExecuteChord(null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.FindCommandByChord(null!));
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("  ")]
	[DataRow(null)]
	[DataRow("missing")]
	public void ProfileIdOverloads_NullChord_ThrowWhateverTheProfileId(string? profileId)
	{
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.BindChord(profileId!, "save", null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.ExecuteChord(profileId!, null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.FindCommandByChord(profileId!, null!));
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("  ")]
	[DataRow(null)]
	public void BindChord_NullChord_ThrowsWhateverTheCommandId(string? commandId)
	{
		_profiles.CreateProfile("default", "Default");
		_profiles.SetActiveProfile("default");

		Assert.ThrowsExactly<ArgumentNullException>(() => _service.BindChord(commandId!, null!));
		Assert.ThrowsExactly<ArgumentNullException>(() => _service.BindChord(_service.ActiveProfile!.Id, commandId!, null!));
	}
}
