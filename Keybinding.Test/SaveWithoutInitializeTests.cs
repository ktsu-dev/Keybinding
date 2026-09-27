// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class SaveWithoutInitializeTests
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

	private async Task<string> LoadProfileIdsAsync()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		return string.Join(',', manager.Profiles.GetAllProfiles().Select(p => p.Id).Order(StringComparer.Ordinal));
	}

	[TestMethod]
	public async Task SaveAsync_WithoutInitialize_KeepsStoredProfiles()
	{
		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			manager.Profiles.CreateProfile("vim", "Vim");
			await manager.SaveAsync().ConfigureAwait(false);
		}

		{
			using KeybindingManager manager = new(_testDataDirectory);
			manager.CreateDefaultProfile("other", "Other");
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Assert.AreEqual("default,other,vim", await LoadProfileIdsAsync().ConfigureAwait(false));
	}

	[TestMethod]
	public async Task DeleteProfile_CreatedAndSavedInSameManager_StaysDeleted()
	{
		{
			using KeybindingManager manager = new(_testDataDirectory);
			manager.Profiles.CreateProfile("default", "Default");
			manager.Profiles.CreateProfile("vim", "Vim");
			await manager.SaveAsync().ConfigureAwait(false);

			Assert.IsTrue(manager.Profiles.DeleteProfile("vim"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Assert.AreEqual("default", await LoadProfileIdsAsync().ConfigureAwait(false));
	}

	[TestMethod]
	public async Task DeleteProfile_ThenRecreate_IsSavedAgain()
	{
		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.Profiles.CreateProfile("vim", "Vim");
			await manager.SaveAsync().ConfigureAwait(false);

			Assert.IsTrue(manager.Profiles.DeleteProfile("vim"));
			await manager.SaveAsync().ConfigureAwait(false);

			manager.Profiles.CreateProfile(new Profile("vim", "Vim again"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Assert.AreEqual("vim", await LoadProfileIdsAsync().ConfigureAwait(false));
	}
}
