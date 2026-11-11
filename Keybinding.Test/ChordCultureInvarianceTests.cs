// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Globalization;
using ktsu.Keybinding.Core.Models;

[TestClass]
[DoNotParallelize]
public class ChordCultureInvarianceTests
{
	// et-EE sorts Z before T, and lt-LT sorts Y before J, so a culture-sensitive sort orders these pairs
	// differently from en-US.
	[TestMethod]
	[DataRow("Ctrl+T+Z", "et-EE")]
	[DataRow("Ctrl+J+Y", "lt-LT")]
	public void HashCodeAndSetLookupDoNotDependOnTheThreadCulture(string text, string otherCulture)
	{
		CultureInfo original = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US");
			Chord chord = Chord.Parse(text);
			HashSet<Chord> set = [chord];
			Dictionary<Chord, string> map = new() { [chord] = "handler" };
			int hash = chord.GetHashCode();

			CultureInfo.CurrentCulture = new CultureInfo(otherCulture);
			Chord reparsed = Chord.Parse(text);

			Assert.AreEqual(hash, chord.GetHashCode());
			Assert.AreEqual(hash, reparsed.GetHashCode());
			Assert.IsTrue(set.Contains(chord));
			Assert.IsTrue(set.Contains(reparsed));
			Assert.IsTrue(map.TryGetValue(reparsed, out string? handler));
			Assert.AreEqual("handler", handler);
		}
		finally
		{
			CultureInfo.CurrentCulture = original;
		}
	}

	[TestMethod]
	[DataRow("Ctrl+T+Z", "et-EE")]
	[DataRow("Ctrl+J+Y", "lt-LT")]
	public void StoredNoteOrderDoesNotDependOnTheThreadCulture(string text, string otherCulture)
	{
		CultureInfo original = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US");
			Chord english = Chord.Parse(text);

			CultureInfo.CurrentCulture = new CultureInfo(otherCulture);
			Chord other = Chord.Parse(text);

			CollectionAssert.AreEqual(english.Notes.ToList(), other.Notes.ToList());
			Assert.AreEqual(english.ToString(), other.ToString());
			Assert.AreEqual(text, other.ToString());
		}
		finally
		{
			CultureInfo.CurrentCulture = original;
		}
	}
}
