// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Services;

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// JSON file-based implementation of keybinding repository
/// </summary>
/// <remarks>
/// <para>
/// Every read-modify-write of the data directory is serialized, across every repository in the process that
/// uses the same directory, so concurrent saves cannot lose each other's profiles or commands.
/// </para>
/// <para>
/// Each file is written to a temporary file beside it and then moved over it, so a crash part-way through a
/// save leaves either the old file or the new one, never a truncated one.
/// </para>
/// <para>
/// A file that is not valid JSON at all loads as empty, but is first moved aside to
/// <c>&lt;name&gt;.corrupt-&lt;timestamp&gt;</c>, so the next save cannot destroy the only copy. A file that
/// parses but has a bad entry loses only that entry: each profile, chord and command is read on its own.
/// </para>
/// </remarks>
public sealed class JsonKeybindingRepository : IKeybindingRepository
{
	// One lock per data directory, shared by every repository in the process over that directory
	private static readonly ConcurrentDictionary<string, SemaphoreSlim> DirectoryLocks = new(StringComparer.Ordinal);

	private readonly string _dataDirectory;
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly SemaphoreSlim _lock;

	private const string ProfilesFileName = Constants.Files.ProfilesFileName;
	private const string CommandsFileName = Constants.Files.CommandsFileName;
	private const string ActiveProfileFileName = Constants.Files.ActiveProfileFileName;

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonKeybindingRepository"/> class
	/// </summary>
	/// <param name="dataDirectory">The directory to store data files</param>
	public JsonKeybindingRepository(string dataDirectory)
	{
		if (string.IsNullOrWhiteSpace(dataDirectory))
		{
			throw new ArgumentException(Constants.ErrorMessages.DataDirectoryNullOrWhitespace, nameof(dataDirectory));
		}

		_dataDirectory = dataDirectory.Trim();
		_lock = DirectoryLocks.GetOrAdd(Path.GetFullPath(_dataDirectory), _ => new SemaphoreSlim(1, 1));
		_jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new JsonStringEnumConverter() }
		};
	}

	/// <inheritdoc/>
	public Task SaveProfileAsync(Profile profile)
	{
		Ensure.NotNull(profile);

		return WithLockAsync(async () =>
		{
			List<Profile> profiles = await LoadAllProfilesUnlockedAsync().ConfigureAwait(false);

			// Replace the stored profile with the same ID if there is one
			profiles.RemoveAll(p => p.Id == profile.Id);
			profiles.Add(profile);

			await WriteProfilesUnlockedAsync(profiles).ConfigureAwait(false);
		});
	}

	/// <inheritdoc/>
	public async Task<Profile?> LoadProfileAsync(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		IReadOnlyCollection<Profile> profiles = await LoadAllProfilesAsync().ConfigureAwait(false);
		return profiles.FirstOrDefault(p => p.Id == profileId.Trim());
	}

	/// <inheritdoc/>
	public async Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync() =>
		await WithLockAsync(LoadAllProfilesUnlockedAsync).ConfigureAwait(false);

	private async Task<List<Profile>> LoadAllProfilesUnlockedAsync()
	{
		List<JsonElement>? entries = await ReadEntriesUnlockedAsync(ProfilesFileName).ConfigureAwait(false);
		if (entries is null)
		{
			return [];
		}

		List<Profile> profiles = [];
		foreach (JsonElement entry in entries)
		{
			Profile? profile = ToProfile(entry);
			if (profile is not null)
			{
				profiles.Add(profile);
			}
		}

		return profiles;
	}

	/// <summary>
	/// Converts a stored profile, skipping it when it does not match the schema or its id or name is invalid,
	/// and skipping any chord that does not match the schema or whose notes are invalid, so one bad entry does
	/// not stop the rest of the file from loading. A skipped entry is not written back by a later save.
	/// </summary>
	private Profile? ToProfile(JsonElement entry)
	{
		StoredProfileDto? dto = Deserialize<StoredProfileDto>(entry, "profile");
		if (dto is null)
		{
			return null;
		}

		Profile profile;
		try
		{
			profile = new(dto.Id, dto.Name, dto.Description);
		}
		catch (ArgumentException ex)
		{
			System.Diagnostics.Debug.WriteLine($"Skipping stored profile '{dto.Id}': {ex.Message}");
			return null;
		}

		foreach (KeyValuePair<string, JsonElement> kvp in dto.Chords ?? [])
		{
			ChordDto? chordDto = Deserialize<ChordDto>(kvp.Value, $"chord for '{kvp.Key}' in profile '{profile.Id}'");
			if (chordDto is null)
			{
				continue;
			}

			try
			{
				Chord chord = new((chordDto.Notes ?? []).Select(noteString => new Note(noteString)));
				profile.SetChord(kvp.Key, chord);
			}
			catch (ArgumentException ex)
			{
				System.Diagnostics.Debug.WriteLine($"Skipping chord for '{kvp.Key}' in profile '{profile.Id}': {ex.Message}");
			}
		}

		return profile;
	}

	/// <inheritdoc/>
	public Task DeleteProfileAsync(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		return WithLockAsync(async () =>
		{
			List<Profile> profiles = await LoadAllProfilesUnlockedAsync().ConfigureAwait(false);
			profiles.RemoveAll(p => p.Id == profileId.Trim());
			await WriteProfilesUnlockedAsync(profiles).ConfigureAwait(false);
		});
	}

	private Task WriteProfilesUnlockedAsync(IEnumerable<Profile> profiles)
	{
		IEnumerable<ProfileDto> profileDtos = profiles.Select(p => new ProfileDto
		{
			Id = p.Id,
			Name = p.Name,
			Description = p.Description,
			Chords = p.GetAllChords().ToDictionary(
				kvp => kvp.Key,
				kvp => new ChordDto
				{
					Notes = [.. kvp.Value.Notes.Select(n => n.ToString())]
				})
		});

		return WriteFileUnlockedAsync(ProfilesFileName, JsonSerializer.Serialize(profileDtos, _jsonOptions));
	}

	/// <inheritdoc/>
	public Task SaveCommandsAsync(IEnumerable<Command> commands)
	{
		Ensure.NotNull(commands);

		List<Command> commandsToSave = [.. commands];
		return WithLockAsync(() => WriteCommandsUnlockedAsync(commandsToSave));
	}

	/// <inheritdoc/>
	public async Task<IReadOnlyCollection<Command>> LoadCommandsAsync() =>
		(await WithLockAsync(LoadCommandsUnlockedAsync).ConfigureAwait(false)).AsReadOnly();

	/// <inheritdoc/>
	/// <remarks>
	/// The read, the update and the write all happen under the directory lock, so a concurrent save through any
	/// repository over the same directory cannot land between them and be lost.
	/// </remarks>
	public async Task<IReadOnlyCollection<Command>> UpdateCommandsAsync(Func<IReadOnlyCollection<Command>, IEnumerable<Command>> update)
	{
		Ensure.NotNull(update);

		return await WithLockAsync(async () =>
		{
			List<Command> storedCommands = await LoadCommandsUnlockedAsync().ConfigureAwait(false);
			List<Command> commands = [.. update(storedCommands.AsReadOnly())];
			await WriteCommandsUnlockedAsync(commands).ConfigureAwait(false);
			return (IReadOnlyCollection<Command>)commands.AsReadOnly();
		}).ConfigureAwait(false);
	}

	private async Task<List<Command>> LoadCommandsUnlockedAsync()
	{
		List<JsonElement>? entries = await ReadEntriesUnlockedAsync(CommandsFileName).ConfigureAwait(false);
		List<Command> commands = [];
		foreach (JsonElement entry in entries ?? [])
		{
			Command? command = ToCommand(entry);
			if (command is not null)
			{
				commands.Add(command);
			}
		}

		return commands;
	}

	private Task WriteCommandsUnlockedAsync(IEnumerable<Command> commands)
	{
		IEnumerable<CommandDto> commandDtos = commands.Select(c => new CommandDto
		{
			Id = c.Id,
			Name = c.Name,
			Description = c.Description,
			Category = c.Category
		});

		return WriteFileUnlockedAsync(CommandsFileName, JsonSerializer.Serialize(commandDtos, _jsonOptions));
	}

	/// <summary>
	/// Converts a stored command, skipping it when it is null, does not match the schema, or its id or name is
	/// invalid, so one bad entry does not stop the rest of the file from loading. A skipped entry is not written
	/// back by a later save.
	/// </summary>
	private Command? ToCommand(JsonElement entry)
	{
		CommandDto? dto = Deserialize<CommandDto>(entry, "command");
		if (dto is null)
		{
			return null;
		}

		try
		{
			return new(dto.Id, dto.Name, dto.Description, dto.Category);
		}
		catch (ArgumentException ex)
		{
			System.Diagnostics.Debug.WriteLine($"Skipping stored command '{dto.Id}': {ex.Message}");
			return null;
		}
	}

	/// <inheritdoc/>
	public Task SaveActiveProfileAsync(string? profileId)
	{
		ActiveProfileDto activeProfileDto = new() { ActiveProfileId = profileId };
		return WithLockAsync(() => WriteFileUnlockedAsync(ActiveProfileFileName, JsonSerializer.Serialize(activeProfileDto, _jsonOptions)));
	}

	/// <inheritdoc/>
	public async Task<string?> LoadActiveProfileAsync() =>
		await WithLockAsync(async () =>
		{
			string? json = await ReadFileUnlockedAsync(ActiveProfileFileName).ConfigureAwait(false);
			if (json is null)
			{
				return null;
			}

			try
			{
				ActiveProfileDto? activeProfileDto = JsonSerializer.Deserialize<ActiveProfileDto>(json, _jsonOptions);
				return string.IsNullOrWhiteSpace(activeProfileDto?.ActiveProfileId) ? null : activeProfileDto.ActiveProfileId;
			}
			catch (JsonException)
			{
				// An unreadable active profile means none is active; the file holds nothing worth keeping aside
				return null;
			}
		}).ConfigureAwait(false);

	private async Task WithLockAsync(Func<Task> action)
	{
		await _lock.WaitAsync().ConfigureAwait(false);
		try
		{
			await EnsureInitializedAsync().ConfigureAwait(false);
			await action().ConfigureAwait(false);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<T> WithLockAsync<T>(Func<Task<T>> action)
	{
		await _lock.WaitAsync().ConfigureAwait(false);
		try
		{
			await EnsureInitializedAsync().ConfigureAwait(false);
			return await action().ConfigureAwait(false);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<string?> ReadFileUnlockedAsync(string fileName)
	{
		string path = Path.Combine(_dataDirectory, fileName);
		return File.Exists(path) ? await File.ReadAllTextAsync(path).ConfigureAwait(false) : null;
	}

	/// <summary>
	/// Reads a file holding a JSON array as its separate entries, so each can be converted on its own. A file
	/// that is missing gives null; one that is not a JSON array is moved aside and also gives null.
	/// </summary>
	private async Task<List<JsonElement>?> ReadEntriesUnlockedAsync(string fileName)
	{
		string? json = await ReadFileUnlockedAsync(fileName).ConfigureAwait(false);
		if (json is null)
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<List<JsonElement>>(json, _jsonOptions) ?? [];
		}
		catch (JsonException ex)
		{
			MoveCorruptFileAside(fileName, ex);
			return null;
		}
	}

	private T? Deserialize<T>(JsonElement entry, string description) where T : class
	{
		try
		{
			return entry.Deserialize<T>(_jsonOptions);
		}
		catch (JsonException ex)
		{
			System.Diagnostics.Debug.WriteLine($"Skipping stored {description}: {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// Moves a file that is not valid JSON to a side file, so the save that follows a load cannot overwrite the
	/// only copy of whatever it held.
	/// </summary>
	private void MoveCorruptFileAside(string fileName, JsonException reason)
	{
		string path = Path.Combine(_dataDirectory, fileName);
		string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
		string sidePath = $"{path}.corrupt-{stamp}";
		for (int attempt = 1; File.Exists(sidePath); attempt++)
		{
			sidePath = $"{path}.corrupt-{stamp}-{attempt}";
		}

		File.Move(path, sidePath);
		System.Diagnostics.Debug.WriteLine($"{fileName} is not valid JSON and was moved to {sidePath}: {reason.Message}");
	}

	/// <summary>
	/// Writes a file through a temporary file in the same directory and a move over the target, so the target
	/// is never truncated in place.
	/// </summary>
	private async Task WriteFileUnlockedAsync(string fileName, string contents)
	{
		string path = Path.Combine(_dataDirectory, fileName);
		string tempPath = Path.Combine(_dataDirectory, $"{fileName}.{Guid.NewGuid():N}.tmp");
		try
		{
			await File.WriteAllTextAsync(tempPath, contents).ConfigureAwait(false);
			File.Move(tempPath, path, overwrite: true);
		}
		finally
		{
			if (File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
		}
	}

	/// <inheritdoc/>
	public async Task<bool> IsInitializedAsync() => await Task.FromResult(Directory.Exists(_dataDirectory)).ConfigureAwait(false);

	/// <inheritdoc/>
	public async Task InitializeAsync()
	{
		if (!Directory.Exists(_dataDirectory))
		{
			Directory.CreateDirectory(_dataDirectory);
		}

		await Task.CompletedTask.ConfigureAwait(false);
	}

	private async Task EnsureInitializedAsync()
	{
		if (!await IsInitializedAsync().ConfigureAwait(false))
		{
			await InitializeAsync().ConfigureAwait(false);
		}
	}

	// Reading a profile keeps each chord as raw JSON, so a chord that does not match the schema costs only itself
	private sealed class StoredProfileDto
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public Dictionary<string, JsonElement>? Chords { get; set; }
	}

	private sealed class ProfileDto
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public Dictionary<string, ChordDto> Chords { get; set; } = [];
	}

	private sealed class ChordDto
	{
		public List<string> Notes { get; set; } = [];
	}

	private sealed class CommandDto
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public string? Category { get; set; }
	}

	private sealed class ActiveProfileDto
	{
		public string? ActiveProfileId { get; set; }
	}
}
