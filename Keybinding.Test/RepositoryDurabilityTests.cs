// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class RepositoryDurabilityTests
{
	private string _testDataDirectory = null!;

	[TestInitialize]
	public void Setup()
	{
		_testDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testDataDirectory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_testDataDirectory))
		{
			Directory.Delete(_testDataDirectory, recursive: true);
		}
	}

	private string PathOf(string fileName) => Path.Combine(_testDataDirectory, fileName);

	[TestMethod]
	public async Task OneWrongTypedProfile_SkipsOnlyThatProfile()
	{
		await File.WriteAllTextAsync(PathOf("profiles.json"), """
			[
			  {"id":"a","name":"A","chords":{"file.save":{"notes":["CTRL","S"]}}},
			  {"id":"b","name":5}
			]
			""").ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Assert.AreEqual("a", string.Join(',', manager.Profiles.GetAllProfiles().Select(p => p.Id)));
		Assert.AreEqual(Chord.Parse("Ctrl+S"), manager.Profiles.GetProfile("a")!.GetChord("file.save"));
	}

	[TestMethod]
	public async Task OneWrongTypedChord_SkipsOnlyThatChord()
	{
		await File.WriteAllTextAsync(PathOf("profiles.json"), """
			[{"id":"a","name":"A","chords":{"file.save":{"notes":["CTRL","S"]},"file.open":{"notes":"CTRL"},"file.new":{"notes":[5]}}}]
			""").ConfigureAwait(false);

		JsonKeybindingRepository repository = new(_testDataDirectory);
		Profile profile = (await repository.LoadAllProfilesAsync().ConfigureAwait(false)).Single();

		Assert.AreEqual(1, profile.ChordCount);
		Assert.IsNotNull(profile.GetChord("file.save"));
	}

	[TestMethod]
	public async Task OneWrongTypedCommand_SkipsOnlyThatCommand()
	{
		await File.WriteAllTextAsync(PathOf("commands.json"), """
			[{"id":"file.save","name":"Save"},{"id":"file.open","name":"Open","category":5}]
			""").ConfigureAwait(false);

		JsonKeybindingRepository repository = new(_testDataDirectory);
		IReadOnlyCollection<Command> commands = await repository.LoadCommandsAsync().ConfigureAwait(false);

		Assert.AreEqual("file.save", string.Join(',', commands.Select(c => c.Id.ToString())));
	}

	[TestMethod]
	public async Task TornProfilesFile_IsKeptAsideBeforeTheNextSave()
	{
		using (KeybindingManager manager = new(_testDataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.Commands.RegisterCommand(new Command("file.save", "Save"));
			for (int i = 0; i < 5; i++)
			{
				manager.Profiles.CreateProfile("p" + i, "P" + i).SetChord("file.save", Chord.Parse("Ctrl+S"));
			}

			await manager.SaveAsync().ConfigureAwait(false);
		}

		string full = await File.ReadAllTextAsync(PathOf("profiles.json")).ConfigureAwait(false);
		string torn = full[..(full.Length / 2)];
		await File.WriteAllTextAsync(PathOf("profiles.json"), torn).ConfigureAwait(false);

		using (KeybindingManager manager = new(_testDataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			Assert.IsEmpty(manager.Profiles.GetAllProfiles(), "A file that does not parse still loads as empty.");
			manager.CreateDefaultProfile();
			await manager.SaveAsync().ConfigureAwait(false);
		}

		string[] sideFiles = Directory.GetFiles(_testDataDirectory, "profiles.json.corrupt-*");
		Assert.HasCount(1, sideFiles);
		Assert.AreEqual(torn, await File.ReadAllTextAsync(sideFiles[0]).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task Saves_LeaveNoTemporaryFilesBehind()
	{
		JsonKeybindingRepository repository = new(_testDataDirectory);
		await repository.SaveProfileAsync(new Profile("a", "A")).ConfigureAwait(false);
		await repository.SaveCommandsAsync([new Command("file.save", "Save")]).ConfigureAwait(false);
		await repository.SaveActiveProfileAsync("a").ConfigureAwait(false);
		await repository.DeleteProfileAsync("a").ConfigureAwait(false);

		Assert.IsEmpty(Directory.GetFiles(_testDataDirectory, "*.tmp"));
		Assert.AreEqual("a", await repository.LoadActiveProfileAsync().ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ParallelSaves_KeepEveryProfile()
	{
		JsonKeybindingRepository repository = new(_testDataDirectory);
		JsonKeybindingRepository second = new(_testDataDirectory);

		await Task.WhenAll(Enumerable.Range(0, 16).Select(i =>
			(i % 2 == 0 ? repository : second).SaveProfileAsync(new Profile("p" + i, "P" + i)))).ConfigureAwait(false);

		IReadOnlyCollection<Profile> profiles = await repository.LoadAllProfilesAsync().ConfigureAwait(false);
		Assert.HasCount(16, profiles);
	}

	[TestMethod]
	public async Task ParallelManagerSaves_KeepEveryProfile()
	{
		using (KeybindingManager manager = new(_testDataDirectory))
		{
			for (int i = 0; i < 5; i++)
			{
				manager.Profiles.CreateProfile("p" + i, "P" + i);
			}

			await Task.WhenAll(manager.SaveAsync(), manager.SaveAsync(), manager.SaveAsync()).ConfigureAwait(false);
		}

		using KeybindingManager reloaded = new(_testDataDirectory);
		await reloaded.InitializeAsync().ConfigureAwait(false);
		Assert.HasCount(5, reloaded.Profiles.GetAllProfiles());
	}
}
