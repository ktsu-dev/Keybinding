// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core;

using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Helpers;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// Main facade class for managing keybindings, commands, and profiles
/// </summary>
/// <remarks>
/// The primary constructor takes every service, including the keybinding service. It is the constructor dependency
/// injection selects, so a custom or decorated <see cref="IKeybindingService"/> registered with the container is
/// the one <see cref="Keybindings"/> returns.
/// </remarks>
/// <param name="commandRegistry">The command registry service</param>
/// <param name="profileManager">The profile manager service</param>
/// <param name="repository">The keybinding repository</param>
/// <param name="keybindingService">
/// The keybinding service. It should work over the same <paramref name="commandRegistry"/> and
/// <paramref name="profileManager"/>, or bindings made through it will not be the ones the manager saves.
/// </param>
public sealed class KeybindingManager(
	ICommandRegistry commandRegistry,
	IProfileManager profileManager,
	IKeybindingRepository repository,
	IKeybindingService keybindingService) : IDisposable
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
	/// Initializes a new instance of the <see cref="KeybindingManager"/> class with custom services, and a
	/// <see cref="KeybindingService"/> built over them
	/// </summary>
	/// <param name="commandRegistry">The command registry service</param>
	/// <param name="profileManager">The profile manager service</param>
	/// <param name="repository">The keybinding repository</param>
	public KeybindingManager(
		ICommandRegistry commandRegistry,
		IProfileManager profileManager,
		IKeybindingRepository repository)
		: this(
			Ensure.NotNull(commandRegistry),
			Ensure.NotNull(profileManager),
			repository,
			new KeybindingService(commandRegistry, profileManager))
	{
	}

	/// <summary>
	/// Gets the command registry for managing commands
	/// </summary>
	public ICommandRegistry Commands { get; } = Ensure.NotNull(commandRegistry);

	/// <summary>
	/// Gets the profile manager for managing profiles
	/// </summary>
	public IProfileManager Profiles { get; } = Ensure.NotNull(profileManager);

	/// <summary>
	/// Gets the keybinding service for managing keybindings
	/// </summary>
	public IKeybindingService Keybindings { get; } = Ensure.NotNull(keybindingService);

	/// <summary>
	/// Gets the repository for persistence operations
	/// </summary>
	public IKeybindingRepository Repository { get; } = Ensure.NotNull(repository);

	/// <summary>
	/// Initializes the keybinding manager and loads persisted data
	/// </summary>
	/// <remarks>
	/// Call this before creating profiles. A profile that already exists with a stored profile's id keeps its own
	/// bindings and takes in each stored binding whose command and chord it does not already use.
	/// </remarks>
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

		// Load profiles. A profile with the same id may already be in memory, created before this call by app code
		// or by a profile manager shared through dependency injection. Its stored bindings are merged into it rather
		// than dropped, since the id is recorded as persisted below and the next save would otherwise overwrite them.
		IReadOnlyCollection<Profile> profiles = await Repository.LoadAllProfilesAsync().ConfigureAwait(false);
		foreach (Profile profile in profiles)
		{
			if (!Profiles.CreateProfile(profile) && Profiles.GetProfile(profile.Id) is Profile existing)
			{
				MergeStoredBindings(existing, profile);
			}
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
	/// Merges the bindings of a stored profile into the in-memory profile with the same id. The in-memory profile
	/// wins: a stored binding is skipped when the in-memory profile already binds that command, or already uses
	/// that chord for another command, so no chord ever runs two commands.
	/// </summary>
	/// <param name="existing">The profile already in memory</param>
	/// <param name="stored">The stored profile with the same id</param>
	private static void MergeStoredBindings(Profile existing, Profile stored)
	{
		IReadOnlyDictionary<string, Chord> current = existing.GetAllChords();
		HashSet<Chord> usedChords = [.. current.Values];
		foreach ((string commandId, Chord chord) in stored.GetAllChords())
		{
			if (!current.ContainsKey(commandId) && usedChords.Add(chord))
			{
				existing.SetChord(commandId, chord);
			}
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
		// The merge runs inside the repository's update, so another manager's save cannot land between reading the
		// stored commands and writing the merged ones.
		IReadOnlyCollection<Command> commandsToSave = await Repository.UpdateCommandsAsync(storedCommands =>
		{
			lock (_persistedStateLock)
			{
				return
				[
					.. commands,
					.. storedCommands.Where(c => !commandIds.Contains(c.Id.ToString()) && !_persistedCommandIds.Contains(c.Id.ToString())),
				];
			}
		}).ConfigureAwait(false);

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
