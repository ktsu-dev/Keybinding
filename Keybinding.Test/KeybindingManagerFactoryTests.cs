// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using Microsoft.Extensions.DependencyInjection;
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Extensions;

[TestClass]
public class KeybindingManagerFactoryTests
{
	private string _dirA = null!;
	private string _dirB = null!;

	[TestInitialize]
	public void Setup()
	{
		_dirA = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_dirB = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_dirA);
		Directory.CreateDirectory(_dirB);
	}

	[TestCleanup]
	public void Cleanup()
	{
		foreach (string dir in new[] { _dirA, _dirB }.Where(Directory.Exists))
		{
			Directory.Delete(dir, recursive: true);
		}
	}

	private static async Task<string> LoadProfileIdsAsync(string dataDirectory)
	{
		using KeybindingManager manager = new(dataDirectory);
		await manager.InitializeAsync().ConfigureAwait(false);
		return string.Join(',', manager.Profiles.GetAllProfiles().Select(p => p.Id).Order(StringComparer.Ordinal));
	}

	[TestMethod]
	public async Task CreateManager_WithServiceProvider_KeepsDirectoriesSeparate()
	{
		ServiceCollection services = new();
		services.AddKeybinding(_dirA);
		using ServiceProvider provider = services.BuildServiceProvider();
		IKeybindingManagerFactory factory = provider.GetRequiredService<IKeybindingManagerFactory>();

		using KeybindingManager a = factory.CreateManager(_dirA);
		using KeybindingManager b = factory.CreateManager(_dirB);

		await a.InitializeAsync().ConfigureAwait(false);
		a.Profiles.CreateProfile("user-a", "User A");
		await a.SaveAsync().ConfigureAwait(false);

		await b.InitializeAsync().ConfigureAwait(false);
		await b.SaveAsync().ConfigureAwait(false);

		Assert.IsFalse(b.Profiles.ProfileExists("user-a"));
		Assert.AreEqual("user-a", await LoadProfileIdsAsync(_dirA).ConfigureAwait(false));
		Assert.AreEqual(string.Empty, await LoadProfileIdsAsync(_dirB).ConfigureAwait(false));
	}

	[TestMethod]
	public void CreateManager_WithServiceProvider_DoesNotReuseSingletonState()
	{
		ServiceCollection services = new();
		services.AddKeybinding(_dirA);
		using ServiceProvider provider = services.BuildServiceProvider();
		IKeybindingManagerFactory factory = provider.GetRequiredService<IKeybindingManagerFactory>();

		using KeybindingManager manager = factory.CreateManager(_dirB);

		Assert.AreNotSame(provider.GetRequiredService<IProfileManager>(), manager.Profiles);
		Assert.AreNotSame(provider.GetRequiredService<ICommandRegistry>(), manager.Commands);
	}
}
