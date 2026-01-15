// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Demo;

using ktsu.Keybinding.Core.Models;
using Spectre.Console;

/// <summary>
/// Demonstrates the musical paradigm for keybindings using console output
/// </summary>
public static class MusicalDemo
{
	/// <summary>
	/// Displays a musical paradigm demonstration
	/// </summary>
	public static void ShowMusicalParadigm()
	{
		AnsiConsole.WriteLine("🎵 Musical Keybinding Paradigm Demo");
		AnsiConsole.WriteLine("=====================================");
		AnsiConsole.WriteLine();

		// �� Notes - Individual keys (with semantic typing)
		NoteName escapeKey = NoteName.Create("ESCAPE");
		NoteName enterKey = NoteName.Create("ENTER");
		NoteName rKey = NoteName.Create("R");

		Note escapeNote = new(escapeKey);
		Note enterNote = new(enterKey);
		Note rNote = new(rKey);

		// 🎶 Chords - Key combinations with modifiers
		Chord copyChord = new([new Note("CTRL"), new Note("C")]);  // Ctrl+C
		Chord pasteChord = new([new Note("CTRL"), new Note("V")]); // Ctrl+V
		Chord selectAllChord = new([new Note("CTRL"), new Note("A")]); // Ctrl+A

		// 🎼 Phrases - Sequences of notes/chords (like Ctrl+R, R for rename in VS)
		Phrase renamePhrase = Phrase.FromStrings("Ctrl+R", "R");
		Phrase commentPhrase = Phrase.FromStrings("Ctrl+K", "Ctrl+C");
		Phrase uncommentPhrase = Phrase.FromStrings("Ctrl+K", "Ctrl+U");

		// 📝 Commands with semantic typing
		CommandId copyCommandId = CommandId.Create("edit.copy");
		CommandName copyCommandName = CommandName.Create("Copy to Clipboard");
		CommandDescription copyDescription = CommandDescription.Create("Copies the selected text to the clipboard");
		CommandCategory editCategory = CommandCategory.Create("Edit");

		Command copyCommand = new(copyCommandId, copyCommandName, copyDescription, editCategory);

		// 🎹 Profiles - Collections of phrase-to-command mappings
		ProfileId vsProfileId = ProfileId.Create("visual-studio");
		ProfileName vsProfileName = ProfileName.Create("Visual Studio");

		// Display the musical metaphor
		AnsiConsole.WriteLine($"🎵 Notes (individual keys):");
		AnsiConsole.WriteLine($"   - Escape: {escapeNote}");
		AnsiConsole.WriteLine($"   - Enter: {enterNote}");
		AnsiConsole.WriteLine($"   - R: {rNote}");
		AnsiConsole.WriteLine();

		AnsiConsole.WriteLine($"🎶 Chords (key combinations):");
		AnsiConsole.WriteLine($"   - Copy: {copyChord}");
		AnsiConsole.WriteLine($"   - Paste: {pasteChord}");
		AnsiConsole.WriteLine($"   - Select All: {selectAllChord}");
		AnsiConsole.WriteLine();

		AnsiConsole.WriteLine($"🎼 Phrases (sequences):");
		AnsiConsole.WriteLine($"   - Rename: {renamePhrase}");
		AnsiConsole.WriteLine($"   - Comment: {commentPhrase}");
		AnsiConsole.WriteLine($"   - Uncomment: {uncommentPhrase}");
		AnsiConsole.WriteLine();

		AnsiConsole.WriteLine($"📝 Commands (actions):");
		AnsiConsole.WriteLine($"   - {copyCommand.Id}: {copyCommand.Name}");
		AnsiConsole.WriteLine($"     Category: {copyCommand.Category}");
		AnsiConsole.WriteLine($"     Description: {copyCommand.Description}");
		AnsiConsole.WriteLine();

		AnsiConsole.WriteLine($"🎹 Profile: {vsProfileName} ({vsProfileId})");
		AnsiConsole.WriteLine("   Maps phrases to commands in a specific context");
		AnsiConsole.WriteLine();

		ShowPhrasePatterns();
	}

	/// <summary>
	/// Shows examples of different phrase patterns commonly used in editors
	/// </summary>
	public static void ShowPhrasePatterns()
	{
		AnsiConsole.WriteLine("🎼 Common Phrase Patterns");
		AnsiConsole.WriteLine("=========================");
		AnsiConsole.WriteLine();

		// Single chord phrases (most common)
		AnsiConsole.WriteLine("Single Chord Phrases:");
		AnsiConsole.WriteLine($"   Copy: {Phrase.Parse("Ctrl+C")}");
		AnsiConsole.WriteLine($"   Paste: {Phrase.Parse("Ctrl+V")}");
		AnsiConsole.WriteLine($"   Save: {Phrase.Parse("Ctrl+S")}");
		AnsiConsole.WriteLine();

		// Sequential chord phrases (VS Code style)
		AnsiConsole.WriteLine("Sequential Chord Phrases:");
		AnsiConsole.WriteLine($"   Rename: {Phrase.Parse("Ctrl+R, R")}");
		AnsiConsole.WriteLine($"   Format Document: {Phrase.Parse("Ctrl+K, Ctrl+D")}");
		AnsiConsole.WriteLine($"   Comment Lines: {Phrase.Parse("Ctrl+K, Ctrl+C")}");
		AnsiConsole.WriteLine();

		// Vim-style note sequences
		AnsiConsole.WriteLine("Note Sequence Phrases (Vim-style):");
		AnsiConsole.WriteLine($"   Go to top: {Phrase.Parse("G, G")}");
		AnsiConsole.WriteLine($"   Delete word: {Phrase.Parse("D, W")}");
		AnsiConsole.WriteLine($"   Change inner word: {Phrase.Parse("C, I, W")}");
		AnsiConsole.WriteLine();

		// Mixed patterns
		AnsiConsole.WriteLine("Mixed Patterns:");
		AnsiConsole.WriteLine($"   Quick Open: {Phrase.Parse("Ctrl+P")}");
		AnsiConsole.WriteLine($"   Command Palette: {Phrase.Parse("Ctrl+Shift+P")}");
		AnsiConsole.WriteLine($"   Multi-cursor: {Phrase.Parse("Ctrl+Alt+Down")}");
		AnsiConsole.WriteLine();
	}
}
