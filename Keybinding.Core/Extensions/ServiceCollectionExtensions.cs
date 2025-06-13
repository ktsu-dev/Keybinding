// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// Extension methods for registering keybinding services with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds keybinding services to the dependency injection container with default configuration
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="dataDirectory">Directory to store keybinding data (optional for JSON repository)</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddKeybinding(this IServiceCollection services, string? dataDirectory = null)
	{
		return services.AddKeybinding<JsonKeybindingRepository>(serviceProvider =>
		{
			string directory = dataDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keybinding");
			return new(directory);
		});
	}

	/// <summary>
	/// Adds keybinding services to the dependency injection container with a custom repository
	/// </summary>
	/// <typeparam name="TRepository">The repository implementation type</typeparam>
	/// <param name="services">The service collection</param>
	/// <param name="repositoryFactory">Factory function to create the repository instance (optional)</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddKeybinding<TRepository>(
		this IServiceCollection services,
		Func<IServiceProvider, TRepository>? repositoryFactory = null)
		where TRepository : class, IKeybindingRepository
	{
		// Register core interfaces with their implementations
		services.TryAddSingleton<ICommandRegistry, CommandRegistry>();
		services.TryAddSingleton<IProfileManager, ProfileManager>();
		services.TryAddSingleton<IKeybindingService, KeybindingService>();

		// Register repository
		if (repositoryFactory != null)
		{
			services.TryAddSingleton<IKeybindingRepository>(repositoryFactory);
		}
		else
		{
			services.TryAddSingleton<IKeybindingRepository, TRepository>();
		}

		// Register the main manager and factory
		services.TryAddSingleton<KeybindingManager>();
		services.TryAddSingleton<IKeybindingManagerFactory, KeybindingManagerFactory>();

		return services;
	}

	/// <summary>
	/// Adds keybinding services to the dependency injection container as scoped services
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="dataDirectory">Directory to store keybinding data (optional for JSON repository)</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddKeybindingScoped(this IServiceCollection services, string? dataDirectory = null)
	{
		return services.AddKeybindingScoped<JsonKeybindingRepository>(serviceProvider =>
		{
			string directory = dataDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Keybinding");
			return new(directory);
		});
	}

	/// <summary>
	/// Adds keybinding services to the dependency injection container as scoped services with a custom repository
	/// </summary>
	/// <typeparam name="TRepository">The repository implementation type</typeparam>
	/// <param name="services">The service collection</param>
	/// <param name="repositoryFactory">Factory function to create the repository instance (optional)</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddKeybindingScoped<TRepository>(
		this IServiceCollection services,
		Func<IServiceProvider, TRepository>? repositoryFactory = null)
		where TRepository : class, IKeybindingRepository
	{
		// Register core interfaces with their implementations as scoped
		services.TryAddScoped<ICommandRegistry, CommandRegistry>();
		services.TryAddScoped<IProfileManager, ProfileManager>();
		services.TryAddScoped<IKeybindingService, KeybindingService>();

		// Register repository
		if (repositoryFactory != null)
		{
			services.TryAddScoped<IKeybindingRepository>(repositoryFactory);
		}
		else
		{
			services.TryAddScoped<IKeybindingRepository, TRepository>();
		}

		// Register the main manager and factory
		services.TryAddScoped<KeybindingManager>();
		services.TryAddScoped<IKeybindingManagerFactory, KeybindingManagerFactory>();

		return services;
	}
}
