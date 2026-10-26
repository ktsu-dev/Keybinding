// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ManagerStartupTests
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

	[TestMethod]
	public async Task CreateDefaultProfile_AfterUserDeletedDefault_KeepsTheirProfileActiveAndDefaultGone()
	{
		using (KeybindingManager manager = new(_testDataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			manager.Profiles.CreateProfile("vim", "Vim");
			manager.Profiles.SetActiveProfile("vim");
			manager.Profiles.DeleteProfile("default");
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using (KeybindingManager manager = new(_testDataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			Assert.IsNull(manager.CreateDefaultProfile());

			Assert.AreEqual("vim", manager.Profiles.GetActiveProfile()?.Id);
			Assert.IsFalse(manager.Profiles.ProfileExists("default"));
		}
	}

	[TestMethod]
	public void CreateDefaultProfile_NoProfiles_CreatesAndActivates()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		Assert.IsNotNull(manager.CreateDefaultProfile());
		Assert.AreEqual("default", manager.Profiles.GetActiveProfile()?.Id);
	}

	[TestMethod]
	[DataRow(" ")]
	[DataRow("\\t")]
	public async Task InitializeAsync_BlankStoredActiveProfile_LoadsWithNoActiveProfile(string escapedActiveProfileId)
	{
		using (KeybindingManager manager = new(_testDataDirectory))
		{
			manager.CreateDefaultProfile();
			await manager.SaveAsync().ConfigureAwait(false);
		}

		await File.WriteAllTextAsync(
			Path.Combine(_testDataDirectory, "active-profile.json"),
			$"{{\"activeProfileId\": \"{escapedActiveProfileId}\"}}").ConfigureAwait(false);

		using (KeybindingManager manager = new(_testDataDirectory))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			Assert.IsNull(manager.Profiles.GetActiveProfile());
			Assert.IsTrue(manager.Profiles.ProfileExists("default"));
		}
	}

	[TestMethod]
	public async Task InitializeAsync_RepositoryReturnsBlankActiveProfile_DoesNotThrow()
	{
		using KeybindingManager manager = new(new CommandRegistry(), new ProfileManager(), new BlankActiveProfileRepository(_testDataDirectory));
		await manager.InitializeAsync().ConfigureAwait(false);
		Assert.IsNull(manager.Profiles.GetActiveProfile());
	}

	[TestMethod]
	public void SetChords_ActiveProfileChangesMidBatch_BindsEveryEntryIntoTheFirstProfile()
	{
		SwitchingProfileManager profiles = new();
		profiles.CreateProfile("a", "A");
		profiles.CreateProfile("b", "B");
		profiles.SetActiveProfile("a");
		using KeybindingManager manager = new(new CommandRegistry(), profiles, new JsonKeybindingRepository(_testDataDirectory));
		manager.Commands.RegisterCommand(new Command("x", "X"));
		manager.Commands.RegisterCommand(new Command("y", "Y"));

		profiles.SwitchToAfterNextActiveRead = "b";
		int count = manager.SetChords(new Dictionary<string, Chord> { ["x"] = Chord.Parse("Ctrl+X"), ["y"] = Chord.Parse("Ctrl+Y") });

		Assert.AreEqual(2, count);
		Assert.AreEqual(2, profiles.GetProfile("a")!.ChordCount);
		Assert.AreEqual(0, profiles.GetProfile("b")!.ChordCount);
	}

	private sealed class BlankActiveProfileRepository(string directory) : IKeybindingRepository
	{
		private readonly JsonKeybindingRepository _inner = new(directory);

		public Task SaveProfileAsync(Profile profile) => _inner.SaveProfileAsync(profile);
		public Task<Profile?> LoadProfileAsync(string profileId) => _inner.LoadProfileAsync(profileId);
		public Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync() => _inner.LoadAllProfilesAsync();
		public Task DeleteProfileAsync(string profileId) => _inner.DeleteProfileAsync(profileId);
		public Task SaveCommandsAsync(IEnumerable<Command> commands) => _inner.SaveCommandsAsync(commands);
		public Task<IReadOnlyCollection<Command>> LoadCommandsAsync() => _inner.LoadCommandsAsync();
		public Task SaveActiveProfileAsync(string? profileId) => _inner.SaveActiveProfileAsync(profileId);
		public Task<string?> LoadActiveProfileAsync() => Task.FromResult<string?>("  ");
		public Task<bool> IsInitializedAsync() => _inner.IsInitializedAsync();
		public Task InitializeAsync() => _inner.InitializeAsync();
	}

	// Changes the active profile straight after the next read of it, standing in for another thread calling
	// SetActiveProfile while a batch is under way.
	private sealed class SwitchingProfileManager : IProfileManager
	{
		private readonly ProfileManager _inner = new();

		public string? SwitchToAfterNextActiveRead { get; set; }

		public Profile? GetActiveProfile()
		{
			Profile? active = _inner.GetActiveProfile();
			if (SwitchToAfterNextActiveRead is { } next)
			{
				SwitchToAfterNextActiveRead = null;
				_inner.SetActiveProfile(next);
			}

			return active;
		}

		public bool CreateProfile(Profile profile) => _inner.CreateProfile(profile);
		public Profile CreateProfile(string id, string name, string? description = null) => _inner.CreateProfile(id, name, description);
		public bool DeleteProfile(string profileId) => _inner.DeleteProfile(profileId);
		public Profile? GetProfile(string profileId) => _inner.GetProfile(profileId);
		public IReadOnlyCollection<Profile> GetAllProfiles() => _inner.GetAllProfiles();
		public bool SetActiveProfile(string profileId) => _inner.SetActiveProfile(profileId);
		public void ClearActiveProfile() => _inner.ClearActiveProfile();
		public bool ProfileExists(string profileId) => _inner.ProfileExists(profileId);
		public Profile? DuplicateProfile(string sourceProfileId, string newProfileId, string newProfileName, string? newDescription = null) =>
			_inner.DuplicateProfile(sourceProfileId, newProfileId, newProfileName, newDescription);
		public bool RenameProfile(string profileId, string newName, string? newDescription = null) => _inner.RenameProfile(profileId, newName, newDescription);
	}
}
