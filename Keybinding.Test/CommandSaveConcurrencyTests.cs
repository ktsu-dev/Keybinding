// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class CommandSaveConcurrencyTests
{
	private string _testDataDirectory = null!;

	[TestInitialize]
	public void Setup()
	{
		_testDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testDataDirectory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_testDataDirectory))
		{
			Directory.Delete(_testDataDirectory, recursive: true);
		}
	}

	[TestMethod]
	public async Task SaveAsync_AnotherManagerSavesBetweenReadAndWrite_KeepsBothCommands()
	{
		InterleavingRepository repository = new(_testDataDirectory);
		using KeybindingManager first = new(new CommandRegistry(), new ProfileManager(), repository);
		using KeybindingManager second = new(_testDataDirectory);
		await first.InitializeAsync().ConfigureAwait(false);
		await second.InitializeAsync().ConfigureAwait(false);

		first.Commands.RegisterCommand(new Command("file.save", "Save"));
		second.Commands.RegisterCommand(new Command("file.open", "Open"));

		// The second manager saves straight after the first reads the stored commands, which is the interleaving
		// that lost a command when the read and the write were separate repository calls
		repository.AfterNextCommandsRead = second.SaveAsync;
		await first.SaveAsync().ConfigureAwait(false);
		await repository.Interleaved.ConfigureAwait(false);

		IReadOnlyCollection<Command> stored = await new JsonKeybindingRepository(_testDataDirectory).LoadCommandsAsync().ConfigureAwait(false);
		Assert.AreEqual("file.open,file.save", string.Join(',', stored.Select(c => c.Id.ToString()).Order(StringComparer.Ordinal)));
	}

	[TestMethod]
	public async Task UpdateCommandsAsync_ConcurrentUpdates_KeepEveryCommand()
	{
		JsonKeybindingRepository[] repositories = [.. Enumerable.Range(0, 8).Select(_ => new JsonKeybindingRepository(_testDataDirectory))];

		await Task.WhenAll(repositories.Select((repository, i) => Task.Run(() =>
			repository.UpdateCommandsAsync(stored => [.. stored, new Command($"command.{i}", $"Command {i}")])))).ConfigureAwait(false);

		IReadOnlyCollection<Command> commands = await repositories[0].LoadCommandsAsync().ConfigureAwait(false);
		Assert.HasCount(8, commands);
	}

	[TestMethod]
	public async Task UpdateCommandsAsync_DefaultImplementation_StoresWhatTheUpdateReturns()
	{
		IKeybindingRepository repository = new LoadThenSaveRepository(_testDataDirectory);
		await repository.SaveCommandsAsync([new Command("file.save", "Save")]).ConfigureAwait(false);

		IReadOnlyCollection<Command> saved = await repository.UpdateCommandsAsync(stored => [.. stored, new Command("file.open", "Open")]).ConfigureAwait(false);

		IReadOnlyCollection<Command> stored = await repository.LoadCommandsAsync().ConfigureAwait(false);
		Assert.AreEqual("file.open,file.save", string.Join(',', saved.Select(c => c.Id.ToString()).Order(StringComparer.Ordinal)));
		Assert.AreEqual("file.open,file.save", string.Join(',', stored.Select(c => c.Id.ToString()).Order(StringComparer.Ordinal)));
	}

	[TestMethod]
	public async Task UpdateCommandsAsync_NullUpdate_Throws()
	{
		IKeybindingRepository defaulted = new LoadThenSaveRepository(_testDataDirectory);
		JsonKeybindingRepository json = new(_testDataDirectory);

		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => defaulted.UpdateCommandsAsync(null!)).ConfigureAwait(false);
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => json.UpdateCommandsAsync(null!)).ConfigureAwait(false);
	}

	// Leaves UpdateCommandsAsync to the interface's default, as a repository written before it existed does
	private sealed class LoadThenSaveRepository(string directory) : IKeybindingRepository
	{
		private readonly JsonKeybindingRepository _inner = new(directory);

		public Task SaveProfileAsync(Profile profile) => _inner.SaveProfileAsync(profile);
		public Task<Profile?> LoadProfileAsync(string profileId) => _inner.LoadProfileAsync(profileId);
		public Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync() => _inner.LoadAllProfilesAsync();
		public Task DeleteProfileAsync(string profileId) => _inner.DeleteProfileAsync(profileId);
		public Task SaveCommandsAsync(IEnumerable<Command> commands) => _inner.SaveCommandsAsync(commands);
		public Task<IReadOnlyCollection<Command>> LoadCommandsAsync() => _inner.LoadCommandsAsync();
		public Task SaveActiveProfileAsync(string? profileId) => _inner.SaveActiveProfileAsync(profileId);
		public Task<string?> LoadActiveProfileAsync() => _inner.LoadActiveProfileAsync();
		public Task<bool> IsInitializedAsync() => _inner.IsInitializedAsync();
		public Task InitializeAsync() => _inner.InitializeAsync();
	}

	// Starts a callback straight after the next read of the stored commands, standing in for another manager
	// saving at that moment. A plain read waits for it, so it lands before the write that follows; a read inside
	// an update cannot, because the update holds the lock the callback's save needs.
	private sealed class InterleavingRepository(string directory) : IKeybindingRepository
	{
		private readonly JsonKeybindingRepository _inner = new(directory);

		public Func<Task>? AfterNextCommandsRead { get; set; }

		public Task Interleaved { get; private set; } = Task.CompletedTask;

		public async Task<IReadOnlyCollection<Command>> LoadCommandsAsync()
		{
			IReadOnlyCollection<Command> commands = await _inner.LoadCommandsAsync().ConfigureAwait(false);
			StartCallback();
			await Interleaved.ConfigureAwait(false);
			return commands;
		}

		public Task<IReadOnlyCollection<Command>> UpdateCommandsAsync(Func<IReadOnlyCollection<Command>, IEnumerable<Command>> update) =>
			_inner.UpdateCommandsAsync(stored =>
			{
				StartCallback();
				return update(stored);
			});

		private void StartCallback()
		{
			if (AfterNextCommandsRead is { } callback)
			{
				AfterNextCommandsRead = null;
				Interleaved = Task.Run(callback);
			}
		}

		public Task SaveProfileAsync(Profile profile) => _inner.SaveProfileAsync(profile);
		public Task<Profile?> LoadProfileAsync(string profileId) => _inner.LoadProfileAsync(profileId);
		public Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync() => _inner.LoadAllProfilesAsync();
		public Task DeleteProfileAsync(string profileId) => _inner.DeleteProfileAsync(profileId);
		public Task SaveCommandsAsync(IEnumerable<Command> commands) => _inner.SaveCommandsAsync(commands);
		public Task SaveActiveProfileAsync(string? profileId) => _inner.SaveActiveProfileAsync(profileId);
		public Task<string?> LoadActiveProfileAsync() => _inner.LoadActiveProfileAsync();
		public Task<bool> IsInitializedAsync() => _inner.IsInitializedAsync();
		public Task InitializeAsync() => _inner.InitializeAsync();
	}
}
