// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

/// <summary>
/// Summary information about the current keybinding state
/// </summary>
public sealed class KeybindingSummary
{
	/// <summary>
	/// Gets or sets the total number of registered commands
	/// </summary>
	public int TotalCommands { get; set; }

	/// <summary>
	/// Gets or sets the total number of profiles
	/// </summary>
	public int TotalProfiles { get; set; }

	/// <summary>
	/// Gets or sets the active profile ID
	/// </summary>
	public string? ActiveProfileId { get; set; }

	/// <summary>
	/// Gets or sets the active profile name
	/// </summary>
	public string? ActiveProfileName { get; set; }

	/// <summary>
	/// Gets or sets the number of keybindings in the active profile
	/// </summary>
	public int ActiveKeybindings { get; set; }
}
