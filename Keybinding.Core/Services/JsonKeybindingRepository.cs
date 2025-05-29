// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

namespace ktsu.Keybinding.Core.Services;

/// <summary>
/// JSON file-based implementation of keybinding repository
/// </summary>
public sealed class JsonKeybindingRepository : IKeybindingRepository
{
	private readonly string _dataDirectory;
	private readonly JsonSerializerOptions _jsonOptions;

	private const string ProfilesFileName = "profiles.json";
	private const string CommandsFileName = "commands.json";
	private const string ActiveProfileFileName = "active-profile.json";

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonKeybindingRepository"/> class
	/// </summary>
	/// <param name="dataDirectory">The directory to store data files</param>
	public JsonKeybindingRepository(string dataDirectory)
	{
		if (string.IsNullOrWhiteSpace(dataDirectory))
			throw new ArgumentException("Data directory cannot be null or whitespace", nameof(dataDirectory));

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

		await EnsureInitializedAsync();

		var profiles = await LoadAllProfilesInternalAsync();
		var profileList = profiles.ToList();

		// Remove existing profile with same ID if it exists
		profileList.RemoveAll(p => p.Id == profile.Id);
		profileList.Add(profile);

		var profilesPath = Path.Combine(_dataDirectory, ProfilesFileName);
		var profileDtos = profileList.Select(p => new ProfileDto
		{
			Id = p.Id,
			Name = p.Name,
			Description = p.Description,
			Keybindings = p.Keybindings.ToDictionary(
				kvp => kvp.Key,
				kvp => new KeyCombinationDto
				{
					Key = kvp.Value.Key,
					Modifiers = kvp.Value.Modifiers
				})
		});

		var json = JsonSerializer.Serialize(profileDtos, _jsonOptions);
		await File.WriteAllTextAsync(profilesPath, json);
	}

	/// <inheritdoc/>
	public async Task<Profile?> LoadProfileAsync(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		var profiles = await LoadAllProfilesAsync();
		return profiles.FirstOrDefault(p => p.Id == profileId.Trim());
	}

	/// <inheritdoc/>
	public async Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync()
	{
		return await LoadAllProfilesInternalAsync();
	}

	private async Task<List<Profile>> LoadAllProfilesInternalAsync()
	{
		await EnsureInitializedAsync();

		var profilesPath = Path.Combine(_dataDirectory, ProfilesFileName);
		if (!File.Exists(profilesPath))
			return new List<Profile>();

		try
		{
			var json = await File.ReadAllTextAsync(profilesPath);
			var profileDtos = JsonSerializer.Deserialize<List<ProfileDto>>(json, _jsonOptions) ?? new List<ProfileDto>();

			return profileDtos.Select(dto =>
			{
				var profile = new Profile(dto.Id, dto.Name, dto.Description);
				foreach (var kvp in dto.Keybindings ?? new Dictionary<string, KeyCombinationDto>())
				{
					var keyCombination = new KeyCombination(kvp.Value.Key, kvp.Value.Modifiers);
					profile.SetKeybinding(kvp.Key, keyCombination);
				}
				return profile;
			}).ToList();
		}
		catch (JsonException)
		{
			// If JSON is corrupted, return empty list
			return new List<Profile>();
		}
	}

	/// <inheritdoc/>
	public async Task DeleteProfileAsync(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			throw new ArgumentException("Profile ID cannot be null or whitespace", nameof(profileId));

		var profiles = await LoadAllProfilesInternalAsync();
		profiles.RemoveAll(p => p.Id == profileId.Trim());

		var profilesPath = Path.Combine(_dataDirectory, ProfilesFileName);
		var profileDtos = profiles.Select(p => new ProfileDto
		{
			Id = p.Id,
			Name = p.Name,
			Description = p.Description,
			Keybindings = p.Keybindings.ToDictionary(
				kvp => kvp.Key,
				kvp => new KeyCombinationDto
				{
					Key = kvp.Value.Key,
					Modifiers = kvp.Value.Modifiers
				})
		});

		var json = JsonSerializer.Serialize(profileDtos, _jsonOptions);
		await File.WriteAllTextAsync(profilesPath, json);
	}

	/// <inheritdoc/>
	public async Task SaveCommandsAsync(IEnumerable<Command> commands)
	{
		ArgumentNullException.ThrowIfNull(commands);

		await EnsureInitializedAsync();

		var commandsPath = Path.Combine(_dataDirectory, CommandsFileName);
		var commandDtos = commands.Select(c => new CommandDto
		{
			Id = c.Id,
			Name = c.Name,
			Description = c.Description,
			Category = c.Category
		});

		var json = JsonSerializer.Serialize(commandDtos, _jsonOptions);
		await File.WriteAllTextAsync(commandsPath, json);
	}

	/// <inheritdoc/>
	public async Task<IReadOnlyCollection<Command>> LoadCommandsAsync()
	{
		await EnsureInitializedAsync();

		var commandsPath = Path.Combine(_dataDirectory, CommandsFileName);
		if (!File.Exists(commandsPath))
			return new List<Command>().AsReadOnly();

		try
		{
			var json = await File.ReadAllTextAsync(commandsPath);
			var commandDtos = JsonSerializer.Deserialize<List<CommandDto>>(json, _jsonOptions) ?? new List<CommandDto>();

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
		await EnsureInitializedAsync();

		var activeProfilePath = Path.Combine(_dataDirectory, ActiveProfileFileName);
		var dto = new ActiveProfileDto { ActiveProfileId = profileId };

		var json = JsonSerializer.Serialize(dto, _jsonOptions);
		await File.WriteAllTextAsync(activeProfilePath, json);
	}

	/// <inheritdoc/>
	public async Task<string?> LoadActiveProfileAsync()
	{
		await EnsureInitializedAsync();

		var activeProfilePath = Path.Combine(_dataDirectory, ActiveProfileFileName);
		if (!File.Exists(activeProfilePath))
			return null;

		try
		{
			var json = await File.ReadAllTextAsync(activeProfilePath);
			var dto = JsonSerializer.Deserialize<ActiveProfileDto>(json, _jsonOptions);
			return dto?.ActiveProfileId;
		}
		catch (JsonException)
		{
			// If JSON is corrupted, return null
			return null;
		}
	}

	/// <inheritdoc/>
	public async Task<bool> IsInitializedAsync()
	{
		return await Task.FromResult(Directory.Exists(_dataDirectory));
	}

	/// <inheritdoc/>
	public async Task InitializeAsync()
	{
		await Task.Run(() =>
		{
			if (!Directory.Exists(_dataDirectory))
			{
				Directory.CreateDirectory(_dataDirectory);
			}
		});
	}

	private async Task EnsureInitializedAsync()
	{
		if (!await IsInitializedAsync())
		{
			await InitializeAsync();
		}
	}

	// DTOs for serialization
	private sealed class ProfileDto
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public Dictionary<string, KeyCombinationDto> Keybindings { get; set; } = new();
	}

	private sealed class KeyCombinationDto
	{
		public string Key { get; set; } = string.Empty;
		public ModifierKeys Modifiers { get; set; }
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
