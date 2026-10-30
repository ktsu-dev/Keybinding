// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class DefaultProfileReactivationTests
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
	public async Task CreateDefaultProfile_AfterActiveProfileWasDeleted_ActivatesExistingDefaultOnNextLaunch()
	{
		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			manager.Profiles.CreateProfile("vim", "Vim");
			Assert.IsTrue(manager.Profiles.SetActiveProfile("vim"));
			Assert.IsTrue(manager.Profiles.DeleteProfile("vim"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			manager.RegisterCommands([new Command("file.save", "Save")]);

			Assert.AreEqual("default", manager.Profiles.GetActiveProfile()?.Id);
			Assert.IsTrue(manager.Keybindings.BindChord("file.save", Chord.Parse("Ctrl+S")));
			Assert.AreEqual("file.save", manager.Keybindings.ExecuteChord(Chord.Parse("Ctrl+S")));
		}
	}

	[TestMethod]
	public async Task CreateDefaultProfile_DefaultAndActiveProfileDeleted_ActivatesRemainingProfileOnNextLaunch()
	{
		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.CreateDefaultProfile();
			manager.Profiles.CreateProfile("vim", "Vim");
			manager.Profiles.CreateProfile("emacs", "Emacs");
			Assert.IsTrue(manager.Profiles.SetActiveProfile("vim"));
			Assert.IsTrue(manager.Profiles.DeleteProfile("default"));
			Assert.IsTrue(manager.Profiles.DeleteProfile("vim"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			Assert.IsNull(manager.CreateDefaultProfile());
			manager.RegisterCommands([new Command("file.save", "Save")]);

			Assert.AreEqual("emacs", manager.Profiles.GetActiveProfile()?.Id);
			Assert.IsFalse(manager.Profiles.ProfileExists("default"));
			Assert.IsTrue(manager.Keybindings.BindChord("file.save", Chord.Parse("Ctrl+S")));
			Assert.AreEqual("file.save", manager.Keybindings.ExecuteChord(Chord.Parse("Ctrl+S")));
		}
	}

	[TestMethod]
	public void CreateDefaultProfile_NoDefaultAndNoneActive_ActivatesFirstProfileByOrdinalId()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.Profiles.CreateProfile("vim", "Vim");
		manager.Profiles.CreateProfile("Emacs", "Emacs");
		manager.Profiles.CreateProfile("helix", "Helix");

		Assert.IsNull(manager.CreateDefaultProfile());
		Assert.AreEqual("Emacs", manager.Profiles.GetActiveProfile()?.Id);
		Assert.IsFalse(manager.Profiles.ProfileExists("default"));
	}

	[TestMethod]
	public void CreateDefaultProfile_NoDefaultAndNoneActiveWithDoNotActivate_LeavesNoActiveProfile()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.Profiles.CreateProfile("vim", "Vim");

		Assert.IsNull(manager.CreateDefaultProfile(activation: ProfileActivation.DoNotActivate));
		Assert.IsNull(manager.Profiles.GetActiveProfile());
	}

	[TestMethod]
	public void CreateDefaultProfile_ExistingDefaultWhileAnotherIsActive_LeavesActiveProfileAlone()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.CreateDefaultProfile();
		manager.Profiles.CreateProfile("vim", "Vim");
		Assert.IsTrue(manager.Profiles.SetActiveProfile("vim"));

		Assert.IsNull(manager.CreateDefaultProfile());
		Assert.AreEqual("vim", manager.Profiles.GetActiveProfile()?.Id);
	}

	[TestMethod]
	public void CreateDefaultProfile_ExistingDefaultWithDoNotActivate_LeavesNoActiveProfile()
	{
		using KeybindingManager manager = new(_testDataDirectory);
		manager.CreateDefaultProfile();
		manager.Profiles.ClearActiveProfile();

		Assert.IsNull(manager.CreateDefaultProfile(activation: ProfileActivation.DoNotActivate));
		Assert.IsNull(manager.Profiles.GetActiveProfile());
	}
}
