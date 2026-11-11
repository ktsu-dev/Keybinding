// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// A save replaces each data file atomically, but must leave the file the user set up as it was: its permissions,
/// and a symlink pointing somewhere else.
/// </summary>
[TestClass]
public class RepositoryFileAttributeTests
{
	private static readonly string[] DataFiles = ["profiles.json", "commands.json", "active-profile.json"];

	private string _testDataDirectory = null!;
	private string _elsewhere = null!;

	[TestInitialize]
	public void Setup()
	{
		_testDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_elsewhere = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testDataDirectory);
		Directory.CreateDirectory(_elsewhere);
	}

	[TestCleanup]
	public void Cleanup()
	{
		foreach (string directory in new[] { _testDataDirectory, _elsewhere })
		{
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, recursive: true);
			}
		}
	}

	private string PathOf(string fileName) => Path.Combine(_testDataDirectory, fileName);

	private async Task SaveWithChordAsync(string chord)
	{
		using KeybindingManager manager = new(_testDataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		manager.Commands.RegisterCommand(new Command("file.save", "Save"));
		Profile? profile = manager.Profiles.GetProfile("default") ?? manager.CreateDefaultProfile();
		Assert.IsNotNull(profile);
		profile.SetChord("file.save", Chord.Parse(chord));
		await manager.SaveAsync().ConfigureAwait(false);
	}

	[TestMethod]
	public async Task Save_KeepsTheUnixModeOfExistingFiles()
	{
		if (OperatingSystem.IsWindows())
		{
			Assert.Inconclusive("Unix file modes do not apply on Windows.");
			return;
		}

		await SaveWithChordAsync("Ctrl+S").ConfigureAwait(false);
		const UnixFileMode ownerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite;
		foreach (string file in DataFiles)
		{
			File.SetUnixFileMode(PathOf(file), ownerOnly);
		}

		await SaveWithChordAsync("Ctrl+Shift+S").ConfigureAwait(false);

		foreach (string file in DataFiles)
		{
			Assert.AreEqual(ownerOnly, File.GetUnixFileMode(PathOf(file)), file);
		}

		StringAssert.Contains(await File.ReadAllTextAsync(PathOf("profiles.json")).ConfigureAwait(false), "SHIFT");
	}

	[TestMethod]
	public async Task Save_WritesThroughASymlinkedDataFile()
	{
		await SaveWithChordAsync("Ctrl+S").ConfigureAwait(false);
		string target = Path.Combine(_elsewhere, "profiles.json");
		File.Move(PathOf("profiles.json"), target);
		try
		{
			File.CreateSymbolicLink(PathOf("profiles.json"), target);
		}
		catch (IOException ex)
		{
			Assert.Inconclusive($"Cannot create a symlink here: {ex.Message}");
			return;
		}
		catch (UnauthorizedAccessException ex)
		{
			Assert.Inconclusive($"Cannot create a symlink here: {ex.Message}");
			return;
		}

		await SaveWithChordAsync("Ctrl+Shift+S").ConfigureAwait(false);

		Assert.IsNotNull(new FileInfo(PathOf("profiles.json")).LinkTarget, "profiles.json is still a symlink.");
		StringAssert.Contains(await File.ReadAllTextAsync(target).ConfigureAwait(false), "SHIFT");
		Assert.IsEmpty(Directory.GetFiles(_elsewhere, "*.tmp"));
		Assert.IsEmpty(Directory.GetFiles(_testDataDirectory, "*.tmp"));
	}

	[TestMethod]
	public async Task Save_CreatesMissingFiles()
	{
		JsonKeybindingRepository repository = new(_testDataDirectory);
		await repository.SaveActiveProfileAsync("a").ConfigureAwait(false);

		Assert.AreEqual("a", await repository.LoadActiveProfileAsync().ConfigureAwait(false));
		Assert.IsEmpty(Directory.GetFiles(_testDataDirectory, "*.tmp"));
	}
}
