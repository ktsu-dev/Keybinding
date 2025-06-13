// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Factory interface for creating KeybindingManager instances
/// </summary>
public interface IKeybindingManagerFactory
{
	/// <summary>
	/// Creates a new KeybindingManager instance
	/// </summary>
	/// <returns>A new KeybindingManager instance</returns>
	public KeybindingManager CreateManager();

	/// <summary>
	/// Creates a new KeybindingManager instance with specified data directory
	/// </summary>
	/// <param name="dataDirectory">Directory to store keybinding data</param>
	/// <returns>A new KeybindingManager instance</returns>
	public KeybindingManager CreateManager(string dataDirectory);
}
