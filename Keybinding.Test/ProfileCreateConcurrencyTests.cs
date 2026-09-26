// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class ProfileCreateConcurrencyTests
{
	private const int ThreadCount = 8;
	private const int Trials = 500;

	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void CreateProfile_CalledConcurrentlyForOneId_ReturnsTheStoredProfileToEveryCaller()
	{
		for (int trial = 0; trial < Trials; trial++)
		{
			ProfileManager profiles = new();
			Profile[] returned = new Profile[ThreadCount];
			using Barrier barrier = new(ThreadCount);

			Thread[] threads = [.. Enumerable.Range(0, ThreadCount).Select(i => new Thread(() =>
			{
				barrier.SignalAndWait(TestContext.CancellationToken);
				returned[i] = profiles.CreateProfile("p", "P");
			}))];

			foreach (Thread thread in threads)
			{
				thread.Start();
			}

			foreach (Thread thread in threads)
			{
				thread.Join();
			}

			Profile? stored = profiles.GetProfile("p");
			Assert.IsNotNull(stored);
			for (int i = 0; i < ThreadCount; i++)
			{
				Assert.AreSame(stored, returned[i], $"Trial {trial}: caller {i} got a Profile that was never stored, so its bindings would be lost.");
			}
		}
	}
}
