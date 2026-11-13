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

	[TestMethod]
	public async Task ProfileCreatedBeforeInitialize_TakesTheStoredNameAndDescription()
	{
		using (KeybindingManager manager = new(_dataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			Assert.IsTrue(manager.Profiles.RenameProfile("default", "My Keys", "Tuned for vim"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using (KeybindingManager manager = new(_dataDirectory))
		{
			manager.CreateDefaultProfile();
			await manager.InitializeAsync().ConfigureAwait(false);

			Profile profile = manager.Profiles.GetProfile("default")!;
			Assert.AreEqual("My Keys", profile.Name);
			Assert.AreEqual("Tuned for vim", profile.Description);
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Profile reloaded = await ReloadDefaultProfileAsync().ConfigureAwait(false);
		Assert.AreEqual("My Keys", reloaded.Name);
		Assert.AreEqual("Tuned for vim", reloaded.Description);
	}

	[TestMethod]
	public async Task ProfileCreatedBeforeInitialize_ResolvesDuplicateStoredChordsAsAFreshLoadDoes()
	{
		await File.WriteAllTextAsync(Path.Combine(_dataDirectory, "commands.json"),
			/*lang=json,strict*/ """[{"id":"z.cmd","name":"Z"},{"id":"a.cmd","name":"A"}]""").ConfigureAwait(false);
		await File.WriteAllTextAsync(Path.Combine(_dataDirectory, "profiles.json"),
			/*lang=json,strict*/ """[{"id":"default","name":"Default","chords":{"z.cmd":{"notes":["CTRL","S"]},"a.cmd":{"notes":["CTRL","S"]}}}]""").ConfigureAwait(false);
		await File.WriteAllTextAsync(Path.Combine(_dataDirectory, "active-profile.json"),
			/*lang=json,strict*/ """{"activeProfileId":"default"}""").ConfigureAwait(false);

		using (KeybindingManager manager = new(_dataDirectory))
		{
			manager.CreateDefaultProfile();
			await manager.InitializeAsync().ConfigureAwait(false);

			Assert.AreEqual(2, manager.Profiles.GetProfile("default")!.ChordCount);
			Assert.AreEqual("a.cmd", manager.Keybindings.FindCommandByChord(Chord.Parse("Ctrl+S")));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Profile reloaded = await ReloadDefaultProfileAsync().ConfigureAwait(false);
		Assert.AreEqual(2, reloaded.ChordCount);
		Assert.AreEqual(Chord.Parse("Ctrl+S"), reloaded.GetChord("z.cmd"));
		Assert.AreEqual(Chord.Parse("Ctrl+S"), reloaded.GetChord("a.cmd"));
	}

	[TestMethod]
	public async Task DefaultProfileCreatedBeforeInitialize_DoesNotBringBackADeletedDefault()
	{
		using (KeybindingManager manager = new(_dataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			manager.Profiles.CreateProfile("vim", "Vim");
			Assert.IsTrue(manager.Profiles.SetActiveProfile("vim"));
			Assert.IsTrue(manager.Profiles.DeleteProfile("default"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using (KeybindingManager manager = new(_dataDirectory))
		{
			manager.CreateDefaultProfile();
			await manager.InitializeAsync().ConfigureAwait(false);

			Assert.AreEqual("vim", string.Join(",", manager.Profiles.GetAllProfiles().Select(p => p.Id)));
			Assert.AreEqual("vim", manager.Profiles.GetActiveProfile()?.Id);
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using (KeybindingManager manager = new(_dataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			Assert.AreEqual("vim", string.Join(",", manager.Profiles.GetAllProfiles().Select(p => p.Id)));
		}
	}

	[TestMethod]
	public async Task DefaultProfileCreatedBeforeInitialize_ActivatesAStoredProfileWhenItsPlaceholderIsDropped()
	{
		using (KeybindingManager manager = new(_dataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.Profiles.CreateProfile("vim", "Vim");
			manager.Profiles.CreateProfile("emacs", "Emacs");
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using KeybindingManager reloaded = new(_dataDirectory);
		reloaded.CreateDefaultProfile();
		await reloaded.InitializeAsync().ConfigureAwait(false);

		Assert.IsNull(reloaded.Profiles.GetProfile("default"));
		Assert.AreEqual("emacs", reloaded.Profiles.GetActiveProfile()?.Id);
	}

	[TestMethod]
	public async Task DefaultProfileCreatedBeforeInitialize_IsKeptOnceTheAppHasBoundIt()
	{
		using (KeybindingManager manager = new(_dataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.Profiles.CreateProfile("vim", "Vim");
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using KeybindingManager reloaded = new(_dataDirectory);
		Profile seeded = reloaded.CreateDefaultProfile()!;
		seeded.SetChord("file.save", Chord.Parse("Ctrl+S"));
		await reloaded.InitializeAsync().ConfigureAwait(false);

		Assert.AreSame(seeded, reloaded.Profiles.GetProfile("default"));
	}
}
