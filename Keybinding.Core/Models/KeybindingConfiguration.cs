// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

using ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Default implementation of keybinding configuration
/// </summary>
public sealed class KeybindingConfiguration : IKeybindingConfiguration
{
	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingConfiguration"/> class with default values
	/// </summary>
	public KeybindingConfiguration()
	{
		DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keybinding");
		DefaultProfileId = "default";
		DefaultProfileName = "Default";
		AutoSave = true;
		AutoSaveIntervalMilliseconds = 5000; // 5 seconds
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingConfiguration"/> class with custom values
	/// </summary>
	/// <param name="dataDirectory">The data directory for storing keybinding files</param>
	/// <param name="defaultProfileId">The default profile ID</param>
	/// <param name="defaultProfileName">The default profile name</param>
	/// <param name="autoSave">Whether to enable auto-save</param>
	/// <param name="autoSaveIntervalMilliseconds">The auto-save interval in milliseconds</param>
	public KeybindingConfiguration(
		string dataDirectory,
		string defaultProfileId = "default",
		string defaultProfileName = "Default",
		bool autoSave = true,
		int autoSaveIntervalMilliseconds = 5000)
	{
		DataDirectory = Ensure.NotNull(dataDirectory);
		DefaultProfileId = Ensure.NotNull(defaultProfileId);
		DefaultProfileName = Ensure.NotNull(defaultProfileName);
		AutoSave = autoSave;
		AutoSaveIntervalMilliseconds = Math.Max(0, autoSaveIntervalMilliseconds);
	}

	/// <inheritdoc/>
	public string DataDirectory { get; }

	/// <inheritdoc/>
	public string DefaultProfileId { get; }

	/// <inheritdoc/>
	public string DefaultProfileName { get; }

	/// <inheritdoc/>
	public bool AutoSave { get; }

	/// <inheritdoc/>
	public int AutoSaveIntervalMilliseconds { get; }
}
