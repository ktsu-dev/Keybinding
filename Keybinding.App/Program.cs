// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.App;

/// <summary>
/// Sample application demonstrating the Keybinding library
/// </summary>
public static class Program
{
	private static KeybindingManager? _manager;
	private static readonly string DataDirectory = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"KeybindingSample");

	/// <summary>
	/// Main entry point for the sample application
	/// </summary>
	/// <returns>Exit code</returns>
	public static async Task<int> Main()
	{
		Console.WriteLine("=== Keybinding Library Sample Application ===");
		Console.WriteLine();

		try
		{
			await InitializeAsync();
			await RunInteractiveDemo();
			return 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			return 1;
		}
		finally
		{
			if (_manager != null)
			{
				await _manager.SaveAsync();
				_manager.Dispose();
			}
		}
	}

	private static async Task InitializeAsync()
	{
		Console.WriteLine("Initializing Keybinding Manager...");
		_manager = new KeybindingManager(DataDirectory);
		await _manager.InitializeAsync();

		// Create default profile if none exist
		if (!_manager.Profiles.GetAllProfiles().Any())
		{
			_manager.CreateDefaultProfile();
			Console.WriteLine("Created default profile.");
		}

		// Register sample commands if none exist
		if (!_manager.Commands.GetAllCommands().Any())
		{
			await RegisterSampleCommands();
		}

		Console.WriteLine($"Data directory: {DataDirectory}");
		Console.WriteLine();
	}

	private static async Task RegisterSampleCommands()
	{
		Console.WriteLine("Registering sample commands...");

		var sampleCommands = new[]
		{
			new Command("file.new", "New File", "Create a new file", "File"),
			new Command("file.open", "Open File", "Open an existing file", "File"),
			new Command("file.save", "Save File", "Save the current file", "File"),
			new Command("file.save_as", "Save As", "Save the current file with a new name", "File"),
			new Command("file.close", "Close File", "Close the current file", "File"),
			new Command("file.exit", "Exit", "Exit the application", "File"),

			new Command("edit.undo", "Undo", "Undo the last action", "Edit"),
			new Command("edit.redo", "Redo", "Redo the last undone action", "Edit"),
			new Command("edit.cut", "Cut", "Cut selected text to clipboard", "Edit"),
			new Command("edit.copy", "Copy", "Copy selected text to clipboard", "Edit"),
			new Command("edit.paste", "Paste", "Paste text from clipboard", "Edit"),
			new Command("edit.select_all", "Select All", "Select all text", "Edit"),
			new Command("edit.find", "Find", "Find text in document", "Edit"),
			new Command("edit.replace", "Replace", "Find and replace text", "Edit"),

			new Command("view.zoom_in", "Zoom In", "Increase zoom level", "View"),
			new Command("view.zoom_out", "Zoom Out", "Decrease zoom level", "View"),
			new Command("view.zoom_reset", "Reset Zoom", "Reset zoom to 100%", "View"),
			new Command("view.fullscreen", "Toggle Fullscreen", "Toggle fullscreen mode", "View"),

			new Command("help.about", "About", "Show application information", "Help"),
			new Command("help.shortcuts", "Keyboard Shortcuts", "Show keyboard shortcuts", "Help"),
		};

		int registered = _manager.RegisterCommands(sampleCommands);
		Console.WriteLine($"Registered {registered} sample commands.");

		// Set some default keybindings
		var defaultKeybindings = new Dictionary<string, string>
		{
			{ "file.new", "Ctrl+N" },
			{ "file.open", "Ctrl+O" },
			{ "file.save", "Ctrl+S" },
			{ "file.save_as", "Ctrl+Shift+S" },
			{ "file.close", "Ctrl+W" },
			{ "file.exit", "Alt+F4" },
			{ "edit.undo", "Ctrl+Z" },
			{ "edit.redo", "Ctrl+Y" },
			{ "edit.cut", "Ctrl+X" },
			{ "edit.copy", "Ctrl+C" },
			{ "edit.paste", "Ctrl+V" },
			{ "edit.select_all", "Ctrl+A" },
			{ "edit.find", "Ctrl+F" },
			{ "edit.replace", "Ctrl+H" },
			{ "view.zoom_in", "Ctrl+Plus" },
			{ "view.zoom_out", "Ctrl+Minus" },
			{ "view.zoom_reset", "Ctrl+0" },
			{ "view.fullscreen", "F11" },
			{ "help.shortcuts", "F1" },
		};

		var keybindingDict = new Dictionary<string, KeyCombination>();
		foreach (var kvp in defaultKeybindings)
		{
			if (KeyCombination.TryParse(kvp.Value, out var keyCombination))
			{
				keybindingDict[kvp.Key] = keyCombination;
			}
		}

		int keybindingsSet = _manager.SetKeybindings(keybindingDict);
		Console.WriteLine($"Set {keybindingsSet} default keybindings.");
		Console.WriteLine();
	}

	private static async Task RunInteractiveDemo()
	{
		while (true)
		{
			ShowMainMenu();
			var choice = Console.ReadLine()?.Trim();

			switch (choice?.ToLower())
			{
				case "1":
					await ShowStatus();
					break;
				case "2":
					await ShowProfiles();
					break;
				case "3":
					await ShowCommands();
					break;
				case "4":
					await ShowKeybindings();
					break;
				case "5":
					await ManageProfiles();
					break;
				case "6":
					await ManageKeybindings();
					break;
				case "7":
					await TestKeyLookup();
					break;
				case "8":
					await DemonstrateProfileSwitching();
					break;
				case "q":
				case "quit":
				case "exit":
					Console.WriteLine("Goodbye!");
					return;
				default:
					Console.WriteLine("Invalid choice. Please try again.");
					break;
			}

			Console.WriteLine("\nPress any key to continue...");
			Console.ReadKey();
			Console.Clear();
		}
	}

	private static void ShowMainMenu()
	{
		Console.WriteLine("=== Keybinding Library Demo ===");
		Console.WriteLine();
		Console.WriteLine("1. Show Status");
		Console.WriteLine("2. Show Profiles");
		Console.WriteLine("3. Show Commands");
		Console.WriteLine("4. Show Keybindings");
		Console.WriteLine("5. Manage Profiles");
		Console.WriteLine("6. Manage Keybindings");
		Console.WriteLine("7. Test Key Lookup");
		Console.WriteLine("8. Demonstrate Profile Switching");
		Console.WriteLine("Q. Quit");
		Console.WriteLine();
		Console.Write("Choose an option: ");
	}

	private static async Task ShowStatus()
	{
		var summary = _manager!.GetSummary();
		Console.WriteLine("\n=== Current Status ===");
		Console.WriteLine($"Total Commands: {summary.TotalCommands}");
		Console.WriteLine($"Total Profiles: {summary.TotalProfiles}");
		Console.WriteLine($"Active Profile: {summary.ActiveProfileName ?? "None"} ({summary.ActiveProfileId ?? "None"})");
		Console.WriteLine($"Active Keybindings: {summary.ActiveKeybindings}");
	}

	private static async Task ShowProfiles()
	{
		Console.WriteLine("\n=== Profiles ===");
		var profiles = _manager!.Profiles.GetAllProfiles();
		var activeProfile = _manager.Profiles.GetActiveProfile();

		foreach (var profile in profiles.OrderBy(p => p.Name))
		{
			var isActive = profile.Id == activeProfile?.Id ? " (ACTIVE)" : "";
			Console.WriteLine($"• {profile.Name} ({profile.Id}){isActive}");
			if (!string.IsNullOrEmpty(profile.Description))
			{
				Console.WriteLine($"  Description: {profile.Description}");
			}
			Console.WriteLine($"  Keybindings: {profile.GetAllKeybindings().Count()}");
		}
	}

	private static async Task ShowCommands()
	{
		Console.WriteLine("\n=== Commands ===");
		var commands = _manager!.Commands.GetAllCommands();

		var groupedCommands = commands.GroupBy(c => string.IsNullOrEmpty(c.Category) ? "General" : c.Category)
			.OrderBy(g => g.Key);

		foreach (var group in groupedCommands)
		{
			Console.WriteLine($"\n[{group.Key}]");
			foreach (var command in group.OrderBy(c => c.Name))
			{
				Console.WriteLine($"  • {command.Name} ({command.Id})");
				if (!string.IsNullOrEmpty(command.Description))
				{
					Console.WriteLine($"    {command.Description}");
				}
			}
		}
	}

	private static async Task ShowKeybindings()
	{
		Console.WriteLine("\n=== Current Keybindings ===");
		var activeProfile = _manager!.Profiles.GetActiveProfile();
		if (activeProfile == null)
		{
			Console.WriteLine("No active profile set.");
			return;
		}

		Console.WriteLine($"Profile: {activeProfile.Name}");
		var keybindings = activeProfile.GetAllKeybindings();

		var groupedKeybindings = keybindings
			.Select(kvp => new {
				KeyCombination = kvp.Value,
				Command = _manager.Commands.GetCommand(kvp.Key)
			})
			.Where(x => x.Command != null)
			.GroupBy(x => string.IsNullOrEmpty(x.Command!.Category) ? "General" : x.Command.Category)
			.OrderBy(g => g.Key);

		foreach (var group in groupedKeybindings)
		{
			Console.WriteLine($"\n[{group.Key}]");
			foreach (var item in group.OrderBy(x => x.KeyCombination.ToString()))
			{
				Console.WriteLine($"  {item.KeyCombination}: {item.Command!.Name}");
			}
		}
	}

	private static async Task ManageProfiles()
	{
		Console.WriteLine("\n=== Profile Management ===");
		Console.WriteLine("1. Create Profile");
		Console.WriteLine("2. Activate Profile");
		Console.WriteLine("3. Duplicate Profile");
		Console.WriteLine("4. Rename Profile");
		Console.WriteLine("5. Delete Profile");
		Console.Write("Choose an option: ");

		var choice = Console.ReadLine()?.Trim();
		switch (choice)
		{
			case "1":
				await CreateProfile();
				break;
			case "2":
				await ActivateProfile();
				break;
			case "3":
				await DuplicateProfile();
				break;
			case "4":
				await RenameProfile();
				break;
			case "5":
				await DeleteProfile();
				break;
			default:
				Console.WriteLine("Invalid choice.");
				break;
		}
	}

	private static async Task CreateProfile()
	{
		Console.Write("Enter profile ID: ");
		var id = Console.ReadLine()?.Trim();
		if (string.IsNullOrEmpty(id)) return;

		Console.Write("Enter profile name: ");
		var name = Console.ReadLine()?.Trim();
		if (string.IsNullOrEmpty(name)) name = id;

		Console.Write("Enter profile description (optional): ");
		var description = Console.ReadLine()?.Trim() ?? "";

		var profile = new Profile(id, name, description);
		if (_manager!.Profiles.CreateProfile(profile))
		{
			Console.WriteLine($"Created profile '{name}' successfully.");
		}
		else
		{
			Console.WriteLine($"Failed to create profile. Profile with ID '{id}' may already exist.");
		}
	}

	private static async Task ActivateProfile()
	{
		var profiles = _manager!.Profiles.GetAllProfiles().ToList();
		if (!profiles.Any())
		{
			Console.WriteLine("No profiles available.");
			return;
		}

		Console.WriteLine("Available profiles:");
		for (int i = 0; i < profiles.Count; i++)
		{
			Console.WriteLine($"{i + 1}. {profiles[i].Name} ({profiles[i].Id})");
		}

		Console.Write("Enter profile number: ");
		if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= profiles.Count)
		{
			var profile = profiles[index - 1];
			if (_manager.Profiles.SetActiveProfile(profile.Id))
			{
				Console.WriteLine($"Activated profile '{profile.Name}'.");
			}
			else
			{
				Console.WriteLine("Failed to activate profile.");
			}
		}
		else
		{
			Console.WriteLine("Invalid selection.");
		}
	}

	private static async Task DuplicateProfile()
	{
		var profiles = _manager!.Profiles.GetAllProfiles().ToList();
		if (!profiles.Any())
		{
			Console.WriteLine("No profiles available to duplicate.");
			return;
		}

		Console.WriteLine("Select profile to duplicate:");
		for (int i = 0; i < profiles.Count; i++)
		{
			Console.WriteLine($"{i + 1}. {profiles[i].Name} ({profiles[i].Id})");
		}

		Console.Write("Enter profile number: ");
		if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= profiles.Count)
		{
			var sourceProfile = profiles[index - 1];

			Console.Write("Enter new profile ID: ");
			var newId = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(newId)) return;

			Console.Write("Enter new profile name: ");
			var newName = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(newName)) newName = newId;

			var newProfile = _manager.Profiles.DuplicateProfile(sourceProfile.Id, newId, newName);
			if (newProfile != null)
			{
				Console.WriteLine($"Duplicated profile '{sourceProfile.Name}' to '{newName}' successfully.");
			}
			else
			{
				Console.WriteLine("Failed to duplicate profile.");
			}
		}
		else
		{
			Console.WriteLine("Invalid selection.");
		}
	}

	private static async Task RenameProfile()
	{
		var profiles = _manager!.Profiles.GetAllProfiles().ToList();
		if (!profiles.Any())
		{
			Console.WriteLine("No profiles available to rename.");
			return;
		}

		Console.WriteLine("Select profile to rename:");
		for (int i = 0; i < profiles.Count; i++)
		{
			Console.WriteLine($"{i + 1}. {profiles[i].Name} ({profiles[i].Id})");
		}

		Console.Write("Enter profile number: ");
		if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= profiles.Count)
		{
			var profile = profiles[index - 1];

			Console.Write($"Enter new name for '{profile.Name}': ");
			var newName = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(newName)) return;

			if (_manager.Profiles.RenameProfile(profile.Id, newName))
			{
				Console.WriteLine($"Renamed profile to '{newName}' successfully.");
			}
			else
			{
				Console.WriteLine("Failed to rename profile.");
			}
		}
		else
		{
			Console.WriteLine("Invalid selection.");
		}
	}

	private static async Task DeleteProfile()
	{
		var profiles = _manager!.Profiles.GetAllProfiles().ToList();
		if (!profiles.Any())
		{
			Console.WriteLine("No profiles available to delete.");
			return;
		}

		Console.WriteLine("Select profile to delete:");
		for (int i = 0; i < profiles.Count; i++)
		{
			Console.WriteLine($"{i + 1}. {profiles[i].Name} ({profiles[i].Id})");
		}

		Console.Write("Enter profile number: ");
		if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= profiles.Count)
		{
			var profile = profiles[index - 1];

			Console.Write($"Are you sure you want to delete '{profile.Name}'? (y/N): ");
			var confirmation = Console.ReadLine()?.Trim().ToLower();
			if (confirmation == "y" || confirmation == "yes")
			{
				if (_manager.Profiles.DeleteProfile(profile.Id))
				{
					Console.WriteLine($"Deleted profile '{profile.Name}' successfully.");
				}
				else
				{
					Console.WriteLine("Failed to delete profile. Cannot delete the active profile.");
				}
			}
			else
			{
				Console.WriteLine("Deletion cancelled.");
			}
		}
		else
		{
			Console.WriteLine("Invalid selection.");
		}
	}

	private static async Task ManageKeybindings()
	{
		Console.WriteLine("\n=== Keybinding Management ===");
		Console.WriteLine("1. Set Keybinding");
		Console.WriteLine("2. Remove Keybinding");
		Console.Write("Choose an option: ");

		var choice = Console.ReadLine()?.Trim();
		switch (choice)
		{
			case "1":
				await SetKeybinding();
				break;
			case "2":
				await RemoveKeybinding();
				break;
			default:
				Console.WriteLine("Invalid choice.");
				break;
		}
	}

	private static async Task SetKeybinding()
	{
		var commands = _manager!.Commands.GetAllCommands().ToList();
		if (!commands.Any())
		{
			Console.WriteLine("No commands available.");
			return;
		}

		Console.WriteLine("Select command:");
		for (int i = 0; i < commands.Count; i++)
		{
			Console.WriteLine($"{i + 1}. {commands[i].Name} ({commands[i].Id})");
		}

		Console.Write("Enter command number: ");
		if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= commands.Count)
		{
			var command = commands[index - 1];

			Console.Write("Enter key combination (e.g., 'Ctrl+S', 'Alt+F4'): ");
			var keyString = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(keyString)) return;

			if (KeyCombination.TryParse(keyString, out var keyCombination))
			{
				if (_manager.Keybindings.SetKeybinding(command.Id, keyCombination))
				{
					Console.WriteLine($"Set keybinding {keyCombination} for '{command.Name}' successfully.");
				}
				else
				{
					Console.WriteLine("Failed to set keybinding.");
				}
			}
			else
			{
				Console.WriteLine($"Invalid key combination: {keyString}");
			}
		}
		else
		{
			Console.WriteLine("Invalid selection.");
		}
	}

	private static async Task RemoveKeybinding()
	{
		var activeProfile = _manager!.Profiles.GetActiveProfile();
		if (activeProfile == null)
		{
			Console.WriteLine("No active profile set.");
			return;
		}

		var keybindings = activeProfile.GetAllKeybindings().ToList();
		if (!keybindings.Any())
		{
			Console.WriteLine("No keybindings in the active profile.");
			return;
		}

		Console.WriteLine("Select keybinding to remove:");
		for (int i = 0; i < keybindings.Count; i++)
		{
			var command = _manager.Commands.GetCommand(keybindings[i].Key);
			var commandName = command?.Name ?? keybindings[i].Key;
			Console.WriteLine($"{i + 1}. {keybindings[i].Value}: {commandName}");
		}

		Console.Write("Enter keybinding number: ");
		if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= keybindings.Count)
		{
			var keybinding = keybindings[index - 1];
			if (_manager.Keybindings.RemoveKeybinding(keybinding.Key))
			{
				Console.WriteLine($"Removed keybinding for '{keybinding.Key}' successfully.");
			}
			else
			{
				Console.WriteLine("Failed to remove keybinding.");
			}
		}
		else
		{
			Console.WriteLine("Invalid selection.");
		}
	}

	private static async Task TestKeyLookup()
	{
		Console.WriteLine("\n=== Test Key Lookup ===");
		Console.Write("Enter key combination to test (e.g., 'Ctrl+S', 'Alt+F4'): ");
		var keyString = Console.ReadLine()?.Trim();
		if (string.IsNullOrEmpty(keyString)) return;

		if (KeyCombination.TryParse(keyString, out var keyCombination))
		{
			var commands = _manager!.Keybindings.FindCommandsByKeyCombination(keyCombination);
			if (commands.Any())
			{
				Console.WriteLine($"\nCommands bound to {keyCombination}:");
				foreach (var command in commands)
				{
					Console.WriteLine($"  • {command.Name} ({command.Id})");
					if (!string.IsNullOrEmpty(command.Description))
					{
						Console.WriteLine($"    {command.Description}");
					}
				}
			}
			else
			{
				Console.WriteLine($"\nNo commands found for key combination {keyCombination}");
			}
		}
		else
		{
			Console.WriteLine($"Invalid key combination: {keyString}");
		}
	}

	private static async Task DemonstrateProfileSwitching()
	{
		Console.WriteLine("\n=== Profile Switching Demo ===");

		// Create a demo profile if it doesn't exist
		const string demoProfileId = "demo";
		if (!_manager!.Profiles.ProfileExists(demoProfileId))
		{
			var demoProfile = new Profile(demoProfileId, "Demo Profile", "Profile for demonstration");
			_manager.Profiles.CreateProfile(demoProfile);

			// Set some different keybindings
			var demoKeybindings = new Dictionary<string, KeyCombination>();
			if (KeyCombination.TryParse("Ctrl+Shift+N", out var newFileKey))
				demoKeybindings["file.new"] = newFileKey;
			if (KeyCombination.TryParse("Ctrl+Shift+O", out var openFileKey))
				demoKeybindings["file.open"] = openFileKey;
			if (KeyCombination.TryParse("Ctrl+Shift+S", out var saveFileKey))
				demoKeybindings["file.save"] = saveFileKey;

			_manager.Profiles.SetActiveProfile(demoProfileId);
			_manager.SetKeybindings(demoKeybindings);
		}

		var originalProfile = _manager.Profiles.GetActiveProfile();

		Console.WriteLine($"Current profile: {originalProfile?.Name ?? "None"}");
		Console.WriteLine("Switching to demo profile...");

		_manager.Profiles.SetActiveProfile(demoProfileId);
		var demoProfile = _manager.Profiles.GetActiveProfile();

		Console.WriteLine($"Now using profile: {demoProfile?.Name}");
		Console.WriteLine("\nDemo profile keybindings:");

		var demoKeybindings = demoProfile!.GetAllKeybindings();
		foreach (var kvp in demoKeybindings.Take(5))
		{
			var command = _manager.Commands.GetCommand(kvp.Key);
			Console.WriteLine($"  {kvp.Value}: {command?.Name ?? kvp.Key}");
		}

		Console.WriteLine("\nSwitching back to original profile...");
		if (originalProfile != null)
		{
			_manager.Profiles.SetActiveProfile(originalProfile.Id);
			Console.WriteLine($"Restored profile: {originalProfile.Name}");
		}
	}
}
