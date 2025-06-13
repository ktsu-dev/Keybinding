// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

/// <summary>
/// JSON file-based implementation of keybinding repository
/// </summary>
public sealed class JsonKeybindingRepository : IKeybindingRepository
{
	private readonly string _dataDirectory;
	private readonly JsonSerializerOptions _jsonOptions;

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
		_jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new JsonStringEnumConverter() }
		};
	}

	/// <inheritdoc/>
	public async Task SaveProfileAsync(Profile profile)
	{
		ArgumentNullException.ThrowIfNull(profile);

		await EnsureInitializedAsync().ConfigureAwait(false);

		List<Profile> profiles = await LoadAllProfilesInternalAsync().ConfigureAwait(false);
		List<Profile> profileList = [.. profiles];

		// Remove existing profile with same ID if it exists
		profileList.RemoveAll(p => p.Id == profile.Id);
		profileList.Add(profile);

		string profilesPath = Path.Combine(_dataDirectory, ProfilesFileName);
		IEnumerable<ProfileDto> profileDtos = profileList.Select(p => new ProfileDto
		{
			Id = p.Id,
			Name = p.Name,
			Description = p.Description,
			Chords = p.Chords.ToDictionary(
				kvp => kvp.Key,
				kvp => new ChordDto
				{
					Notes = [.. kvp.Value.Notes.Select(n => n.ToString())]
				})
		});

		string json = JsonSerializer.Serialize(profileDtos, _jsonOptions);
		await File.WriteAllTextAsync(profilesPath, json).ConfigureAwait(false);
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
	public async Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync() => await LoadAllProfilesInternalAsync().ConfigureAwait(false);

	private async Task<List<Profile>> LoadAllProfilesInternalAsync()
	{
		await EnsureInitializedAsync().ConfigureAwait(false);

		string profilesPath = Path.Combine(_dataDirectory, ProfilesFileName);
		if (!File.Exists(profilesPath))
		{
			return [];
		}

		try
		{
			string json = await File.ReadAllTextAsync(profilesPath).ConfigureAwait(false);
			List<ProfileDto> profileDtos = JsonSerializer.Deserialize<List<ProfileDto>>(json, _jsonOptions) ?? [];

			return [.. profileDtos.Select(dto =>
			{
				Profile profile = new(dto.Id, dto.Name, dto.Description);
				foreach (KeyValuePair<string, ChordDto> kvp in dto.Chords ?? [])
				{
					Chord chord = new(kvp.Value.Notes.Select(noteString => new Note(noteString)));
					profile.SetChord(kvp.Key, chord);
				}

				return profile;
			})];
		}
		catch (JsonException)
		{
			// If JSON is corrupted, return empty list
			return [];
		}
	}

	/// <inheritdoc/>
	public async Task DeleteProfileAsync(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
		{
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));
		}

		List<Profile> profiles = await LoadAllProfilesInternalAsync().ConfigureAwait(false);
		profiles.RemoveAll(p => p.Id == profileId.Trim());

		string profilesPath = Path.Combine(_dataDirectory, ProfilesFileName);
		IEnumerable<ProfileDto> profileDtos = profiles.Select(p => new ProfileDto
		{
			Id = p.Id,
			Name = p.Name,
			Description = p.Description,
			Chords = p.Chords.ToDictionary(
				kvp => kvp.Key,
				kvp => new ChordDto
				{
					Notes = [.. kvp.Value.Notes.Select(n => n.ToString())]
				})
		});

		string json = JsonSerializer.Serialize(profileDtos, _jsonOptions);
		await File.WriteAllTextAsync(profilesPath, json).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task SaveCommandsAsync(IEnumerable<Command> commands)
	{
		ArgumentNullException.ThrowIfNull(commands);

		await EnsureInitializedAsync().ConfigureAwait(false);

		string commandsPath = Path.Combine(_dataDirectory, CommandsFileName);
		IEnumerable<CommandDto> commandDtos = commands.Select(c => new CommandDto
		{
			Id = c.Id,
			Name = c.Name,
			Description = c.Description,
			Category = c.Category
		});

		string json = JsonSerializer.Serialize(commandDtos, _jsonOptions);
		await File.WriteAllTextAsync(commandsPath, json).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<IReadOnlyCollection<Command>> LoadCommandsAsync()
	{
		await EnsureInitializedAsync().ConfigureAwait(false);

		string commandsPath = Path.Combine(_dataDirectory, CommandsFileName);
		if (!File.Exists(commandsPath))
		{
			return new List<Command>().AsReadOnly();
		}

		try
		{
			string json = await File.ReadAllTextAsync(commandsPath).ConfigureAwait(false);
			List<CommandDto> commandDtos = JsonSerializer.Deserialize<List<CommandDto>>(json, _jsonOptions) ?? [];

			return commandDtos
				.Select(dto => new Command(dto.Id, dto.Name, dto.Description, dto.Category))
				.ToList()
				.AsReadOnly();
		}
		catch (JsonException)
		{
			// If JSON is corrupted, return empty list
			return new List<Command>().AsReadOnly();
		}
	}

	/// <inheritdoc/>
	public async Task SaveActiveProfileAsync(string? profileId)
	{
		await EnsureInitializedAsync().ConfigureAwait(false);

		string activeProfilePath = Path.Combine(_dataDirectory, ActiveProfileFileName);
		ActiveProfileDto activeProfileDto = new() { ActiveProfileId = profileId };

		string json = JsonSerializer.Serialize(activeProfileDto, _jsonOptions);
		await File.WriteAllTextAsync(activeProfilePath, json).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<string?> LoadActiveProfileAsync()
	{
		await EnsureInitializedAsync().ConfigureAwait(false);

		string activeProfilePath = Path.Combine(_dataDirectory, ActiveProfileFileName);
		if (!File.Exists(activeProfilePath))
		{
			return null;
		}

		try
		{
			string json = await File.ReadAllTextAsync(activeProfilePath).ConfigureAwait(false);
			ActiveProfileDto? activeProfileDto = JsonSerializer.Deserialize<ActiveProfileDto>(json, _jsonOptions);

			return activeProfileDto?.ActiveProfileId;
		}
		catch (JsonException)
		{
			// If JSON is corrupted, return null
			return null;
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
