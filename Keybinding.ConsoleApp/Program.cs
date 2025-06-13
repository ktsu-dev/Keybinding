// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.CLI;

using System.Text;
using CommandLine;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using Spectre.Console;
using Profile = ktsu.Keybinding.Core.Models.Profile;

/// <summary>
/// Demo application showcasing the Keybinding library functionality
/// </summary>
public static class Program
{
	private static KeybindingManager? _manager;

	/// <summary>
	/// Main entry point for the demo application
	/// </summary>
	/// <param name="args">Command line arguments</param>
	/// <returns>Exit code</returns>
	public static async Task<int> Main(string[] args)
	{
		// Set console to use UTF-8 encoding
		Console.OutputEncoding = Encoding.UTF8;

		AnsiConsole.WriteLine("🔧 Keybinding Library Demo");
		AnsiConsole.WriteLine("========================");
		AnsiConsole.WriteLine();

		var parser = new Parser(with => {
			with.CaseSensitive = false;
			with.IgnoreUnknownArguments = false;
		});

		int result = await parser.ParseArguments<DemoOptions>(args)
			.MapResult(
				async opts => await RunDemo(opts).ConfigureAwait(false),
				errs => Task.FromResult(1)).ConfigureAwait(false);

		return result;
	}

	/// <summary>
	/// Runs the keybinding demo
	/// </summary>
	/// <param name="options">Demo options</param>
	/// <returns>Exit code</returns>
	private static async Task<int> RunDemo(DemoOptions options)
	{
		try
		{
			await InitializeManager(options.DataDirectory).ConfigureAwait(false);

			AnsiConsole.WriteLine($"📁 Data directory: {options.DataDirectory}");
			AnsiConsole.WriteLine();

			if (options.Reset)
			{
				await ResetDemo().ConfigureAwait(false);
			}

			await SetupDemoData().ConfigureAwait(false);

			// Show the musical paradigm demo
			MusicalDemo.ShowMusicalParadigm();

			DisplayCurrentStatus();

			if (options.Interactive)
			{
				await RunInteractiveDemo().ConfigureAwait(false);
			}
			else
			{
				RunBasicDemo();
			}

			return 0;
		}
		catch (ArgumentException ex)
		{
			AnsiConsole.WriteLine($"❌ Invalid argument: {ex.Message}");
			return 1;
		}
		catch (InvalidOperationException ex)
		{
			AnsiConsole.WriteLine($"❌ Operation error: {ex.Message}");
			return 1;
		}
		catch (UnauthorizedAccessException ex)
		{
			AnsiConsole.WriteLine($"❌ Access denied: {ex.Message}");
			return 1;
		}
		finally
		{
			if (_manager != null)
			{
				await _manager.SaveAsync().ConfigureAwait(false);
				_manager.Dispose();
			}
		}
	}

	/// <summary>
	/// Initializes the keybinding manager
	/// </summary>
	/// <param name="dataDirectory">Data directory path</param>
	private static async Task InitializeManager(string dataDirectory)
	{
		_manager = new KeybindingManager(dataDirectory);
		await _manager.InitializeAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Resets the demo by clearing all data
	/// </summary>
	private static async Task ResetDemo()
	{
		AnsiConsole.WriteLine("🔄 Resetting demo data...");

		// Clear all profiles except default
		IReadOnlyCollection<Profile> profiles = _manager!.Profiles.GetAllProfiles();
		foreach (Profile profile in profiles.Where(p => p.Id != "default"))
		{
			_manager.Profiles.DeleteProfile(profile.Id);
		}

		// Clear all commands
		IReadOnlyCollection<Command> commands = _manager.Commands.GetAllCommands();
		foreach (Command command in commands)
		{
			_manager.Commands.UnregisterCommand(command.Id);
		}

		await _manager.SaveAsync().ConfigureAwait(false);
		AnsiConsole.WriteLine("✅ Demo data reset complete");
		AnsiConsole.WriteLine();
	}

	/// <summary>
	/// Sets up demo data including commands and profiles
	/// </summary>
	private static async Task SetupDemoData()
	{
		AnsiConsole.WriteLine("🚀 Setting up demo data...");

		// Create default profile if it doesn't exist
		_manager!.CreateDefaultProfile();

		// Register demo commands
		Command[] demoCommands = [
			new Command("file.new", "New File", "Create a new file", "File"),
			new Command("file.open", "Open File", "Open an existing file", "File"),
			new Command("file.save", "Save File", "Save the current file", "File"),
			new Command("file.save_as", "Save As", "Save the current file with a new name", "File"),
			new Command("edit.copy", "Copy", "Copy selected text to clipboard", "Edit"),
			new Command("edit.cut", "Cut", "Cut selected text to clipboard", "Edit"),
			new Command("edit.paste", "Paste", "Paste text from clipboard", "Edit"),
			new Command("edit.undo", "Undo", "Undo the last action", "Edit"),
			new Command("edit.redo", "Redo", "Redo the last undone action", "Edit"),
			new Command("view.zoom_in", "Zoom In", "Increase the zoom level", "View"),
			new Command("view.zoom_out", "Zoom Out", "Decrease the zoom level", "View"),
			new Command("navigate.go_to_line", "Go to Line", "Navigate to a specific line", "Navigate"),
		];

		int registeredCount = _manager.RegisterCommands(demoCommands);
		AnsiConsole.WriteLine($"✅ Registered {registeredCount} demo commands");

		// Set up default chord bindings
		Dictionary<string, Chord> defaultChords = new()
		{
			["file.new"] = _manager.Keybindings.ParseChord("Ctrl+N"),
			["file.open"] = _manager.Keybindings.ParseChord("Ctrl+O"),
			["file.save"] = _manager.Keybindings.ParseChord("Ctrl+S"),
			["file.save_as"] = _manager.Keybindings.ParseChord("Ctrl+Shift+S"),
			["edit.copy"] = _manager.Keybindings.ParseChord("Ctrl+C"),
			["edit.cut"] = _manager.Keybindings.ParseChord("Ctrl+X"),
			["edit.paste"] = _manager.Keybindings.ParseChord("Ctrl+V"),
			["edit.undo"] = _manager.Keybindings.ParseChord("Ctrl+Z"),
			["edit.redo"] = _manager.Keybindings.ParseChord("Ctrl+Y"),
			["view.zoom_in"] = _manager.Keybindings.ParseChord("Ctrl+Plus"),
			["view.zoom_out"] = _manager.Keybindings.ParseChord("Ctrl+Minus"),
			["navigate.go_to_line"] = _manager.Keybindings.ParseChord("Ctrl+G"),
		};

		int chordCount = _manager.SetChords(defaultChords);
		AnsiConsole.WriteLine($"✅ Set {chordCount} default chord bindings");

		// Create additional demo profiles
		await CreateDemoProfiles().ConfigureAwait(false);

		AnsiConsole.WriteLine("✅ Demo data setup complete");
		AnsiConsole.WriteLine();
	}

	/// <summary>
	/// Creates additional demo profiles
	/// </summary>
	private static async Task CreateDemoProfiles()
	{
		// Create Vim-style profile
		Profile vimProfile = new("vim", "Vim Style", "Vim-inspired keybindings");
		if (_manager!.Profiles.CreateProfile(vimProfile))
		{
			_manager.Profiles.SetActiveProfile("vim");

			Dictionary<string, Chord> vimChords = new()
			{
				["file.save"] = _manager.Keybindings.ParseChord("Escape"),
				["edit.copy"] = _manager.Keybindings.ParseChord("y"),
				["edit.paste"] = _manager.Keybindings.ParseChord("p"),
				["edit.undo"] = _manager.Keybindings.ParseChord("u"),
				["navigate.go_to_line"] = _manager.Keybindings.ParseChord("G"),
			};

			_manager.SetChords(vimChords);
			AnsiConsole.WriteLine("✅ Created Vim-style profile");
		}

		// Create Mac-style profile
		Profile macProfile = new("mac", "Mac Style", "Mac-inspired keybindings");
		if (_manager.Profiles.CreateProfile(macProfile))
		{
			_manager.Profiles.SetActiveProfile("mac");

			Dictionary<string, Chord> macChords = new()
			{
				["file.new"] = _manager.Keybindings.ParseChord("Cmd+N"),
				["file.open"] = _manager.Keybindings.ParseChord("Cmd+O"),
				["file.save"] = _manager.Keybindings.ParseChord("Cmd+S"),
				["edit.copy"] = _manager.Keybindings.ParseChord("Cmd+C"),
				["edit.cut"] = _manager.Keybindings.ParseChord("Cmd+X"),
				["edit.paste"] = _manager.Keybindings.ParseChord("Cmd+V"),
				["edit.undo"] = _manager.Keybindings.ParseChord("Cmd+Z"),
				["edit.redo"] = _manager.Keybindings.ParseChord("Cmd+Shift+Z"),
			};

			_manager.SetChords(macChords);
			AnsiConsole.WriteLine("✅ Created Mac-style profile");
		}

		// Switch back to default profile
		_manager.Profiles.SetActiveProfile("default");
		await Task.CompletedTask.ConfigureAwait(false);
	}

	/// <summary>
	/// Displays the current status
	/// </summary>
	private static void DisplayCurrentStatus()
	{
		KeybindingSummary summary = _manager!.GetSummary();

		AnsiConsole.WriteLine("📊 Current Status:");
		AnsiConsole.WriteLine($"   Total Commands: {summary.TotalCommands}");
		AnsiConsole.WriteLine($"   Total Profiles: {summary.TotalProfiles}");
		AnsiConsole.WriteLine($"   Active Profile: {summary.ActiveProfileName} ({summary.ActiveProfileId})");
		AnsiConsole.WriteLine($"   Active Keybindings: {summary.ActiveKeybindings}");
		AnsiConsole.WriteLine();
	}

	/// <summary>
	/// Runs a basic demo showing the functionality
	/// </summary>
	private static void RunBasicDemo()
	{
		AnsiConsole.WriteLine("🎯 Basic Demo - Command and Keybinding Operations");
		AnsiConsole.WriteLine("================================================");
		AnsiConsole.WriteLine();

		// List all commands
		AnsiConsole.WriteLine("📋 Available Commands:");
		IReadOnlyCollection<Command> commands = _manager!.Commands.GetAllCommands();
		foreach (Command command in commands.OrderBy(c => c.Category).ThenBy(c => c.Name))
		{
			AnsiConsole.WriteLine($"   {command.Category}/{command.Id}: {command.Name}");
		}

		AnsiConsole.WriteLine();

		// Show keybindings for current profile
		ShowProfileKeybindings("default");

		// Demonstrate profile switching
		AnsiConsole.WriteLine("🔄 Switching between profiles...");
		AnsiConsole.WriteLine();

		foreach (string profileId in new[] { "vim", "mac", "default" })
		{
			if (_manager.Profiles.ProfileExists(profileId))
			{
				_manager.Profiles.SetActiveProfile(profileId);
				ShowProfileKeybindings(profileId);
			}
		}

		// Demonstrate finding commands by key
		AnsiConsole.WriteLine("🔍 Finding commands by key combination:");
		string[] testKeys = ["Ctrl+S", "Ctrl+C", "Cmd+N"];

		foreach (string keyStr in testKeys)
		{
			try
			{
				Chord key = _manager.Keybindings.ParseChord(keyStr);
				string? commandId = _manager.Keybindings.FindCommandByChord(key);
				if (!string.IsNullOrEmpty(commandId))
				{
					Command? command = _manager.Commands.GetCommand(commandId);
					AnsiConsole.WriteLine($"   {keyStr} → {command?.Name ?? "Unknown"}");
				}
				else
				{
					AnsiConsole.WriteLine($"   {keyStr} → (no command assigned)");
				}
			}
			catch (ArgumentException)
			{
				AnsiConsole.WriteLine($"   {keyStr} → (invalid key combination)");
			}
		}

		AnsiConsole.WriteLine();

		AnsiConsole.WriteLine("✅ Basic demo complete!");
	}

	/// <summary>
	/// Shows keybindings for a specific profile
	/// </summary>
	/// <param name="profileId">Profile ID</param>
	private static void ShowProfileKeybindings(string profileId)
	{
		Profile? profile = _manager!.Profiles.GetProfile(profileId);
		if (profile == null)
		{
			return;
		}

		AnsiConsole.WriteLine($"⌨️  Keybindings for '{profile.Name}' profile:");

		if (profile.Chords.Count != 0)
		{
			foreach (KeyValuePair<string, Chord> kvp in profile.Chords.OrderBy(k => k.Key))
			{
				Command? command = _manager.Commands.GetCommand(kvp.Key);
				AnsiConsole.WriteLine($"   {kvp.Value} → {command?.Name ?? kvp.Key}");
			}
		}
		else
		{
			AnsiConsole.WriteLine("   (no keybindings configured)");
		}

		AnsiConsole.WriteLine();
	}

	/// <summary>
	/// Runs an interactive demo allowing user input
	/// </summary>
	private static async Task RunInteractiveDemo()
	{
		AnsiConsole.WriteLine("🎮 Interactive Demo");
		AnsiConsole.WriteLine("==================");
		AnsiConsole.WriteLine("Enter commands to try the keybinding system:");
		AnsiConsole.WriteLine("  'list profiles' - Show all profiles");
		AnsiConsole.WriteLine("  'switch <profile>' - Switch to a profile");
		AnsiConsole.WriteLine("  'find <key>' - Find command by key (e.g., 'find Ctrl+S')");
		AnsiConsole.WriteLine("  'patterns' - Show common phrase patterns");
		AnsiConsole.WriteLine("  'status' - Show current status");
		AnsiConsole.WriteLine("  'help' - Show this help");
		AnsiConsole.WriteLine("  'quit' - Exit interactive mode");
		AnsiConsole.WriteLine();

		while (true)
		{
			AnsiConsole.Write("🔧 > ");
			string? input = Console.ReadLine()?.Trim();

			if (string.IsNullOrEmpty(input))
			{
				continue;
			}

			if (input.Equals("quit", StringComparison.OrdinalIgnoreCase))
			{
				break;
			}

			ProcessInteractiveCommand(input);
		}

		AnsiConsole.WriteLine("👋 Goodbye!");
		await Task.CompletedTask.ConfigureAwait(false);
	}

	/// <summary>
	/// Processes an interactive command
	/// </summary>
	/// <param name="input">User input</param>
	private static void ProcessInteractiveCommand(string input)
	{
		try
		{
			string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
			{
				return;
			}

			string command = parts[0].ToLowerInvariant();

			switch (command)
			{
				case "list":
					if (parts.Length > 1 && parts[1].Equals("profiles", StringComparison.OrdinalIgnoreCase))
					{
						ListProfiles();
					}

					break;

				case "switch":
					if (parts.Length > 1)
					{
						SwitchProfile(parts[1]);
					}
					else
					{
						AnsiConsole.WriteLine("❌ Please specify a profile name");
					}

					break;

				case "find":
					if (parts.Length > 1)
					{
						FindCommandByKey(string.Join(" ", parts.Skip(1)));
					}
					else
					{
						AnsiConsole.WriteLine("❌ Please specify a key combination");
					}

					break;

				case "status":
					DisplayCurrentStatus();
					break;

				case "patterns":
					MusicalDemo.ShowPhrasePatterns();
					break;

				case "help":
					AnsiConsole.WriteLine("Available commands:");
					AnsiConsole.WriteLine("  'list profiles' - Show all profiles");
					AnsiConsole.WriteLine("  'switch <profile>' - Switch to a profile");
					AnsiConsole.WriteLine("  'find <key>' - Find command by key");
					AnsiConsole.WriteLine("  'patterns' - Show common phrase patterns");
					AnsiConsole.WriteLine("  'status' - Show current status");
					AnsiConsole.WriteLine("  'quit' - Exit interactive mode");
					break;

				default:
					AnsiConsole.WriteLine($"❌ Unknown command: {command}");
					break;
			}
		}
		catch (ArgumentException ex)
		{
			AnsiConsole.WriteLine($"❌ Invalid argument: {ex.Message}");
		}
		catch (InvalidOperationException ex)
		{
			AnsiConsole.WriteLine($"❌ Operation error: {ex.Message}");
		}

		AnsiConsole.WriteLine();
	}

	/// <summary>
	/// Lists all available profiles
	/// </summary>
	private static void ListProfiles()
	{
		AnsiConsole.WriteLine("📋 Available Profiles:");
		IReadOnlyCollection<Profile> profiles = _manager!.Profiles.GetAllProfiles();
		Profile? activeProfile = _manager.Profiles.GetActiveProfile();

		foreach (Profile profile in profiles.OrderBy(p => p.Id))
		{
			string isActive = profile.Id == activeProfile?.Id ? " (active)" : "";
			AnsiConsole.WriteLine($"   {profile.Id}: {profile.Name}{isActive}");
			AnsiConsole.WriteLine($"      {profile.Description}");
			AnsiConsole.WriteLine($"      Chords: {profile.Chords.Count}");
		}
	}

	/// <summary>
	/// Switches to a different profile
	/// </summary>
	/// <param name="profileId">Profile ID</param>
	private static void SwitchProfile(string profileId)
	{
		if (_manager!.Profiles.ProfileExists(profileId))
		{
			_manager.Profiles.SetActiveProfile(profileId);
			Profile? profile = _manager.Profiles.GetProfile(profileId);
			AnsiConsole.WriteLine($"✅ Switched to profile: {profile?.Name} ({profileId})");
		}
		else
		{
			AnsiConsole.WriteLine($"❌ Profile not found: {profileId}");
		}
	}

	/// <summary>
	/// Finds a command by key combination
	/// </summary>
	/// <param name="keyStr">Key combination string</param>
	private static void FindCommandByKey(string keyStr)
	{
		if (_manager is null)
		{
			AnsiConsole.WriteLine("❌ Manager not initialized");
			return;
		}

		try
		{
			Chord key = _manager.Keybindings.ParseChord(keyStr);
			string? commandId = _manager.Keybindings.FindCommandByChord(key);

			if (!string.IsNullOrEmpty(commandId))
			{
				Command? command = _manager.Commands.GetCommand(commandId);
				AnsiConsole.WriteLine($"✅ {keyStr} → {command?.Name ?? "Unknown"} ({commandId})");
			}
			else
			{
				AnsiConsole.WriteLine($"❌ No command assigned to: {keyStr}");
			}
		}
		catch (ArgumentException ex)
		{
			AnsiConsole.WriteLine($"❌ Invalid key combination '{keyStr}': {ex.Message}");
		}
	}
}

/// <summary>
/// Command line options for the demo application
/// </summary>
[Verb("demo", isDefault: true, HelpText = "Run the keybinding library demo")]
public class DemoOptions
{
	/// <inheritdoc/>
	[Option('d', "data-dir", Required = false,
		HelpText = "Directory to store keybinding data",
		Default = null)]
	public string DataDirectory { get; set; } = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"Keybinding", "Demo");

	/// <inheritdoc/>
	[Option('i', "interactive", Required = false,
		HelpText = "Run in interactive mode")]
	public bool Interactive { get; set; }

	/// <inheritdoc/>
	[Option('r', "reset", Required = false,
		HelpText = "Reset demo data before running")]
	public bool Reset { get; set; }
}
