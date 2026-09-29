// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Factory interface for creating KeybindingManager instances
/// </summary>
public interface IKeybindingManagerFactory
{
	/// <summary>
	/// Gets a KeybindingManager for the default data directory
	/// </summary>
	/// <returns>
	/// The KeybindingManager registered with dependency injection when there is one, which is shared rather than new;
	/// otherwise a new instance
	/// </returns>
	public KeybindingManager CreateManager();

	/// <summary>
	/// Creates a new KeybindingManager instance with specified data directory
	/// </summary>
	/// <param name="dataDirectory">Directory to store keybinding data</param>
	/// <returns>A new KeybindingManager instance whose commands and profiles are not shared with any other manager</returns>
	public KeybindingManager CreateManager(string dataDirectory);
}
