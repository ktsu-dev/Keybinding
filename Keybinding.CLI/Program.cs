// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.CommandLine;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.CLI;

/// <summary>
/// Command-line interface for the Keybinding library
/// </summary>
public static class Program
{
	private static KeybindingManager? _manager;

	/// <summary>
	/// Main entry point for the CLI application
	/// </summary>
	/// <param name="args">Command line arguments</param>
	/// <returns>Exit code</returns>
	public static async Task<int> Main(string[] args)
	{
		var dataDirectoryOption = new Option<string>(
			"--data-dir",
			description: "Directory to store keybinding data",
			getDefaultValue: () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keybinding"));

		var rootCommand = new RootCommand("Keybinding Management CLI")
		{
			dataDirectoryOption
		};

		// Profile commands
		var profileCommand = new Command("profile", "Manage keybinding profiles");
		var profileListCommand = new Command("list", "List all profiles");
		var profileCreateCommand = new Command("create", "Create a new profile");
		var profileDeleteCommand = new Command("delete", "Delete a profile");
		var profileActivateCommand = new Command("activate", "Activate a profile");
		var profileRenameCommand = new Command("rename", "Rename a profile");
		var profileDuplicateCommand = new Command("duplicate", "Duplicate a profile");

		// Command management commands
		var commandCommand = new Command("command", "Manage commands");
		var commandListCommand = new Command("list", "List all commands");
		var commandRegisterCommand = new Command("register", "Register a new command");
		var commandUnregisterCommand = new Command("unregister", "Unregister a command");

		// Keybinding commands
		var keybindingCommand = new Command("keybinding", "Manage keybindings");
		var keybindingListCommand = new Command("list", "List all keybindings");
		var keybindingSetCommand = new Command("set", "Set a keybinding");
		var keybindingRemoveCommand = new Command("remove", "Remove a keybinding");
		var keybindingFindCommand = new Command("find", "Find command by key combination");

		// Status command
		var statusCommand = new Command("status", "Show current status");

		// Add options to commands
		AddProfileOptions(profileCreateCommand, profileDeleteCommand, profileActivateCommand, profileRenameCommand, profileDuplicateCommand);
		AddCommandOptions(commandRegisterCommand, commandUnregisterCommand);
		AddKeybindingOptions(keybindingSetCommand, keybindingRemoveCommand, keybindingFindCommand);

		// Build command hierarchy
		profileCommand.AddCommand(profileListCommand);
		profileCommand.AddCommand(profileCreateCommand);
		profileCommand.AddCommand(profileDeleteCommand);
		profileCommand.AddCommand(profileActivateCommand);
		profileCommand.AddCommand(profileRenameCommand);
		profileCommand.AddCommand(profileDuplicateCommand);

		commandCommand.AddCommand(commandListCommand);
		commandCommand.AddCommand(commandRegisterCommand);
		commandCommand.AddCommand(commandUnregisterCommand);

		keybindingCommand.AddCommand(keybindingListCommand);
		keybindingCommand.AddCommand(keybindingSetCommand);
		keybindingCommand.AddCommand(keybindingRemoveCommand);
		keybindingCommand.AddCommand(keybindingFindCommand);

		rootCommand.AddCommand(profileCommand);
		rootCommand.AddCommand(commandCommand);
		rootCommand.AddCommand(keybindingCommand);
		rootCommand.AddCommand(statusCommand);

		// Set up handlers
		SetupHandlers(dataDirectoryOption, profileListCommand, profileCreateCommand, profileDeleteCommand,
			profileActivateCommand, profileRenameCommand, profileDuplicateCommand,
			commandListCommand, commandRegisterCommand, commandUnregisterCommand,
			keybindingListCommand, keybindingSetCommand, keybindingRemoveCommand, keybindingFindCommand,
			statusCommand);

		return await rootCommand.InvokeAsync(args);
	}

	private static void AddProfileOptions(params Command[] commands)
	{
		var profileIdOption = new Option<string>("--id", "Profile ID") { IsRequired = true };
		var profileNameOption = new Option<string>("--name", "Profile name");
		var profileDescriptionOption = new Option<string>("--description", "Profile description");
		var newProfileIdOption = new Option<string>("--new-id", "New profile ID");
		var newProfileNameOption = new Option<string>("--new-name", "New profile name");

		foreach (var command in commands)
		{
			switch (command.Name)
			{
				case "create":
					command.AddOption(profileIdOption);
					command.AddOption(profileNameOption);
					command.AddOption(profileDescriptionOption);
					break;
				case "delete":
				case "activate":
					command.AddOption(profileIdOption);
					break;
				case "rename":
					command.AddOption(profileIdOption);
					command.AddOption(newProfileNameOption);
					break;
				case "duplicate":
					command.AddOption(profileIdOption);
					command.AddOption(newProfileIdOption);
					command.AddOption(newProfileNameOption);
					break;
			}
		}
	}

	private static void AddCommandOptions(params Command[] commands)
	{
		var commandIdOption = new Option<string>("--id", "Command ID") { IsRequired = true };
		var commandNameOption = new Option<string>("--name", "Command name") { IsRequired = true };
		var commandDescriptionOption = new Option<string>("--description", "Command description");
		var commandCategoryOption = new Option<string>("--category", "Command category");

		foreach (var command in commands)
		{
			switch (command.Name)
			{
				case "register":
					command.AddOption(commandIdOption);
					command.AddOption(commandNameOption);
					command.AddOption(commandDescriptionOption);
					command.AddOption(commandCategoryOption);
					break;
				case "unregister":
					command.AddOption(commandIdOption);
					break;
			}
		}
	}

	private static void AddKeybindingOptions(params Command[] commands)
	{
		var commandIdOption = new Option<string>("--command", "Command ID") { IsRequired = true };
		var keyOption = new Option<string>("--key", "Key combination (e.g., 'Ctrl+S', 'Alt+F4')") { IsRequired = true };

		foreach (var command in commands)
		{
			switch (command.Name)
			{
				case "set":
					command.AddOption(commandIdOption);
					command.AddOption(keyOption);
					break;
				case "remove":
					command.AddOption(commandIdOption);
					break;
				case "find":
					command.AddOption(keyOption);
					break;
			}
		}
	}

	private static void SetupHandlers(Option<string> dataDirectoryOption, params Command[] commands)
	{
		foreach (var command in commands)
		{
			command.SetHandler(async (context) =>
			{
				var dataDirectory = context.ParseResult.GetValueForOption(dataDirectoryOption)!;
				await InitializeManager(dataDirectory);

				try
				{
					await HandleCommand(command, context);
				}
				finally
				{
					if (_manager != null)
					{
						await _manager.SaveAsync();
						_manager.Dispose();
						_manager = null;
					}
				}
			});
		}
	}

	private static async Task InitializeManager(string dataDirectory)
	{
		_manager = new KeybindingManager(dataDirectory);
		await _manager.InitializeAsync();

		// Create default profile if none exist
		if (!_manager.Profiles.GetAllProfiles().Any())
		{
			_manager.CreateDefaultProfile();
			Console.WriteLine("Created default profile.");
		}
	}

	private static async Task HandleCommand(Command command, InvocationContext context)
	{
		if (_manager == null)
			throw new InvalidOperationException("Manager not initialized");

		var commandPath = GetCommandPath(command);

		switch (commandPath)
		{
			case "profile list":
				await HandleProfileList();
				break;
			case "profile create":
				await HandleProfileCreate(context);
				break;
			case "profile delete":
				await HandleProfileDelete(context);
				break;
			case "profile activate":
				await HandleProfileActivate(context);
				break;
			case "profile rename":
				await HandleProfileRename(context);
				break;
			case "profile duplicate":
				await HandleProfileDuplicate(context);
				break;
			case "command list":
				await HandleCommandList();
				break;
			case "command register":
				await HandleCommandRegister(context);
				break;
			case "command unregister":
				await HandleCommandUnregister(context);
				break;
			case "keybinding list":
				await HandleKeybindingList();
				break;
			case "keybinding set":
				await HandleKeybindingSet(context);
				break;
			case "keybinding remove":
				await HandleKeybindingRemove(context);
				break;
			case "keybinding find":
				await HandleKeybindingFind(context);
				break;
			case "status":
				await HandleStatus();
				break;
		}
	}

	private static string GetCommandPath(Command command)
	{
		var parts = new List<string>();
		var current = command;
		while (current != null && current.Name != "keybinding-cli")
		{
			parts.Insert(0, current.Name);
			current = current.Parents.OfType<Command>().FirstOrDefault();
		}
		return string.Join(" ", parts);
	}

	private static async Task HandleProfileList()
	{
		var profiles = _manager!.Profiles.GetAllProfiles();
		var activeProfile = _manager.Profiles.GetActiveProfile();

		Console.WriteLine("Profiles:");
		foreach (var profile in profiles.OrderBy(p => p.Name))
		{
			var isActive = profile.Id == activeProfile?.Id ? " (active)" : "";
			Console.WriteLine($"  {profile.Id}: {profile.Name}{isActive}");
			if (!string.IsNullOrEmpty(profile.Description))
			{
				Console.WriteLine($"    Description: {profile.Description}");
			}
			Console.WriteLine($"    Keybindings: {profile.GetAllKeybindings().Count()}");
		}
	}

	private static async Task HandleProfileCreate(InvocationContext context)
	{
		var id = context.ParseResult.GetValueForOption<string>("--id")!;
		var name = context.ParseResult.GetValueForOption<string>("--name") ?? id;
		var description = context.ParseResult.GetValueForOption<string>("--description") ?? "";

		var profile = new Profile(id, name, description);
		if (_manager!.Profiles.CreateProfile(profile))
		{
			Console.WriteLine($"Created profile '{name}' with ID '{id}'");
		}
		else
		{
			Console.WriteLine($"Failed to create profile. Profile with ID '{id}' may already exist.");
		}
	}

	private static async Task HandleProfileDelete(InvocationContext context)
	{
		var id = context.ParseResult.GetValueForOption<string>("--id")!;

		if (_manager!.Profiles.DeleteProfile(id))
		{
			Console.WriteLine($"Deleted profile '{id}'");
		}
		else
		{
			Console.WriteLine($"Failed to delete profile '{id}'. Profile may not exist or may be the active profile.");
		}
	}

	private static async Task HandleProfileActivate(InvocationContext context)
	{
		var id = context.ParseResult.GetValueForOption<string>("--id")!;

		if (_manager!.Profiles.SetActiveProfile(id))
		{
			Console.WriteLine($"Activated profile '{id}'");
		}
		else
		{
			Console.WriteLine($"Failed to activate profile '{id}'. Profile may not exist.");
		}
	}

	private static async Task HandleProfileRename(InvocationContext context)
	{
		var id = context.ParseResult.GetValueForOption<string>("--id")!;
		var newName = context.ParseResult.GetValueForOption<string>("--new-name")!;

		if (_manager!.Profiles.RenameProfile(id, newName))
		{
			Console.WriteLine($"Renamed profile '{id}' to '{newName}'");
		}
		else
		{
			Console.WriteLine($"Failed to rename profile '{id}'. Profile may not exist.");
		}
	}

	private static async Task HandleProfileDuplicate(InvocationContext context)
	{
		var sourceId = context.ParseResult.GetValueForOption<string>("--id")!;
		var newId = context.ParseResult.GetValueForOption<string>("--new-id")!;
		var newName = context.ParseResult.GetValueForOption<string>("--new-name") ?? newId;

		var newProfile = _manager!.Profiles.DuplicateProfile(sourceId, newId, newName);
		if (newProfile != null)
		{
			Console.WriteLine($"Duplicated profile '{sourceId}' to '{newId}' ('{newName}')");
		}
		else
		{
			Console.WriteLine($"Failed to duplicate profile. Source profile '{sourceId}' may not exist or target ID '{newId}' may already exist.");
		}
	}

	private static async Task HandleCommandList()
	{
		var commands = _manager!.Commands.GetAllCommands();

		Console.WriteLine("Commands:");
		foreach (var cmd in commands.OrderBy(c => c.Category).ThenBy(c => c.Name))
		{
			var category = string.IsNullOrEmpty(cmd.Category) ? "General" : cmd.Category;
			Console.WriteLine($"  [{category}] {cmd.Id}: {cmd.Name}");
			if (!string.IsNullOrEmpty(cmd.Description))
			{
				Console.WriteLine($"    Description: {cmd.Description}");
			}
		}
	}

	private static async Task HandleCommandRegister(InvocationContext context)
	{
		var id = context.ParseResult.GetValueForOption<string>("--id")!;
		var name = context.ParseResult.GetValueForOption<string>("--name")!;
		var description = context.ParseResult.GetValueForOption<string>("--description") ?? "";
		var category = context.ParseResult.GetValueForOption<string>("--category") ?? "";

		var command = new Command(id, name, description, category);
		if (_manager!.Commands.RegisterCommand(command))
		{
			Console.WriteLine($"Registered command '{name}' with ID '{id}'");
		}
		else
		{
			Console.WriteLine($"Failed to register command. Command with ID '{id}' may already exist.");
		}
	}

	private static async Task HandleCommandUnregister(InvocationContext context)
	{
		var id = context.ParseResult.GetValueForOption<string>("--id")!;

		if (_manager!.Commands.UnregisterCommand(id))
		{
			Console.WriteLine($"Unregistered command '{id}'");
		}
		else
		{
			Console.WriteLine($"Failed to unregister command '{id}'. Command may not exist.");
		}
	}

	private static async Task HandleKeybindingList()
	{
		var activeProfile = _manager!.Profiles.GetActiveProfile();
		if (activeProfile == null)
		{
			Console.WriteLine("No active profile set.");
			return;
		}

		Console.WriteLine($"Keybindings for profile '{activeProfile.Name}':");
		var keybindings = activeProfile.GetAllKeybindings();

		foreach (var kvp in keybindings.OrderBy(k => k.Value.ToString()))
		{
			var command = _manager.Commands.GetCommand(kvp.Key);
			var commandName = command?.Name ?? kvp.Key;
			Console.WriteLine($"  {kvp.Value}: {commandName}");
		}
	}

	private static async Task HandleKeybindingSet(InvocationContext context)
	{
		var commandId = context.ParseResult.GetValueForOption<string>("--command")!;
		var keyString = context.ParseResult.GetValueForOption<string>("--key")!;

		if (!KeyCombination.TryParse(keyString, out var keyCombination))
		{
			Console.WriteLine($"Invalid key combination: {keyString}");
			return;
		}

		if (_manager!.Keybindings.SetKeybinding(commandId, keyCombination))
		{
			Console.WriteLine($"Set keybinding {keyCombination} for command '{commandId}'");
		}
		else
		{
			Console.WriteLine($"Failed to set keybinding. Command '{commandId}' may not exist or key combination may be invalid.");
		}
	}

	private static async Task HandleKeybindingRemove(InvocationContext context)
	{
		var commandId = context.ParseResult.GetValueForOption<string>("--command")!;

		if (_manager!.Keybindings.RemoveKeybinding(commandId))
		{
			Console.WriteLine($"Removed keybinding for command '{commandId}'");
		}
		else
		{
			Console.WriteLine($"Failed to remove keybinding. Command '{commandId}' may not exist or may not have a keybinding.");
		}
	}

	private static async Task HandleKeybindingFind(InvocationContext context)
	{
		var keyString = context.ParseResult.GetValueForOption<string>("--key")!;

		if (!KeyCombination.TryParse(keyString, out var keyCombination))
		{
			Console.WriteLine($"Invalid key combination: {keyString}");
			return;
		}

		var commands = _manager!.Keybindings.FindCommandsByKeyCombination(keyCombination);
		if (commands.Any())
		{
			Console.WriteLine($"Commands bound to {keyCombination}:");
			foreach (var command in commands)
			{
				Console.WriteLine($"  {command.Id}: {command.Name}");
			}
		}
		else
		{
			Console.WriteLine($"No commands found for key combination {keyCombination}");
		}
	}

	private static async Task HandleStatus()
	{
		var summary = _manager!.GetSummary();
		Console.WriteLine("Keybinding Manager Status:");
		Console.WriteLine($"  Total Commands: {summary.TotalCommands}");
		Console.WriteLine($"  Total Profiles: {summary.TotalProfiles}");
		Console.WriteLine($"  Active Profile: {summary.ActiveProfileName ?? "None"} ({summary.ActiveProfileId ?? "None"})");
		Console.WriteLine($"  Active Keybindings: {summary.ActiveKeybindings}");
	}
}