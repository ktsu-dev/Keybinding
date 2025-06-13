// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Configuration interface for keybinding settings
/// </summary>
public interface IKeybindingConfiguration
{
	/// <summary>
	/// Gets the data directory for storing keybinding files
	/// </summary>
	public string DataDirectory { get; }

	/// <summary>
	/// Gets the default profile ID to use when no profile is active
	/// </summary>
	public string DefaultProfileId { get; }

	/// <summary>
	/// Gets the default profile name
	/// </summary>
	public string DefaultProfileName { get; }

	/// <summary>
	/// Gets a value indicating whether to auto-save changes
	/// </summary>
	public bool AutoSave { get; }

	/// <summary>
	/// Gets the auto-save interval in milliseconds (0 to disable)
	/// </summary>
	public int AutoSaveIntervalMilliseconds { get; }
}
