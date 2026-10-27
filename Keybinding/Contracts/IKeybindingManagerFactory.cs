// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Factory interface for creating KeybindingManager instances
/// </summary>
public interface IKeybindingManagerFactory
{
	/// <summary>
	/// Creates a new KeybindingManager for the default data directory
	/// </summary>
	/// <returns>
	/// A new KeybindingManager, which the caller owns and may dispose. When the factory comes from dependency
	/// injection, the manager is built over the registered command registry, profile manager and repository, so it
	/// shares their state; disposing it does not dispose them or any registered KeybindingManager.
	/// </returns>
	public KeybindingManager CreateManager();

	/// <summary>
	/// Creates a new KeybindingManager instance with specified data directory
	/// </summary>
	/// <param name="dataDirectory">Directory to store keybinding data</param>
	/// <returns>
	/// A new KeybindingManager instance whose commands and profiles are not shared with any other manager. The caller
	/// owns it and may dispose it.
	/// </returns>
	public KeybindingManager CreateManager(string dataDirectory);
}
