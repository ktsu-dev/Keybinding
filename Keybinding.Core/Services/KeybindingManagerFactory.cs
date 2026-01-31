// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Services;

using System;
using ktsu.Keybinding.Core.Contracts;

/// <summary>
/// Default factory implementation for creating KeybindingManager instances
/// </summary>
public sealed class KeybindingManagerFactory : IKeybindingManagerFactory
{
	private readonly IServiceProvider? _serviceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingManagerFactory"/> class for standalone usage
	/// </summary>
	public KeybindingManagerFactory() => _serviceProvider = null;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeybindingManagerFactory"/> class for dependency injection usage
	/// </summary>
	/// <param name="serviceProvider">The service provider for resolving dependencies</param>
	public KeybindingManagerFactory(IServiceProvider serviceProvider) =>
		_serviceProvider = Ensure.NotNull(serviceProvider);

	/// <inheritdoc/>
	public KeybindingManager CreateManager()
	{
		if (_serviceProvider != null)
		{
			// Try to resolve from DI container first
			if (_serviceProvider.GetService(typeof(KeybindingManager)) is KeybindingManager manager)
			{
				return manager;
			}

			// If not registered in DI, create with DI services
			if (_serviceProvider.GetService(typeof(ICommandRegistry)) is ICommandRegistry commandRegistry &&
				_serviceProvider.GetService(typeof(IProfileManager)) is IProfileManager profileManager &&
				_serviceProvider.GetService(typeof(IKeybindingRepository)) is IKeybindingRepository repository)
			{
				return new KeybindingManager(commandRegistry, profileManager, repository);
			}
		}

		// Fallback to default implementation
		string defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keybinding");
		return new KeybindingManager(defaultDirectory);
	}

	/// <inheritdoc/>
	public KeybindingManager CreateManager(string dataDirectory)
	{
		if (_serviceProvider != null)
		{
			// If we have a service provider, try to resolve services but use custom directory
			if (_serviceProvider.GetService(typeof(ICommandRegistry)) is ICommandRegistry commandRegistry &&
				_serviceProvider.GetService(typeof(IProfileManager)) is IProfileManager profileManager)
			{
				JsonKeybindingRepository repository = new(dataDirectory);
				return new KeybindingManager(commandRegistry, profileManager, repository);
			}
		}

		// Fallback to standard constructor
		return new KeybindingManager(dataDirectory);
	}
}
