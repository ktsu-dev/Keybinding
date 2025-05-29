// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

namespace ktsu.Keybinding.Core;

/// <summary>
/// Main facade class for managing keybindings, commands, and profiles
/// </summary>
public sealed class KeybindingManager : IDisposable
{
	private readonly ICommandRegistry _commandRegistry;
	private readonly IProfileManager _profileManager;
	private readonly IKeybindingService _keybindingService;
	private readonly IKeybindingRepository _repository;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingManager"/> class with default services
	/// </summary>
	/// <param name="dataDirectory">Directory to store keybinding data</param>
	public KeybindingManager(string dataDirectory)
		: this(
			new CommandRegistry(),
			new ProfileManager(),
			new JsonKeybindingRepository(dataDirectory))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingManager"/> class with custom services
	/// </summary>
	/// <param name="commandRegistry">The command registry service</param>
	/// <param name="profileManager">The profile manager service</param>
	/// <param name="repository">The keybinding repository</param>
	public KeybindingManager(
		ICommandRegistry commandRegistry,
		IProfileManager profileManager,
		IKeybindingRepository repository)
	{
		_commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
		_profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));

		_keybindingService = new KeybindingService(_commandRegistry, _profileManager);
	}

	/// <summary>
	/// Gets the command registry for managing commands
	/// </summary>
	public ICommandRegistry Commands => _commandRegistry;

	/// <summary>
	/// Gets the profile manager for managing profiles
	/// </summary>
	public IProfileManager Profiles => _profileManager;

	/// <summary>
	/// Gets the keybinding service for managing keybindings
	/// </summary>
	public IKeybindingService Keybindings => _keybindingService;

	/// <summary>
	/// Gets the repository for persistence operations
	/// </summary>
	public IKeybindingRepository Repository => _repository;

	/// <summary>
	/// Initializes the keybinding manager and loads persisted data
	/// </summary>
	/// <returns>Task representing the async operation</returns>
	public async Task InitializeAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		await _repository.InitializeAsync();

		// Load commands
		var commands = await _repository.LoadCommandsAsync();
		foreach (var command in commands)
		{
			_commandRegistry.RegisterCommand(command);
		}

		// Load profiles
		var profiles = await _repository.LoadAllProfilesAsync();
		foreach (var profile in profiles)
		{
			_profileManager.CreateProfile(profile);
		}

		// Load active profile
		var activeProfileId = await _repository.LoadActiveProfileAsync();
		if (!string.IsNullOrEmpty(activeProfileId) && _profileManager.ProfileExists(activeProfileId))
		{
			_profileManager.SetActiveProfile(activeProfileId);
		}
	}

	/// <summary>
	/// Saves all data to persistent storage
	/// </summary>
	/// <returns>Task representing the async operation</returns>
	public async Task SaveAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		// Save commands
		var commands = _commandRegistry.GetAllCommands();
		await _repository.SaveCommandsAsync(commands);

		// Save profiles
		var profiles = _profileManager.GetAllProfiles();
		foreach (var profile in profiles)
		{
			await _repository.SaveProfileAsync(profile);
		}

		// Save active profile
		var activeProfile = _profileManager.GetActiveProfile();
		await _repository.SaveActiveProfileAsync(activeProfile?.Id);
	}

	/// <summary>
	/// Creates a default profile if none exist
	/// </summary>
	/// <param name="profileId">The ID for the default profile</param>
	/// <param name="profileName">The name for the default profile</param>
	/// <param name="setAsActive">Whether to set as the active profile</param>
	/// <returns>The created profile, or null if a profile already exists with the given ID</returns>
	public Profile? CreateDefaultProfile(string profileId = "default", string profileName = "Default", bool setAsActive = true)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (_profileManager.ProfileExists(profileId))
			return null;

		var profile = new Profile(profileId, profileName, "Default keybinding profile");

		if (_profileManager.CreateProfile(profile))
		{
			if (setAsActive)
			{
				_profileManager.SetActiveProfile(profileId);
			}
			return profile;
		}

		return null;
	}

	/// <summary>
	/// Registers a batch of commands
	/// </summary>
	/// <param name="commands">The commands to register</param>
	/// <returns>Number of commands successfully registered</returns>
	public int RegisterCommands(IEnumerable<Command> commands)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(commands);

		return commands.Count(command => _commandRegistry.RegisterCommand(command));
	}

	/// <summary>
	/// Sets multiple keybindings for the active profile
	/// </summary>
	/// <param name="keybindings">Dictionary of command ID to key combination mappings</param>
	/// <returns>Number of keybindings successfully set</returns>
	public int SetKeybindings(IReadOnlyDictionary<string, KeyCombination> keybindings)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(keybindings);

		var activeProfile = _profileManager.GetActiveProfile();
		if (activeProfile is null)
			throw new InvalidOperationException("No active profile is set");

		int successCount = 0;
		foreach (var kvp in keybindings)
		{
			try
			{
				if (_keybindingService.SetKeybinding(kvp.Key, kvp.Value))
				{
					successCount++;
				}
			}
			catch
			{
				// Skip invalid keybindings
			}
		}

		return successCount;
	}

	/// <summary>
	/// Gets a summary of the current keybinding state
	/// </summary>
	/// <returns>Summary information</returns>
	public KeybindingSummary GetSummary()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		var activeProfile = _profileManager.GetActiveProfile();
		var totalCommands = _commandRegistry.GetAllCommands().Count;
		var totalProfiles = _profileManager.GetAllProfiles().Count;
		var activeKeybindings = activeProfile?.Keybindings.Count ?? 0;

		return new KeybindingSummary
		{
			TotalCommands = totalCommands,
			TotalProfiles = totalProfiles,
			ActiveProfileId = activeProfile?.Id,
			ActiveProfileName = activeProfile?.Name,
			ActiveKeybindings = activeKeybindings
		};
	}

	/// <summary>
	/// Disposes the keybinding manager
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
		}
	}
}

/// <summary>
/// Summary information about the current keybinding state
/// </summary>
public sealed class KeybindingSummary
{
	/// <summary>
	/// Gets or sets the total number of registered commands
	/// </summary>
	public int TotalCommands { get; set; }

	/// <summary>
	/// Gets or sets the total number of profiles
	/// </summary>
	public int TotalProfiles { get; set; }

	/// <summary>
	/// Gets or sets the active profile ID
	/// </summary>
	public string? ActiveProfileId { get; set; }

	/// <summary>
	/// Gets or sets the active profile name
	/// </summary>
	public string? ActiveProfileName { get; set; }

	/// <summary>
	/// Gets or sets the number of keybindings in the active profile
	/// </summary>
	public int ActiveKeybindings { get; set; }
}
