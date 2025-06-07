// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

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
		using KeybindingManager manager = new(_testDataDirectory);

		// Act
		await manager.InitializeAsync().ConfigureAwait(false);

		// Assert
		Assert.IsTrue(Directory.Exists(_testDataDirectory));
	}

	[TestMethod]
	public async Task CreateDefaultProfile_CreatesAndSetsActiveProfile()
	{
		// Arrange
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		// Act
		Profile? profile = manager.CreateDefaultProfile();

		// Assert
		Assert.IsNotNull(profile);
		Assert.AreEqual("default", profile.Id);
		Assert.AreEqual("Default", profile.Name);

		Profile? activeProfile = manager.Profiles.GetActiveProfile();
		Assert.IsNotNull(activeProfile);
		Assert.AreEqual("default", activeProfile.Id);
	}

	[TestMethod]
	public async Task RegisterCommands_AddsCommandsToRegistry()
	{
		// Arrange
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Command[] commands =
		[
			new Command("copy", "Copy", "Copy content", "Edit"),
			new Command("paste", "Paste", "Paste content", "Edit")
		];

		// Act
		int registeredCount = manager.RegisterCommands(commands);

		// Assert
		Assert.AreEqual(2, registeredCount);
		Assert.IsTrue(manager.Commands.IsCommandRegistered("copy"));
		Assert.IsTrue(manager.Commands.IsCommandRegistered("paste"));
	}

	[TestMethod]
	public async Task SetKeybindings_WithActiveProfile_SetsKeybindings()
	{
		// Arrange
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		manager.CreateDefaultProfile();

		Command[] commands =
		[
			new Command("copy", "Copy", "Copy content", "Edit"),
			new Command("paste", "Paste", "Paste content", "Edit")
		];
		manager.RegisterCommands(commands);

		Dictionary<string, KeyCombination> keybindings = new()
		{
			["copy"] = new KeyCombination("C", ModifierKeys.Ctrl),
			["paste"] = new KeyCombination("V", ModifierKeys.Ctrl)
		};

		// Act
		int setCount = manager.SetKeybindings(keybindings);

		// Assert
		Assert.AreEqual(2, setCount);

		KeyCombination? copyKeybinding = manager.Keybindings.GetKeybinding("copy");
		Assert.IsNotNull(copyKeybinding);
		Assert.AreEqual("C", copyKeybinding.Key);
		Assert.AreEqual(ModifierKeys.Ctrl, copyKeybinding.Modifiers);
	}

	[TestMethod]
	public async Task FindCommandByKeybinding_ExistingKeybinding_ReturnsCommandId()
	{
		// Arrange
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		manager.CreateDefaultProfile();

		Command command = new("save", "Save", "Save document", "File");
		manager.Commands.RegisterCommand(command);

		KeyCombination keyCombination = new("S", ModifierKeys.Ctrl);
		manager.Keybindings.SetKeybinding("save", keyCombination);

		// Act
		string? commandId = manager.Keybindings.FindCommandByKeybinding(keyCombination);

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
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);

			Profile? profile = manager.CreateDefaultProfile();
			profileId = profile!.Id;

			Command command = new(commandId, "Test Command", "Test description", "Test");
			manager.Commands.RegisterCommand(command);

			KeyCombination keyCombination = new("T", ModifierKeys.Ctrl);
			manager.Keybindings.SetKeybinding(commandId, keyCombination);

			await manager.SaveAsync().ConfigureAwait(false);
		}

		// Load and verify data
		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);

			KeybindingSummary summary = manager.GetSummary();
			Assert.AreEqual(1, summary.TotalCommands);
			Assert.AreEqual(1, summary.TotalProfiles);
			Assert.AreEqual(profileId, summary.ActiveProfileId);
			Assert.AreEqual(1, summary.ActiveKeybindings);

			Command? command = manager.Commands.GetCommand(commandId);
			Assert.IsNotNull(command);
			Assert.AreEqual("Test Command", command.Name);

			KeyCombination? keybinding = manager.Keybindings.GetKeybinding(commandId);
			Assert.IsNotNull(keybinding);
			Assert.AreEqual("T", keybinding.Key);
			Assert.AreEqual(ModifierKeys.Ctrl, keybinding.Modifiers);
		}
	}

	[TestMethod]
	public async Task GetSummary_ReturnsCorrectInformation()
	{
		// Arrange
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		manager.CreateDefaultProfile();

		Command[] commands =
		[
			new Command("cmd1", "Command 1"),
			new Command("cmd2", "Command 2"),
			new Command("cmd3", "Command 3")
		];
		manager.RegisterCommands(commands);

		manager.Keybindings.SetKeybinding("cmd1", new KeyCombination("A", ModifierKeys.Ctrl));
		manager.Keybindings.SetKeybinding("cmd2", new KeyCombination("B", ModifierKeys.Ctrl));

		// Act
		KeybindingSummary summary = manager.GetSummary();

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
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Command command = new("test", "Test Command");
		manager.Commands.RegisterCommand(command);

		// Create two profiles
		Profile profile1 = new("profile1", "Profile 1");
		Profile profile2 = new("profile2", "Profile 2");
		manager.Profiles.CreateProfile(profile1);
		manager.Profiles.CreateProfile(profile2);

		// Set different keybindings for each profile
		KeyCombination key1 = new("A", ModifierKeys.Ctrl);
		KeyCombination key2 = new("B", ModifierKeys.Ctrl);

		manager.Keybindings.SetKeybinding("profile1", "test", key1);
		manager.Keybindings.SetKeybinding("profile2", "test", key2);

		// Act & Assert
		KeyCombination? keybinding1 = manager.Keybindings.GetKeybinding("profile1", "test");
		KeyCombination? keybinding2 = manager.Keybindings.GetKeybinding("profile2", "test");

		Assert.IsNotNull(keybinding1);
		Assert.IsNotNull(keybinding2);
		Assert.AreEqual("A", keybinding1.Key);
		Assert.AreEqual("B", keybinding2.Key);
		Assert.AreNotEqual(keybinding1, keybinding2);
	}
}
