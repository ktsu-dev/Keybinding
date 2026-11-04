// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// A relative data directory is resolved once, when the repository is built, so a later change of the
/// current directory (an OpenFileDialog without RestoreDirectory does this) moves neither the files nor
/// the lock (ktsu-dev/Keybinding#217). These tests change the process-wide current directory, so they
/// must not run alongside other tests.
/// </summary>
[TestClass]
[DoNotParallelize]
public class RelativeDataDirectoryTests
{
	private string _root = null!;
	private string _originalDirectory = null!;

	[TestInitialize]
	public void Setup()
	{
		_originalDirectory = Environment.CurrentDirectory;
		_root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(Path.Combine(_root, "a"));
		Directory.CreateDirectory(Path.Combine(_root, "b"));
	}

	[TestCleanup]
	public void Cleanup()
	{
		Environment.CurrentDirectory = _originalDirectory;
		if (Directory.Exists(_root))
		{
			Directory.Delete(_root, recursive: true);
		}
	}

	[TestMethod]
	public async Task Repository_SavesToTheDirectoryResolvedAtConstruction_AfterTheCurrentDirectoryChanges()
	{
		Environment.CurrentDirectory = Path.Combine(_root, "a");
		JsonKeybindingRepository repository = new("data");
		await repository.SaveProfileAsync(new Profile("first", "First")).ConfigureAwait(false);

		Environment.CurrentDirectory = Path.Combine(_root, "b");
		await repository.SaveProfileAsync(new Profile("second", "Second")).ConfigureAwait(false);

		Assert.IsFalse(Directory.Exists(Path.Combine(_root, "b", "data")), "No save may follow the current directory.");
		JsonKeybindingRepository reader = new(Path.Combine(_root, "a", "data"));
		IReadOnlyCollection<Profile> profiles = await reader.LoadAllProfilesAsync().ConfigureAwait(false);
		Assert.AreEqual("first,second", string.Join(',', profiles.Select(p => p.Id).Order(StringComparer.Ordinal)));
	}

	[TestMethod]
	public async Task Manager_RebindAfterTheCurrentDirectoryChanges_IsKeptInTheOriginalDirectory()
	{
		Environment.CurrentDirectory = Path.Combine(_root, "a");
		using (KeybindingManager manager = new("data"))
		{
			await manager.InitializeAsync().ConfigureAwait(false);
			manager.Commands.RegisterCommand(new Command("copy", "Copy"));
			manager.CreateDefaultProfile();
			manager.Keybindings.BindChord("copy", Chord.Parse("Ctrl+C"));
			await manager.SaveAsync().ConfigureAwait(false);

			Environment.CurrentDirectory = Path.Combine(_root, "b");
			manager.Keybindings.BindChord("copy", Chord.Parse("Ctrl+Shift+C"));
			await manager.SaveAsync().ConfigureAwait(false);
		}

		Assert.IsFalse(Directory.Exists(Path.Combine(_root, "b", "data")), "No save may follow the current directory.");
		Environment.CurrentDirectory = Path.Combine(_root, "a");
		using KeybindingManager restarted = new("data");
		await restarted.InitializeAsync().ConfigureAwait(false);
		Assert.AreEqual(Chord.Parse("Ctrl+Shift+C"), restarted.Keybindings.GetChord("copy"));
	}

	[TestMethod]
	public async Task RepositoriesFromOneRelativePathUnderDifferentCurrentDirectories_WriteToTheirOwnDirectories()
	{
		Environment.CurrentDirectory = Path.Combine(_root, "a");
		JsonKeybindingRepository first = new("data");
		Environment.CurrentDirectory = Path.Combine(_root, "b");
		JsonKeybindingRepository second = new("data");

		await Task.WhenAll(Enumerable.Range(0, 100).Select(i =>
			(i % 2 == 0 ? first : second).SaveProfileAsync(new Profile("p" + i, "P" + i)))).ConfigureAwait(false);

		IReadOnlyCollection<Profile> inA = await new JsonKeybindingRepository(Path.Combine(_root, "a", "data")).LoadAllProfilesAsync().ConfigureAwait(false);
		IReadOnlyCollection<Profile> inB = await new JsonKeybindingRepository(Path.Combine(_root, "b", "data")).LoadAllProfilesAsync().ConfigureAwait(false);
		Assert.AreEqual(50, inA.Count, "Every save through the first repository lands in a/data.");
		Assert.AreEqual(50, inB.Count, "Every save through the second repository lands in b/data.");
	}
}
