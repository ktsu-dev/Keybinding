// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Services;

[TestClass]
public class ProfileActivateDeleteConcurrencyTests
{
	private const int Trials = 10000;

	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void SetActiveProfile_RacingDeleteProfile_NeverLeavesTheDeletedIdActive()
	{
		int stale = 0;
		for (int trial = 0; trial < Trials; trial++)
		{
			ProfileManager profiles = new();
			profiles.CreateProfile("x", "X");
			using Barrier barrier = new(2);

			Thread activate = new(() =>
			{
				barrier.SignalAndWait(TestContext.CancellationToken);

				// Keep activating until the delete lands, so the activation is in flight when it does
				while (profiles.SetActiveProfile("x"))
				{
					// Intentionally empty: each iteration is another activation racing the delete
				}
			});
			Thread delete = new(() =>
			{
				barrier.SignalAndWait(TestContext.CancellationToken);
				profiles.DeleteProfile("x");
			});

			activate.Start();
			delete.Start();
			activate.Join();
			delete.Join();

			// A profile recreated with the deleted id must not become active on its own
			profiles.CreateProfile("x", "Recreated");
			if (profiles.GetActiveProfile() is not null)
			{
				stale++;
			}
		}

		Assert.AreEqual(0, stale, $"{stale} of {Trials} trials left the active id pointing at the deleted profile.");
	}
}
