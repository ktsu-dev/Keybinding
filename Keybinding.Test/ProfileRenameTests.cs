// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ProfileRenameTests
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
	public void RenameProfile_WithoutDescription_KeepsTheExistingDescription()
	{
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Old", "my desc");

		Assert.IsTrue(profiles.RenameProfile("p", "New Name"));

		Profile? renamed = profiles.GetProfile("p");
		Assert.IsNotNull(renamed);
		Assert.AreEqual("New Name", renamed.Name);
		Assert.AreEqual("my desc", renamed.Description, "Leaving out the optional description should not erase it.");
	}

	[TestMethod]
	public void RenameProfile_WithDescription_ReplacesIt()
	{
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Old", "my desc");

		Assert.IsTrue(profiles.RenameProfile("p", " New Name ", " new desc "));

		Profile? renamed = profiles.GetProfile("p");
		Assert.IsNotNull(renamed);
		Assert.AreEqual("New Name", renamed.Name);
		Assert.AreEqual("new desc", renamed.Description);
	}

	[TestMethod]
	public void RenameProfile_UnknownProfile_ReturnsFalse()
	{
		ProfileManager profiles = new();

		Assert.IsFalse(profiles.RenameProfile("missing", "New Name"));
	}

	[TestMethod]
	public void RenameProfile_KeepsHeldReferencesAttached()
	{
		ProfileManager profiles = new();
		Profile held = profiles.CreateProfile("p", "Old", "my desc");

		profiles.RenameProfile("p", "New Name");

		Assert.AreSame(held, profiles.GetProfile("p"), "The renamed profile should be the same object callers already hold.");

		Chord chord = Chord.Parse("Ctrl+A");
		held.SetChord("x.y", chord);
		Assert.AreEqual(chord, profiles.GetProfile("p")!.GetChord("x.y"), "A chord set through a held reference should reach the stored profile.");
	}

	[TestMethod]
	public async Task RenameProfile_ChordsSetThroughAHeldReference_ArePersisted()
	{
		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);

			manager.Commands.RegisterCommand(new Command("x.y", "X Y"));
			Profile? held = manager.CreateDefaultProfile();
			Assert.IsNotNull(held);

			Assert.IsTrue(manager.Profiles.RenameProfile(held.Id, "Renamed"));
			held.SetChord("x.y", manager.Keybindings.ParseChord("Ctrl+T"));

			await manager.SaveAsync().ConfigureAwait(false);
		}

		{
			using KeybindingManager manager = new(_testDataDirectory);
			await manager.InitializeAsync().ConfigureAwait(false);

			Profile? reloaded = manager.Profiles.GetProfile("default");
			Assert.IsNotNull(reloaded);
			Assert.AreEqual("Renamed", reloaded.Name);
			Assert.AreEqual("Ctrl+T", reloaded.GetChord("x.y")?.ToString());
		}
	}
}
