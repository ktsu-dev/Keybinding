// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.CLI;
using CommandLine;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

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
		Console.WriteLine("🔧 Keybinding Library Demo");
		Console.WriteLine("========================");
		Console.WriteLine();

		Parser.Default.Settings.CaseSensitive = false;
		Parser.Default.Settings.IgnoreUnknownArguments = false;

		ParserResult<DemoOptions> result = await Parser.Default.ParseArguments<DemoOptions>(args)
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

			Console.WriteLine($"📁 Data directory: {options.DataDirectory}");
			Console.WriteLine();

			if (options.Reset)
			{
				await ResetDemo().ConfigureAwait(false);
			}

			await SetupDemoData().ConfigureAwait(false);
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
			Console.WriteLine($"❌ Invalid argument: {ex.Message}");
			return 1;
		}
		catch (InvalidOperationException ex)
		{
			Console.WriteLine($"❌ Operation error: {ex.Message}");
			return 1;
		}
		catch (UnauthorizedAccessException ex)
		{
			Console.WriteLine($"❌ Access denied: {ex.Message}");
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
		Console.WriteLine("🔄 Resetting demo data...");

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
		Console.WriteLine("✅ Demo data reset complete");
		Console.WriteLine();
	}

	/// <summary>
	/// Sets up demo data including commands and profiles
	/// </summary>
	private static async Task SetupDemoData()
	{
		Console.WriteLine("🚀 Setting up demo data...");

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
		Console.WriteLine($"✅ Registered {registeredCount} demo commands");

		// Set up default keybindings
		Dictionary<string, KeyCombination> defaultKeybindings = new()
		{
			["file.new"] = KeyCombination.Parse("Ctrl+N"),
			["file.open"] = KeyCombination.Parse("Ctrl+O"),
			["file.save"] = KeyCombination.Parse("Ctrl+S"),
			["file.save_as"] = KeyCombination.Parse("Ctrl+Shift+S"),
			["edit.copy"] = KeyCombination.Parse("Ctrl+C"),
			["edit.cut"] = KeyCombination.Parse("Ctrl+X"),
			["edit.paste"] = KeyCombination.Parse("Ctrl+V"),
			["edit.undo"] = KeyCombination.Parse("Ctrl+Z"),
			["edit.redo"] = KeyCombination.Parse("Ctrl+Y"),
			["view.zoom_in"] = KeyCombination.Parse("Ctrl+Plus"),
			["view.zoom_out"] = KeyCombination.Parse("Ctrl+Minus"),
			["navigate.go_to_line"] = KeyCombination.Parse("Ctrl+G"),
		};

		int keybindingCount = _manager.SetKeybindings(defaultKeybindings);
		Console.WriteLine($"✅ Set {keybindingCount} default keybindings");

		// Create additional demo profiles
		await CreateDemoProfiles().ConfigureAwait(false);

		Console.WriteLine("✅ Demo data setup complete");
		Console.WriteLine();
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

			Dictionary<string, KeyCombination> vimKeybindings = new()
			{
				["file.save"] = KeyCombination.Parse("Escape"),
				["edit.copy"] = KeyCombination.Parse("y"),
				["edit.paste"] = KeyCombination.Parse("p"),
				["edit.undo"] = KeyCombination.Parse("u"),
				["navigate.go_to_line"] = KeyCombination.Parse("G"),
			};

			_manager.SetKeybindings(vimKeybindings);
			Console.WriteLine("✅ Created Vim-style profile");
		}

		// Create Mac-style profile
		Profile macProfile = new("mac", "Mac Style", "Mac-inspired keybindings");
		if (_manager.Profiles.CreateProfile(macProfile))
		{
			_manager.Profiles.SetActiveProfile("mac");

			Dictionary<string, KeyCombination> macKeybindings = new()
			{
				["file.new"] = KeyCombination.Parse("Cmd+N"),
				["file.open"] = KeyCombination.Parse("Cmd+O"),
				["file.save"] = KeyCombination.Parse("Cmd+S"),
				["edit.copy"] = KeyCombination.Parse("Cmd+C"),
				["edit.cut"] = KeyCombination.Parse("Cmd+X"),
				["edit.paste"] = KeyCombination.Parse("Cmd+V"),
				["edit.undo"] = KeyCombination.Parse("Cmd+Z"),
				["edit.redo"] = KeyCombination.Parse("Cmd+Shift+Z"),
			};

			_manager.SetKeybindings(macKeybindings);
			Console.WriteLine("✅ Created Mac-style profile");
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

		Console.WriteLine("📊 Current Status:");
		Console.WriteLine($"   Total Commands: {summary.TotalCommands}");
		Console.WriteLine($"   Total Profiles: {summary.TotalProfiles}");
		Console.WriteLine($"   Active Profile: {summary.ActiveProfileName} ({summary.ActiveProfileId})");
		Console.WriteLine($"   Active Keybindings: {summary.ActiveKeybindings}");
		Console.WriteLine();
	}

	/// <summary>
	/// Runs a basic demo showing the functionality
	/// </summary>
	private static void RunBasicDemo()
	{
		Console.WriteLine("🎯 Basic Demo - Command and Keybinding Operations");
		Console.WriteLine("================================================");
		Console.WriteLine();

		// List all commands
		Console.WriteLine("📋 Available Commands:");
		IReadOnlyCollection<Command> commands = _manager!.Commands.GetAllCommands();
		foreach (Command command in commands.OrderBy(c => c.Category).ThenBy(c => c.Name))
		{
			Console.WriteLine($"   {command.Category}/{command.Id}: {command.Name}");
		}

		Console.WriteLine();

		// Show keybindings for current profile
		ShowProfileKeybindings("default");

		// Demonstrate profile switching
		Console.WriteLine("🔄 Switching between profiles...");
		Console.WriteLine();

		foreach (string profileId in new[] { "vim", "mac", "default" })
		{
			if (_manager.Profiles.ProfileExists(profileId))
			{
				_manager.Profiles.SetActiveProfile(profileId);
				ShowProfileKeybindings(profileId);
			}
		}

		// Demonstrate finding commands by key
		Console.WriteLine("🔍 Finding commands by key combination:");
		string[] testKeys = ["Ctrl+S", "Ctrl+C", "Cmd+N"];

		foreach (string keyStr in testKeys)
		{
			try
			{
				KeyCombination key = KeyCombination.Parse(keyStr);
				string? commandId = _manager.Keybindings.FindCommandByKeybinding(key);
				if (!string.IsNullOrEmpty(commandId))
				{
					Command? command = _manager.Commands.GetCommand(commandId);
					Console.WriteLine($"   {keyStr} → {command?.Name ?? "Unknown"}");
				}
				else
				{
					Console.WriteLine($"   {keyStr} → (no command assigned)");
				}
			}
			catch (ArgumentException)
			{
				Console.WriteLine($"   {keyStr} → (invalid key combination)");
			}
		}

		Console.WriteLine();

		Console.WriteLine("✅ Basic demo complete!");
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

		Console.WriteLine($"⌨️  Keybindings for '{profile.Name}' profile:");

		if (profile.Keybindings.Count != 0)
		{
			foreach (KeyValuePair<string, KeyCombination> kvp in profile.Keybindings.OrderBy(k => k.Key))
			{
				Command? command = _manager.Commands.GetCommand(kvp.Key);
				Console.WriteLine($"   {kvp.Value} → {command?.Name ?? kvp.Key}");
			}
		}
		else
		{
			Console.WriteLine("   (no keybindings configured)");
		}

		Console.WriteLine();
	}

	/// <summary>
	/// Runs an interactive demo allowing user input
	/// </summary>
	private static async Task RunInteractiveDemo()
	{
		Console.WriteLine("🎮 Interactive Demo");
		Console.WriteLine("==================");
		Console.WriteLine("Enter commands to try the keybinding system:");
		Console.WriteLine("  'list profiles' - Show all profiles");
		Console.WriteLine("  'switch <profile>' - Switch to a profile");
		Console.WriteLine("  'find <key>' - Find command by key (e.g., 'find Ctrl+S')");
		Console.WriteLine("  'status' - Show current status");
		Console.WriteLine("  'help' - Show this help");
		Console.WriteLine("  'quit' - Exit interactive mode");
		Console.WriteLine();

		while (true)
		{
			Console.Write("🔧 > ");
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

		Console.WriteLine("👋 Goodbye!");
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
						Console.WriteLine("❌ Please specify a profile name");
					}

					break;

				case "find":
					if (parts.Length > 1)
					{
						FindCommandByKey(string.Join(" ", parts.Skip(1)));
					}
					else
					{
						Console.WriteLine("❌ Please specify a key combination");
					}

					break;

				case "status":
					DisplayCurrentStatus();
					break;

				case "help":
					Console.WriteLine("Available commands:");
					Console.WriteLine("  'list profiles' - Show all profiles");
					Console.WriteLine("  'switch <profile>' - Switch to a profile");
					Console.WriteLine("  'find <key>' - Find command by key");
					Console.WriteLine("  'status' - Show current status");
					Console.WriteLine("  'quit' - Exit interactive mode");
					break;

				default:
					Console.WriteLine($"❌ Unknown command: {command}");
					break;
			}
		}
		catch (ArgumentException ex)
		{
			Console.WriteLine($"❌ Invalid argument: {ex.Message}");
		}
		catch (InvalidOperationException ex)
		{
			Console.WriteLine($"❌ Operation error: {ex.Message}");
		}

		Console.WriteLine();
	}

	/// <summary>
	/// Lists all available profiles
	/// </summary>
	private static void ListProfiles()
	{
		Console.WriteLine("📋 Available Profiles:");
		IReadOnlyCollection<Profile> profiles = _manager!.Profiles.GetAllProfiles();
		Profile? activeProfile = _manager.Profiles.GetActiveProfile();

		foreach (Profile profile in profiles.OrderBy(p => p.Id))
		{
			string isActive = profile.Id == activeProfile?.Id ? " (active)" : "";
			Console.WriteLine($"   {profile.Id}: {profile.Name}{isActive}");
			Console.WriteLine($"      {profile.Description}");
			Console.WriteLine($"      Keybindings: {profile.Keybindings.Count}");
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
			Console.WriteLine($"✅ Switched to profile: {profile?.Name} ({profileId})");
		}
		else
		{
			Console.WriteLine($"❌ Profile not found: {profileId}");
		}
	}

	/// <summary>
	/// Finds a command by key combination
	/// </summary>
	/// <param name="keyStr">Key combination string</param>
	private static void FindCommandByKey(string keyStr)
	{
		try
		{
			KeyCombination key = KeyCombination.Parse(keyStr);
			string? commandId = _manager!.Keybindings.FindCommandByKeybinding(key);

			if (!string.IsNullOrEmpty(commandId))
			{
				Command? command = _manager.Commands.GetCommand(commandId);
				Console.WriteLine($"✅ {keyStr} → {command?.Name ?? "Unknown"} ({commandId})");
			}
			else
			{
				Console.WriteLine($"❌ No command assigned to: {keyStr}");
			}
		}
		catch (ArgumentException ex)
		{
			Console.WriteLine($"❌ Invalid key combination '{keyStr}': {ex.Message}");
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
