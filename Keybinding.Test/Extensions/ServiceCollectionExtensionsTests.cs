// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Extensions;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class ServiceCollectionExtensionsTests
{
	[TestMethod]
	public void AddKeybinding_WithDefaultConfiguration_RegistersAllServices()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		services.AddKeybinding("./test-data");

		// Assert
		using ServiceProvider provider = services.BuildServiceProvider();

		// Verify all services are registered
		Assert.IsNotNull(provider.GetService<ICommandRegistry>());
		Assert.IsNotNull(provider.GetService<IProfileManager>());
		Assert.IsNotNull(provider.GetService<IKeybindingService>());
		Assert.IsNotNull(provider.GetService<IKeybindingRepository>());
		Assert.IsNotNull(provider.GetService<KeybindingManager>());
		Assert.IsNotNull(provider.GetService<IKeybindingManagerFactory>());
	}

	[TestMethod]
	public void AddKeybinding_WithCustomRepository_RegistersCustomRepository()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		services.AddKeybinding<TestKeybindingRepository>();

		// Assert
		using ServiceProvider provider = services.BuildServiceProvider();

		IKeybindingRepository repository = provider.GetRequiredService<IKeybindingRepository>();
		Assert.IsInstanceOfType<TestKeybindingRepository>(repository);
	}

	[TestMethod]
	public void AddKeybindingScoped_RegistersServicesAsScoped()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		services.AddKeybindingScoped("./test-data");

		// Assert
		using ServiceProvider provider = services.BuildServiceProvider();

		// Create two scopes and verify different instances
		using IServiceScope scope1 = provider.CreateScope();
		using IServiceScope scope2 = provider.CreateScope();

		KeybindingManager manager1 = scope1.ServiceProvider.GetRequiredService<KeybindingManager>();
		KeybindingManager manager2 = scope2.ServiceProvider.GetRequiredService<KeybindingManager>();

		Assert.AreNotSame(manager1, manager2);
	}

	[TestMethod]
	public void AddKeybinding_RegisteredAsSingleton_ReturnsSameInstance()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		services.AddKeybinding("./test-data");

		// Assert
		using ServiceProvider provider = services.BuildServiceProvider();

		KeybindingManager manager1 = provider.GetRequiredService<KeybindingManager>();
		KeybindingManager manager2 = provider.GetRequiredService<KeybindingManager>();

		Assert.AreSame(manager1, manager2);
	}

	/// <summary>
	/// Test implementation of IKeybindingRepository for testing purposes
	/// </summary>
	public sealed class TestKeybindingRepository : IKeybindingRepository
	{
		public Task SaveProfileAsync(Profile profile) => Task.CompletedTask;
		public Task<Profile?> LoadProfileAsync(string profileId) => Task.FromResult<Profile?>(null);
		public Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync() => Task.FromResult<IReadOnlyCollection<Profile>>([]);
		public Task DeleteProfileAsync(string profileId) => Task.CompletedTask;
		public Task SaveCommandsAsync(IEnumerable<Command> commands) => Task.CompletedTask;
		public Task<IReadOnlyCollection<Command>> LoadCommandsAsync() => Task.FromResult<IReadOnlyCollection<Command>>([]);
		public Task SaveActiveProfileAsync(string? profileId) => Task.CompletedTask;
		public Task<string?> LoadActiveProfileAsync() => Task.FromResult<string?>(null);
		public Task<bool> IsInitializedAsync() => Task.FromResult(true);
		public Task InitializeAsync() => Task.CompletedTask;
	}
}
