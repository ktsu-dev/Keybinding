// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// Main facade class for managing keybindings, commands, and profiles
/// </summary>
public sealed class KeybindingManager : IDisposable
{
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
		Commands = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
		Profiles = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
		Repository = repository ?? throw new ArgumentNullException(nameof(repository));

		Keybindings = new KeybindingService(Commands, Profiles);
	}

	/// <summary>
	/// Gets the command registry for managing commands
	/// </summary>
	public ICommandRegistry Commands { get; }

	/// <summary>
	/// Gets the profile manager for managing profiles
	/// </summary>
	public IProfileManager Profiles { get; }

	/// <summary>
	/// Gets the keybinding service for managing keybindings
	/// </summary>
	public IKeybindingService Keybindings { get; }

	/// <summary>
	/// Gets the repository for persistence operations
	/// </summary>
	public IKeybindingRepository Repository { get; }

	/// <summary>
	/// Initializes the keybinding manager and loads persisted data
	/// </summary>
	/// <returns>Task representing the async operation</returns>
	public async Task InitializeAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		await Repository.InitializeAsync().ConfigureAwait(false);

		// Load commands
		IReadOnlyCollection<Command> commands = await Repository.LoadCommandsAsync().ConfigureAwait(false);
		foreach (Command command in commands)
		{
			Commands.RegisterCommand(command);
		}

		// Load profiles
		IReadOnlyCollection<Profile> profiles = await Repository.LoadAllProfilesAsync().ConfigureAwait(false);
		foreach (Profile profile in profiles)
		{
			Profiles.CreateProfile(profile);
		}

		// Load active profile
		string? activeProfileId = await Repository.LoadActiveProfileAsync().ConfigureAwait(false);
		if (!string.IsNullOrEmpty(activeProfileId) && Profiles.ProfileExists(activeProfileId))
		{
			Profiles.SetActiveProfile(activeProfileId);
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
		IReadOnlyCollection<Command> commands = Commands.GetAllCommands();
		await Repository.SaveCommandsAsync(commands).ConfigureAwait(false);

		// Save profiles
		IReadOnlyCollection<Profile> profiles = Profiles.GetAllProfiles();
		foreach (Profile profile in profiles)
		{
			await Repository.SaveProfileAsync(profile).ConfigureAwait(false);
		}

		// Save active profile
		Profile? activeProfile = Profiles.GetActiveProfile();
		await Repository.SaveActiveProfileAsync(activeProfile?.Id).ConfigureAwait(false);
	}

	/// <summary>
	/// Creates a default profile if none exist
	/// </summary>
	/// <param name="profileId">The ID for the default profile</param>
	/// <param name="profileName">The name for the default profile</param>
	/// <param name="activation">Whether to set as the active profile</param>
	/// <returns>The created profile, or null if a profile already exists with the given ID</returns>
	public Profile? CreateDefaultProfile(string profileId = "default", string profileName = "Default", ProfileActivation activation = ProfileActivation.Activate)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (Profiles.ProfileExists(profileId))
		{
			return null;
		}

		Profile profile = new(profileId, profileName, "Default keybinding profile");

		if (Profiles.CreateProfile(profile))
		{
			if (activation == ProfileActivation.Activate)
			{
				Profiles.SetActiveProfile(profileId);
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

		return commands.Count(Commands.RegisterCommand);
	}

	/// <summary>
	/// Sets multiple chord bindings for the active profile
	/// </summary>
	/// <param name="chords">Dictionary of command ID to chord mappings</param>
	/// <returns>Number of chord bindings successfully set</returns>
	public int SetChords(IReadOnlyDictionary<string, Chord> chords)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(chords);

		Profile activeProfile = Profiles.GetActiveProfile() ?? throw new InvalidOperationException("No active profile is set");

		int successCount = 0;
		foreach (KeyValuePair<string, Chord> kvp in chords)
		{
			try
			{
				if (Keybindings.BindChord(kvp.Key, kvp.Value))
				{
					successCount++;
				}
			}
			catch (ArgumentException)
			{
				// Skip invalid chord bindings
			}
			catch (InvalidOperationException)
			{
				// Skip chord bindings that can't be set due to state issues
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

		Profile? activeProfile = Profiles.GetActiveProfile();
		int totalCommands = Commands.GetAllCommands().Count;
		int totalProfiles = Profiles.GetAllProfiles().Count;
		int activeKeybindings = activeProfile?.Chords.Count ?? 0;

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
