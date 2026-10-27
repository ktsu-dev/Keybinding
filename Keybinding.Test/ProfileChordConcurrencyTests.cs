// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Globalization;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ProfileChordConcurrencyTests
{
	private const int CommandCount = 20_000;

	[TestMethod]
	public void BindUnbindAndFind_CalledInParallel_KeepEveryBindingAndDoNotThrow()
	{
		CommandRegistry registry = new();
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Profile");
		profiles.SetActiveProfile("p");
		KeybindingService service = new(registry, profiles);

		for (int i = 0; i < CommandCount; i++)
		{
			registry.RegisterCommand(new Command($"c{i}", $"C{i}"));
		}

		// Odd commands stay bound, even ones are unbound again, and reads race the writes. Each command gets its
		// own chord, since binding a chord another command uses moves it.
		Parallel.For(0, CommandCount, i =>
		{
			string commandId = $"c{i}";
			Chord chord = Chord.Parse($"Ctrl+K{i}");
			Assert.IsTrue(service.BindChord(commandId, chord));
			_ = service.FindCommandByChord(chord);
			_ = service.GetAllChords();

			if (i % 2 == 0)
			{
				Assert.IsTrue(service.UnbindChord(commandId));
			}
		});

		Profile profile = profiles.GetProfile("p")!;
		Assert.AreEqual(CommandCount / 2, profile.ChordCount);
		Assert.HasCount(CommandCount / 2, service.GetAllChords());
		Assert.IsTrue(service.GetAllChords().Keys.All(id => int.Parse(id[1..], CultureInfo.InvariantCulture) % 2 == 1));
	}

	[TestMethod]
	public void BindChord_SameChordInParallel_LeavesExactlyOneCommandBound()
	{
		CommandRegistry registry = new();
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Profile");
		profiles.SetActiveProfile("p");
		KeybindingService service = new(registry, profiles);

		for (int i = 0; i < CommandCount; i++)
		{
			registry.RegisterCommand(new Command($"c{i}", $"C{i}"));
		}

		Chord chord = Chord.Parse("Ctrl+K");
		Parallel.For(0, CommandCount, i => Assert.IsTrue(service.BindChord($"c{i}", chord)));

		Assert.AreEqual(1, profiles.GetProfile("p")!.ChordCount, "Concurrent binds of one chord must not both keep it.");
	}
}
