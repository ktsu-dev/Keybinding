// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;

[TestClass]
public class InvalidStoredCommandEntryTests
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

	private async Task WriteCommandsAsync(string json) =>
		await File.WriteAllTextAsync(Path.Combine(_testDataDirectory, "commands.json"), json).ConfigureAwait(false);

	[TestMethod]
	public async Task InitializeAsync_CorruptCommandsFile_LoadsNoCommands()
	{
		await WriteCommandsAsync("{not json").ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Assert.IsEmpty(manager.Commands.GetAllCommands());
	}

	[TestMethod]
	[DataRow("""[{"id":"file.save","name":"Save"},{"id":"file.open","name":""}]""", DisplayName = "Blank name")]
	[DataRow("""[{"id":"file.save","name":"Save"},{"name":"Open"}]""", DisplayName = "Missing id")]
	[DataRow("""[{"id":"file.save","name":"Save"},null]""", DisplayName = "Null entry")]
	public async Task InitializeAsync_InvalidCommandEntry_SkipsOnlyThatEntry(string json)
	{
		await WriteCommandsAsync(json).ConfigureAwait(false);

		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);

		Assert.AreEqual("file.save", string.Join(',', manager.Commands.GetAllCommands().Select(c => c.Id)));
	}

	[TestMethod]
	public async Task SaveAsync_AfterSkippingInvalidEntries_KeepsTheValidOnes()
	{
		await WriteCommandsAsync("""[{"id":"file.save","name":"Save"},null,{"id":"file.open","name":""}]""").ConfigureAwait(false);

		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);
			await manager.SaveAsync().ConfigureAwait(false);
		}

		using KeybindingManager reloaded = new(_testDataDirectory);
		await reloaded.InitializeAsync().ConfigureAwait(false);

		Assert.AreEqual("file.save", string.Join(',', reloaded.Commands.GetAllCommands().Select(c => c.Id)));
	}
}
