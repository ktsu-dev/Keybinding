// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using Microsoft.Extensions.DependencyInjection;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Extensions;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

/// <summary>
/// The manager uses the <see cref="IKeybindingService"/> registered with the container instead of creating its own
/// (ktsu-dev/Keybinding#208).
/// </summary>
[TestClass]
public class KeybindingServiceInjectionTests
{
	private string _dataDirectory = null!;

	[TestInitialize]
	public void Setup()
	{
		_dataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_dataDirectory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_dataDirectory))
		{
			Directory.Delete(_dataDirectory, recursive: true);
		}
	}

	private static void AddCustomKeybindingService(IServiceCollection services) =>
		services.AddSingleton<IKeybindingService>(sp => new KeybindingService(
			sp.GetRequiredService<ICommandRegistry>(),
			sp.GetRequiredService<IProfileManager>()));

	[TestMethod]
	public void Manager_FromContainer_UsesTheRegisteredCustomService()
	{
		ServiceCollection services = new();
		AddCustomKeybindingService(services);
		services.AddKeybinding(_dataDirectory);
		using ServiceProvider provider = services.BuildServiceProvider();

		IKeybindingService registered = provider.GetRequiredService<IKeybindingService>();
		KeybindingManager manager = provider.GetRequiredService<KeybindingManager>();

		Assert.AreSame(registered, manager.Keybindings);
	}

	[TestMethod]
	public void Manager_FromContainer_UsesTheDefaultRegisteredService()
	{
		ServiceCollection services = new();
		services.AddKeybinding(_dataDirectory);
		using ServiceProvider provider = services.BuildServiceProvider();

		IKeybindingService registered = provider.GetRequiredService<IKeybindingService>();
		KeybindingManager manager = provider.GetRequiredService<KeybindingManager>();

		Assert.AreSame(registered, manager.Keybindings);
	}

	[TestMethod]
	public void ScopedManager_UsesTheServiceRegisteredInItsScope()
	{
		ServiceCollection services = new();
		services.AddKeybindingScoped(_dataDirectory);
		using ServiceProvider provider = services.BuildServiceProvider();
		using IServiceScope scope = provider.CreateScope();

		IKeybindingService registered = scope.ServiceProvider.GetRequiredService<IKeybindingService>();
		KeybindingManager manager = scope.ServiceProvider.GetRequiredService<KeybindingManager>();

		Assert.AreSame(registered, manager.Keybindings);
	}

	[TestMethod]
	public void FactoryCreateManager_UsesTheRegisteredService()
	{
		ServiceCollection services = new();
		AddCustomKeybindingService(services);
		services.AddKeybinding(_dataDirectory);
		using ServiceProvider provider = services.BuildServiceProvider();

		IKeybindingService registered = provider.GetRequiredService<IKeybindingService>();
		using KeybindingManager manager = provider.GetRequiredService<IKeybindingManagerFactory>().CreateManager();

		Assert.AreSame(registered, manager.Keybindings);
	}

	[TestMethod]
	public void Manager_FromContainer_BindsThroughTheRegisteredService()
	{
		ServiceCollection services = new();
		AddCustomKeybindingService(services);
		services.AddKeybinding(_dataDirectory);
		using ServiceProvider provider = services.BuildServiceProvider();

		KeybindingManager manager = provider.GetRequiredService<KeybindingManager>();
		manager.CreateDefaultProfile();
		manager.Commands.RegisterCommand(new Command("file.save", "Save"));

		Assert.IsTrue(manager.Keybindings.BindChord("file.save", Chord.Parse("Ctrl+S")));
		Assert.AreEqual(Chord.Parse("Ctrl+S"), provider.GetRequiredService<IKeybindingService>().GetChord("file.save"));
	}

	[TestMethod]
	public void StandaloneConstructor_StillCreatesItsOwnKeybindingService()
	{
		using KeybindingManager manager = new(new CommandRegistry(), new ProfileManager(), new JsonKeybindingRepository(_dataDirectory));

		Assert.IsInstanceOfType<KeybindingService>(manager.Keybindings);
	}
}
