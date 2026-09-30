// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class CommandCategoryFilterTests
{
	private static readonly string[] UncategorizedIds = ["file.save"];
	private static readonly string[] EditIds = ["edit.copy"];

	private CommandRegistry _registry = null!;

	[TestInitialize]
	public void Setup()
	{
		_registry = new CommandRegistry();
		_registry.RegisterCommand(new Command("file.save", "Save"));
		_registry.RegisterCommand(new Command("edit.copy", "Copy", null, "Edit"));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("  ")]
	public void GetCommandsByCategory_BlankCategory_ReturnsOnlyUncategorizedCommands(string? category)
	{
		string[] ids = [.. _registry.GetCommandsByCategory(category).Select(c => (string)c.Id)];

		CollectionAssert.AreEquivalent(UncategorizedIds, ids);
	}

	[TestMethod]
	[DataRow("Edit")]
	[DataRow("edit")]
	[DataRow(" Edit ")]
	public void GetCommandsByCategory_NamedCategory_ReturnsOnlyThatCategory(string category)
	{
		string[] ids = [.. _registry.GetCommandsByCategory(category).Select(c => (string)c.Id)];

		CollectionAssert.AreEquivalent(EditIds, ids);
	}

	[TestMethod]
	public void GetCommandsByCategory_PassingACommandsOwnCategoryBack_FindsThatCommand()
	{
		foreach (Command command in _registry.GetAllCommands())
		{
			Assert.IsTrue(
				_registry.GetCommandsByCategory(command.Category).Contains(command),
				$"{command.Id} should be in the group for its own category.");
		}
	}
}
