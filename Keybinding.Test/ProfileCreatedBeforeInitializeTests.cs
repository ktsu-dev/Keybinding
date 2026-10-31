// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// A profile created before <see cref="KeybindingManager.InitializeAsync"/> takes in the stored bindings of the
/// profile with the same id, instead of silently discarding them (ktsu-dev/Keybinding#206).
/// </summary>
[TestClass]
public class ProfileCreatedBeforeInitializeTests
{
	private string _dataDirectory = null!;

	[TestInitialize]
	public void Setup()
	{
		_dataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_dataDirectory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_dataDirectory))
		{
			Directory.Delete(_dataDirectory, recursive: true);
		}
	}

	private async Task StoreDefaultProfileAsync(params (string CommandId, string Chord)[] bindings)
	{
		using KeybindingManager manager = new(_dataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		manager.CreateDefaultProfile();
		foreach ((string commandId, string chord) in bindings)
		{
			manager.Commands.RegisterCommand(new Command(commandId, commandId));
			Assert.IsTrue(manager.Keybindings.BindChord(commandId, Chord.Parse(chord)));
		}

		await manager.SaveAsync().ConfigureAwait(false);
	}

	private async Task<Profile> ReloadDefaultProfileAsync()
	{
		using KeybindingManager manager = new(_dataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		return manager.Profiles.GetProfile("default")!;
	}

	[TestMethod]
	public async Task ProfileCreatedBeforeInitialize_KeepsTheStoredBindings()
	{
		await StoreDefaultProfileAsync(("file.save", "Ctrl+S")).ConfigureAwait(false);

		using (KeybindingManager manager = new(_dataDirectory))
		{
			manager.CreateDefaultProfile();
			await manager.InitializeAsync().ConfigureAwait(false);

			Assert.AreEqual(Chord.Parse("Ctrl+S"), manager.Profiles.GetProfile("default")!.GetChord("file.save"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Profile reloaded = await ReloadDefaultProfileAsync().ConfigureAwait(false);
		Assert.AreEqual(1, reloaded.ChordCount);
		Assert.AreEqual(Chord.Parse("Ctrl+S"), reloaded.GetChord("file.save"));
	}

	[TestMethod]
	public async Task ProfileCreatedBeforeInitialize_KeepsItsOwnBindingsOverStoredOnes()
	{
		await StoreDefaultProfileAsync(("file.save", "Ctrl+S"), ("file.open", "Ctrl+O"), ("edit.copy", "Ctrl+C")).ConfigureAwait(false);

		using KeybindingManager manager = new(_dataDirectory);
		Profile seeded = manager.CreateDefaultProfile()!;
		seeded.SetChord("file.save", Chord.Parse("Ctrl+Shift+S"));
		seeded.SetChord("edit.paste", Chord.Parse("Ctrl+O"));
		await manager.InitializeAsync().ConfigureAwait(false);

		Profile profile = manager.Profiles.GetProfile("default")!;
		Assert.AreSame(seeded, profile);

		// The in-memory binding wins for the same command
		Assert.AreEqual(Chord.Parse("Ctrl+Shift+S"), profile.GetChord("file.save"));

		// A stored binding whose chord the in-memory profile already uses is not added, so no chord runs two commands
		Assert.IsNull(profile.GetChord("file.open"));
		Assert.AreEqual(Chord.Parse("Ctrl+O"), profile.GetChord("edit.paste"));

		// Every other stored binding is merged in
		Assert.AreEqual(Chord.Parse("Ctrl+C"), profile.GetChord("edit.copy"));
		Assert.AreEqual(3, profile.ChordCount);
	}
}
