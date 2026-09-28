// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ProfileChordAccessTests
{
	private static readonly Chord CtrlA = Chord.Parse("Ctrl+A");
	private static readonly Chord CtrlB = Chord.Parse("Ctrl+B");
	private static readonly string[] BothCommands = ["a", "b"];

	[TestMethod]
	public void ChordAccessors_ReflectSetRemoveAndClear()
	{
		Profile profile = new("p", "Profile");

		profile.SetChord(" a ", CtrlA);
		profile.SetChord("b", CtrlB);

		Assert.AreEqual(2, profile.ChordCount);
		Assert.AreEqual(CtrlA, profile.GetChord("a"));
		Assert.IsNull(profile.GetChord("missing"));
		Assert.IsTrue(profile.HasChord(" a "));
		Assert.IsFalse(profile.HasChord("missing"));
		CollectionAssert.AreEquivalent(BothCommands, profile.BoundCommands.ToArray());

		Assert.IsTrue(profile.RemoveChord("a"));
		Assert.IsFalse(profile.RemoveChord("a"));
		Assert.AreEqual(1, profile.ChordCount);

		profile.ClearChords();
		Assert.AreEqual(0, profile.ChordCount);
		Assert.IsEmpty(profile.GetAllChords());
	}

	[TestMethod]
	public void ChordAccessors_RejectBlankCommandIds()
	{
		Profile profile = new("p", "Profile");

		Assert.ThrowsExactly<ArgumentException>(() => profile.SetChord(" ", CtrlA));
		Assert.ThrowsExactly<ArgumentException>(() => profile.GetChord(" "));
		Assert.ThrowsExactly<ArgumentException>(() => profile.HasChord(" "));
		Assert.ThrowsExactly<ArgumentException>(() => profile.RemoveChord(" "));
	}

	[TestMethod]
	public void ObsoleteChords_StillExposesTheLiveBindings()
	{
		Profile profile = new("p", "Profile");
		profile.SetChord("a", CtrlA);

#pragma warning disable CS0618 // Type or member is obsolete
		Dictionary<string, Chord> chords = profile.Chords;
#pragma warning restore CS0618 // Type or member is obsolete

		Assert.HasCount(1, chords);
		profile.SetChord("b", CtrlB);
		Assert.HasCount(2, chords, "Chords keeps returning the live dictionary for existing callers");
	}

	[TestMethod]
	public void FindAndExecuteChord_UseTheLockedSnapshot()
	{
		CommandRegistry registry = new();
		ProfileManager profiles = new();
		profiles.CreateProfile("p", "Profile");
		profiles.SetActiveProfile("p");
		KeybindingService service = new(registry, profiles);
		registry.RegisterCommand(new Command("a", "A"));
		registry.RegisterCommand(new Command("b", "B"));

		Assert.IsTrue(service.BindChord("a", CtrlA));
		Assert.IsTrue(service.BindChord("b", CtrlB));

		Assert.AreEqual("a", service.FindCommandByChord(CtrlA));
		Assert.IsNull(service.FindCommandByChord(Chord.Parse("Ctrl+C")));
		Assert.AreEqual("b", service.ExecuteChord(CtrlB));

		registry.UnregisterCommand("b");
		Assert.IsNull(service.ExecuteChord(CtrlB), "A binding whose command is unregistered is skipped");
		Assert.AreEqual("b", service.FindCommandByChord(CtrlB), "FindCommandByChord does not filter by registration");
	}
}
