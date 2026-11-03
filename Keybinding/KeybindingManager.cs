// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core;

using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Helpers;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// Main facade class for managing keybindings, commands, and profiles
/// </summary>
public sealed class KeybindingManager : IDisposable
{
	private bool _disposed;

	// Ids of the stored profiles this manager has loaded or saved. SaveAsync deletes a stored profile only if
	// it is in here and no longer in memory, so profiles this manager never saw are not taken as deleted.
	private readonly HashSet<string> _persistedProfileIds = [];
	private readonly Lock _persistedStateLock = new();

	// Ids of the stored commands this manager has loaded or saved, guarded by the same lock. SaveAsync keeps a
	// stored command it never saw, and removes one only if it is in here and no longer registered.
	private readonly HashSet<string> _persistedCommandIds = [];

	// Whether this manager has read or written the stored active profile. Until it has, SaveAsync writes the
	// active profile only when one is set, so a manager that never loaded it cannot erase the user's choice.
	private bool _activeProfileKnown;

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
		Commands = Ensure.NotNull(commandRegistry);
		Profiles = Ensure.NotNull(profileManager);
		Repository = Ensure.NotNull(repository);

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
		DisposalHelper.ThrowIfDisposed(_disposed, this);

		await Repository.InitializeAsync().ConfigureAwait(false);

		// Load commands
		IReadOnlyCollection<Command> commands = await Repository.LoadCommandsAsync().ConfigureAwait(false);
		foreach (Command command in commands)
		{
			Commands.RegisterCommand(command);
		}

		lock (_persistedStateLock)
		{
			_persistedCommandIds.UnionWith(commands.Select(c => c.Id.ToString()));
		}

		// Load profiles
		IReadOnlyCollection<Profile> profiles = await Repository.LoadAllProfilesAsync().ConfigureAwait(false);
		foreach (Profile profile in profiles)
		{
			Profiles.CreateProfile(profile);
		}

		lock (_persistedStateLock)
		{
			_persistedProfileIds.UnionWith(profiles.Select(p => p.Id));
		}

		// Load active profile
		string? activeProfileId = await Repository.LoadActiveProfileAsync().ConfigureAwait(false);
		if (!string.IsNullOrWhiteSpace(activeProfileId) && Profiles.ProfileExists(activeProfileId))
		{
			Profiles.SetActiveProfile(activeProfileId);
		}

		lock (_persistedStateLock)
		{
			_activeProfileKnown = true;
		}
	}

	/// <summary>
	/// Saves all data to persistent storage
	/// </summary>
	/// <returns>Task representing the async operation</returns>
	public async Task SaveAsync()
	{
		DisposalHelper.ThrowIfDisposed(_disposed, this);

		// Save commands, keeping stored ones this manager never loaded. As with profiles below, a command that is
		// not registered here was removed only if this manager had it before.
		IReadOnlyCollection<Command> commands = Commands.GetAllCommands();
		HashSet<string> commandIds = [.. commands.Select(c => c.Id.ToString())];
		IReadOnlyCollection<Command> storedCommands = await Repository.LoadCommandsAsync().ConfigureAwait(false);
		List<Command> commandsToSave;
		lock (_persistedStateLock)
		{
			commandsToSave =
			[
				.. commands,
				.. storedCommands.Where(c => !commandIds.Contains(c.Id.ToString()) && !_persistedCommandIds.Contains(c.Id.ToString())),
			];
		}

		await Repository.SaveCommandsAsync(commandsToSave).ConfigureAwait(false);

		lock (_persistedStateLock)
		{
			_persistedCommandIds.Clear();
			_persistedCommandIds.UnionWith(commandsToSave.Select(c => c.Id.ToString()));
		}

		// Remove stored profiles that were deleted in memory, so they do not come back on the next load. Only
		// profiles this manager loaded or saved count: one it never saw is not in memory because it was never
		// loaded, not because it was deleted.
		IReadOnlyCollection<Profile> profiles = Profiles.GetAllProfiles();
		HashSet<string> profileIds = [.. profiles.Select(p => p.Id)];
		List<string> deletedProfileIds;
		lock (_persistedStateLock)
		{
			deletedProfileIds = [.. _persistedProfileIds.Where(id => !profileIds.Contains(id))];
		}

		await AsyncBatchHelper.ForEachAsync(deletedProfileIds, Repository.DeleteProfileAsync).ConfigureAwait(false);

		// Save profiles using batch helper
		await AsyncBatchHelper.ForEachAsync(profiles, Repository.SaveProfileAsync).ConfigureAwait(false);

		lock (_persistedStateLock)
		{
			_persistedProfileIds.ExceptWith(deletedProfileIds);
			_persistedProfileIds.UnionWith(profileIds);
		}

		// Save active profile. A manager that never loaded the stored one and has none set leaves it alone, so
		// saving without InitializeAsync does not clear the user's active profile.
		Profile? activeProfile = Profiles.GetActiveProfile();
		bool activeProfileKnown;
		lock (_persistedStateLock)
		{
			activeProfileKnown = _activeProfileKnown;
		}

		if (activeProfile is not null || activeProfileKnown)
		{
			await Repository.SaveActiveProfileAsync(activeProfile?.Id).ConfigureAwait(false);
			lock (_persistedStateLock)
			{
				_activeProfileKnown = true;
			}
		}
	}

	/// <summary>
	/// Creates a default profile if no profiles exist
	/// </summary>
	/// <param name="profileId">The ID for the default profile</param>
	/// <param name="profileName">The name for the default profile</param>
	/// <param name="activation">
	/// Whether to set as the active profile. A profile is activated only if no profile is active, so a startup
	/// call recovers from a deleted or unset active profile without overriding the one the user chose. The
	/// profile with the given ID is activated if it exists or was just created; otherwise, when other profiles
	/// exist, the first of them by ordinal ID is activated.
	/// </param>
	/// <returns>
	/// The created profile, or null if it was not created: because a profile with the given ID already exists,
	/// or because other profiles exist. A startup call therefore does not bring back a default profile the user
	/// deleted after making another.
	/// </returns>
	public Profile? CreateDefaultProfile(string profileId = "default", string profileName = "Default", ProfileActivation activation = ProfileActivation.Activate)
	{
		DisposalHelper.ThrowIfDisposed(_disposed, this);

		if (Profiles.ProfileExists(profileId))
		{
			if (activation == ProfileActivation.Activate && Profiles.GetActiveProfile() is null)
			{
				Profiles.SetActiveProfile(profileId);
			}

			return null;
		}

		IReadOnlyCollection<Profile> existingProfiles = Profiles.GetAllProfiles();
		if (existingProfiles.Count > 0)
		{
			if (activation == ProfileActivation.Activate && Profiles.GetActiveProfile() is null)
			{
				string firstProfileId = existingProfiles.Select(p => p.Id).OrderBy(id => id, StringComparer.Ordinal).First();
				Profiles.SetActiveProfile(firstProfileId);
			}

			return null;
		}

		Profile profile = new(profileId, profileName, "Default keybinding profile");

		if (Profiles.CreateProfile(profile))
		{
			if (activation == ProfileActivation.Activate && Profiles.GetActiveProfile() is null)
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
		DisposalHelper.ThrowIfDisposed(_disposed, this);
		Ensure.NotNull(commands);

		return OperationHelper.ExecuteWithCount(commands, Commands.RegisterCommand, shouldContinueOnError: false);
	}

	/// <summary>
	/// Sets multiple chord bindings for the active profile
	/// </summary>
	/// <param name="chords">Dictionary of command ID to chord mappings</param>
	/// <returns>
	/// Number of bindings from the batch that remain once it is applied. A chord runs only one command, so when
	/// the batch gives one chord to two commands the later entry takes it and only that binding is counted.
	/// Use <see cref="SetChords(IReadOnlyDictionary{string, Chord}, out IReadOnlyList{string})"/> to learn which
	/// commands lost their chord.
	/// </returns>
	public int SetChords(IReadOnlyDictionary<string, Chord> chords) => SetChords(chords, out _);

	/// <summary>
	/// Sets multiple chord bindings for the active profile, and reports the commands that lost a chord to them
	/// </summary>
	/// <param name="chords">Dictionary of command ID to chord mappings</param>
	/// <param name="displacedCommandIds">
	/// The IDs of the commands whose chord was taken by a later binding: commands outside the batch that held one
	/// of its chords, and entries of the batch whose chord a later entry took. A command that the batch itself
	/// rebinds, and that keeps its new binding, is not listed. Each ID appears once, in the order it was displaced.
	/// </param>
	/// <returns>Number of bindings from the batch that remain once it is applied</returns>
	public int SetChords(IReadOnlyDictionary<string, Chord> chords, out IReadOnlyList<string> displacedCommandIds)
	{
		DisposalHelper.ThrowIfDisposed(_disposed, this);
		Ensure.NotNull(chords);

		Profile activeProfile = Profiles.GetActiveProfile() ?? throw new InvalidOperationException("No active profile is set");

		List<string> displaced = [];
		Dictionary<string, Chord> bound = [];

		// Bind into the profile resolved above, so a concurrent SetActiveProfile cannot split the batch
		OperationHelper.ExecuteWithCount(chords, (commandId, chord) =>
		{
			string[] holders = [.. activeProfile.GetAllChords().Where(b => b.Value.Equals(chord) && b.Key != commandId.Trim()).Select(b => b.Key)];
			if (!Keybindings.BindChord(activeProfile.Id, commandId, chord))
			{
				return false;
			}

			bound[commandId.Trim()] = chord;
			foreach (string holder in holders)
			{
				if (!chord.Equals(activeProfile.GetChord(holder)) && !displaced.Contains(holder))
				{
					displaced.Add(holder);
				}
			}

			return true;
		});

		// A command the batch rebinds after it lost its old chord is not displaced, as long as it keeps the new one
		displacedCommandIds = [.. displaced.Where(id => !(bound.TryGetValue(id, out Chord? chord) && chord.Equals(activeProfile.GetChord(id))))];
		return bound.Count(b => b.Value.Equals(activeProfile.GetChord(b.Key)));
	}

	/// <summary>
	/// Gets a summary of the current keybinding state
	/// </summary>
	/// <returns>Summary information</returns>
	public KeybindingSummary GetSummary()
	{
		DisposalHelper.ThrowIfDisposed(_disposed, this);

		Profile? activeProfile = Profiles.GetActiveProfile();
		int totalCommands = Commands.GetAllCommands().Count;
		int totalProfiles = Profiles.GetAllProfiles().Count;
		int activeKeybindings = activeProfile?.ChordCount ?? 0;

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
