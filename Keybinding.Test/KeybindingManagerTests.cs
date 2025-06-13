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

		Dictionary<string, Chord> chords = new()
		{
			["copy"] = manager.Keybindings.ParseChord("Ctrl+C"),
			["paste"] = manager.Keybindings.ParseChord("Ctrl+V")
		};

		// Act
		int setCount = manager.SetChords(chords);

		// Assert
		Assert.AreEqual(2, setCount);

		Chord? copyChord = manager.Keybindings.GetChord("copy");
		Assert.IsNotNull(copyChord);
		Assert.AreEqual("Ctrl+C", copyChord.ToString());
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

		Chord chord = manager.Keybindings.ParseChord("Ctrl+S");
		manager.Keybindings.BindChord("save", chord);

		// Act
		string? commandId = manager.Keybindings.FindCommandByChord(chord);

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

			Chord chord = manager.Keybindings.ParseChord("Ctrl+T");
			manager.Keybindings.BindChord(commandId, chord);

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

			Chord? chord = manager.Keybindings.GetChord(commandId);
			Assert.IsNotNull(chord);
			Assert.AreEqual("Ctrl+T", chord.ToString());
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

		manager.Keybindings.BindChord("cmd1", manager.Keybindings.ParseChord("Ctrl+A"));
		manager.Keybindings.BindChord("cmd2", manager.Keybindings.ParseChord("Ctrl+B"));

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

		// Set different chord bindings for each profile
		Chord chord1 = manager.Keybindings.ParseChord("Ctrl+A");
		Chord chord2 = manager.Keybindings.ParseChord("Ctrl+B");

		manager.Keybindings.BindChord("profile1", "test", chord1);
		manager.Keybindings.BindChord("profile2", "test", chord2);

		// Act & Assert
		Chord? binding1 = manager.Keybindings.GetChord("profile1", "test");
		Chord? binding2 = manager.Keybindings.GetChord("profile2", "test");

		Assert.IsNotNull(binding1);
		Assert.IsNotNull(binding2);
		Assert.AreEqual("Ctrl+A", binding1.ToString());
		Assert.AreEqual("Ctrl+B", binding2.ToString());
		Assert.AreNotEqual(binding1, binding2);
	}
}
