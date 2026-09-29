// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class InvalidStoredProfileEntryTests
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

	private async Task WriteProfilesAsync(string json) =>
		await File.WriteAllTextAsync(Path.Combine(_testDataDirectory, "profiles.json"), json).ConfigureAwait(false);

	[TestMethod]
	public async Task InitializeAsync_CorruptProfilesFile_LoadsNoProfiles()
	{
		await WriteProfilesAsync("{not json").ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Assert.IsEmpty(manager.Profiles.GetAllProfiles());
	}

	[TestMethod]
	public async Task InitializeAsync_ChordWithNoNotes_SkipsOnlyThatBinding()
	{
		await WriteProfilesAsync("""
			[{"id":"default","name":"Default","chords":{"file.save":{"notes":[]},"file.open":{"notes":["CTRL","O"]}}}]
			""").ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Profile? profile = manager.Profiles.GetProfile("default");
		Assert.IsNotNull(profile);
		Assert.IsNull(profile.GetChord("file.save"));
		Assert.IsNotNull(profile.GetChord("file.open"));
	}

	[TestMethod]
	public async Task InitializeAsync_BlankNote_SkipsOnlyThatBinding()
	{
		await WriteProfilesAsync("""
			[{"id":"default","name":"Default","chords":{"file.save":{"notes":["CTRL"," "]},"file.open":{"notes":["CTRL","O"]}}}]
			""").ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Profile? profile = manager.Profiles.GetProfile("default");
		Assert.IsNotNull(profile);
		Assert.IsNull(profile.GetChord("file.save"));
		Assert.IsNotNull(profile.GetChord("file.open"));
	}

	[TestMethod]
	public async Task InitializeAsync_ProfileWithBlankIdOrName_SkipsOnlyThatProfile()
	{
		await WriteProfilesAsync("""
			[{"id":"","name":"No Id"},{"id":"noname","name":" "},{"id":"default","name":"Default"}]
			""").ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Assert.AreEqual("default", string.Join(',', manager.Profiles.GetAllProfiles().Select(p => p.Id)));
	}

	[TestMethod]
	public async Task SaveAsync_AfterSkippingInvalidEntries_KeepsTheValidOnes()
	{
		await WriteProfilesAsync("""
			[{"id":"default","name":"Default","chords":{"file.save":{"notes":[]},"file.open":{"notes":["CTRL","O"]}}}]
			""").ConfigureAwait(false);

		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.Profiles.CreateProfile("vim", "Vim");
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using KeybindingManager reloaded = new(_testDataDirectory);
		await reloaded.InitializeAsync().ConfigureAwait(false);

		Assert.AreEqual("default,vim", string.Join(',', reloaded.Profiles.GetAllProfiles().Select(p => p.Id).Order(StringComparer.Ordinal)));
		Assert.IsNotNull(reloaded.Profiles.GetProfile("default")?.GetChord("file.open"));
	}
}
