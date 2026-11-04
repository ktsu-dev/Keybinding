// Copyright (c) 2023-2026 ktsu-dev contributors

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
	/// <remarks>
	/// The manager is always new, even when a <see cref="KeybindingManager"/> is registered with the service
	/// provider, because the caller owns and disposes what a factory returns. Handing out the registered singleton
	/// meant disposing it broke every other user of that instance for the rest of the process.
	/// </remarks>
	public KeybindingManager CreateManager()
	{
		if (_serviceProvider != null)
		{
			// Build over the registered services, so the new manager sees the same commands and profiles as the
			// registered one without sharing its lifetime
			if (_serviceProvider.GetService(typeof(ICommandRegistry)) is ICommandRegistry commandRegistry &&
				_serviceProvider.GetService(typeof(IProfileManager)) is IProfileManager profileManager &&
				_serviceProvider.GetService(typeof(IKeybindingRepository)) is IKeybindingRepository repository)
			{
				// Use the registered keybinding service too, so a custom or decorated one takes effect
				return _serviceProvider.GetService(typeof(IKeybindingService)) is IKeybindingService keybindingService
					? new KeybindingManager(commandRegistry, profileManager, repository, keybindingService)
					: new KeybindingManager(commandRegistry, profileManager, repository);
			}
		}

		// Fallback to default implementation
		string defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keybinding");
		return new KeybindingManager(defaultDirectory);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// The manager always gets its own command registry and profile manager. Reusing the singletons registered in
	/// the service provider would let managers for different directories share profiles, so saving one directory
	/// would write another directory's profiles into it.
	/// </remarks>
	public KeybindingManager CreateManager(string dataDirectory) => new(dataDirectory);
}
