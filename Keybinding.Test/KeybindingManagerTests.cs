// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Test;

[TestClass]
public class KeybindingManagerTests
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
	public async Task InitializeAsync_CreatesDataDirectory()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);

		// Act
		await manager.InitializeAsync();

		// Assert
		Assert.IsTrue(Directory.Exists(_testDataDirectory));
	}

	[TestMethod]
	public async Task CreateDefaultProfile_CreatesAndSetsActiveProfile()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);
		await manager.InitializeAsync();

		// Act
		var profile = manager.CreateDefaultProfile();

		// Assert
		Assert.IsNotNull(profile);
		Assert.AreEqual("default", profile.Id);
		Assert.AreEqual("Default", profile.Name);

		var activeProfile = manager.Profiles.GetActiveProfile();
		Assert.IsNotNull(activeProfile);
		Assert.AreEqual("default", activeProfile.Id);
	}

	[TestMethod]
	public async Task RegisterCommands_AddsCommandsToRegistry()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);
		await manager.InitializeAsync();

		var commands = new[]
		{
			new Command("copy", "Copy", "Copy content", "Edit"),
			new Command("paste", "Paste", "Paste content", "Edit")
		};

		// Act
		var registeredCount = manager.RegisterCommands(commands);

		// Assert
		Assert.AreEqual(2, registeredCount);
		Assert.IsTrue(manager.Commands.IsCommandRegistered("copy"));
		Assert.IsTrue(manager.Commands.IsCommandRegistered("paste"));
	}

	[TestMethod]
	public async Task SetKeybindings_WithActiveProfile_SetsKeybindings()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);
		await manager.InitializeAsync();
		manager.CreateDefaultProfile();

		var commands = new[]
		{
			new Command("copy", "Copy", "Copy content", "Edit"),
			new Command("paste", "Paste", "Paste content", "Edit")
		};
		manager.RegisterCommands(commands);

		var keybindings = new Dictionary<string, KeyCombination>
		{
			["copy"] = new KeyCombination("C", ModifierKeys.Ctrl),
			["paste"] = new KeyCombination("V", ModifierKeys.Ctrl)
		};

		// Act
		var setCount = manager.SetKeybindings(keybindings);

		// Assert
		Assert.AreEqual(2, setCount);

		var copyKeybinding = manager.Keybindings.GetKeybinding("copy");
		Assert.IsNotNull(copyKeybinding);
		Assert.AreEqual("C", copyKeybinding.Key);
		Assert.AreEqual(ModifierKeys.Ctrl, copyKeybinding.Modifiers);
	}

	[TestMethod]
	public async Task FindCommandByKeybinding_ExistingKeybinding_ReturnsCommandId()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);
		await manager.InitializeAsync();
		manager.CreateDefaultProfile();

		var command = new Command("save", "Save", "Save document", "File");
		manager.Commands.RegisterCommand(command);

		var keyCombination = new KeyCombination("S", ModifierKeys.Ctrl);
		manager.Keybindings.SetKeybinding("save", keyCombination);

		// Act
		var commandId = manager.Keybindings.FindCommandByKeybinding(keyCombination);

		// Assert
		Assert.AreEqual("save", commandId);
	}

	[TestMethod]
	public async Task SaveAndLoadData_PersistsDataCorrectly()
	{
		// Arrange
		string profileId;
		string commandId = "test-command";

		// Create and save data
		{
			using var manager = new KeybindingManager(_testDataDirectory);
			await manager.InitializeAsync();

			var profile = manager.CreateDefaultProfile();
			profileId = profile!.Id;

			var command = new Command(commandId, "Test Command", "Test description", "Test");
			manager.Commands.RegisterCommand(command);

			var keyCombination = new KeyCombination("T", ModifierKeys.Ctrl);
			manager.Keybindings.SetKeybinding(commandId, keyCombination);

			await manager.SaveAsync();
		}

		// Load and verify data
		{
			using var manager = new KeybindingManager(_testDataDirectory);
			await manager.InitializeAsync();

			var summary = manager.GetSummary();
			Assert.AreEqual(1, summary.TotalCommands);
			Assert.AreEqual(1, summary.TotalProfiles);
			Assert.AreEqual(profileId, summary.ActiveProfileId);
			Assert.AreEqual(1, summary.ActiveKeybindings);

			var command = manager.Commands.GetCommand(commandId);
			Assert.IsNotNull(command);
			Assert.AreEqual("Test Command", command.Name);

			var keybinding = manager.Keybindings.GetKeybinding(commandId);
			Assert.IsNotNull(keybinding);
			Assert.AreEqual("T", keybinding.Key);
			Assert.AreEqual(ModifierKeys.Ctrl, keybinding.Modifiers);
		}
	}

	[TestMethod]
	public async Task GetSummary_ReturnsCorrectInformation()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);
		await manager.InitializeAsync();
		manager.CreateDefaultProfile();

		var commands = new[]
		{
			new Command("cmd1", "Command 1"),
			new Command("cmd2", "Command 2"),
			new Command("cmd3", "Command 3")
		};
		manager.RegisterCommands(commands);

		manager.Keybindings.SetKeybinding("cmd1", new KeyCombination("A", ModifierKeys.Ctrl));
		manager.Keybindings.SetKeybinding("cmd2", new KeyCombination("B", ModifierKeys.Ctrl));

		// Act
		var summary = manager.GetSummary();

		// Assert
		Assert.AreEqual(3, summary.TotalCommands);
		Assert.AreEqual(1, summary.TotalProfiles);
		Assert.AreEqual("default", summary.ActiveProfileId);
		Assert.AreEqual("Default", summary.ActiveProfileName);
		Assert.AreEqual(2, summary.ActiveKeybindings);
	}

	[TestMethod]
	public async Task MultipleProfiles_WorkIndependently()
	{
		// Arrange
		using var manager = new KeybindingManager(_testDataDirectory);
		await manager.InitializeAsync();

		var command = new Command("test", "Test Command");
		manager.Commands.RegisterCommand(command);

		// Create two profiles
		var profile1 = new Profile("profile1", "Profile 1");
		var profile2 = new Profile("profile2", "Profile 2");
		manager.Profiles.CreateProfile(profile1);
		manager.Profiles.CreateProfile(profile2);

		// Set different keybindings for each profile
		var key1 = new KeyCombination("A", ModifierKeys.Ctrl);
		var key2 = new KeyCombination("B", ModifierKeys.Ctrl);

		manager.Keybindings.SetKeybinding("profile1", "test", key1);
		manager.Keybindings.SetKeybinding("profile2", "test", key2);

		// Act & Assert
		var keybinding1 = manager.Keybindings.GetKeybinding("profile1", "test");
		var keybinding2 = manager.Keybindings.GetKeybinding("profile2", "test");

		Assert.IsNotNull(keybinding1);
		Assert.IsNotNull(keybinding2);
		Assert.AreEqual("A", keybinding1.Key);
		Assert.AreEqual("B", keybinding2.Key);
		Assert.AreNotEqual(keybinding1, keybinding2);
	}
}
