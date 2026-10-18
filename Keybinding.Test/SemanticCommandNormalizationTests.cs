// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;

[TestClass]
public class SemanticCommandNormalizationTests
{
	private static Command CreatePaddedCommand() =>
		new(CommandId.Create("edit.copy"), CommandName.Create(" Copy "), CommandDescription.Create("   "), CommandCategory.Create("Editing "));

	[TestMethod]
	public void Constructor_PaddedNameAndCategory_AreTrimmed()
	{
		Command command = CreatePaddedCommand();

		Assert.AreEqual("Copy", command.Name.ToString());
		Assert.AreEqual("Editing", command.Category?.ToString());
	}

	[TestMethod]
	public void Constructor_WhitespaceOnlyDescriptionAndCategory_BecomeNull()
	{
		Command command = new(CommandId.Create("edit.copy"), CommandName.Create("Copy"), CommandDescription.Create("   "), CommandCategory.Create(" "));

		Assert.IsNull(command.Description);
		Assert.IsNull(command.Category);
	}

	[TestMethod]
	public void Constructor_WhitespaceOnlyName_Throws()
	{
		Assert.ThrowsExactly<ArgumentException>(() => new Command(CommandId.Create("edit.copy"), CommandName.Create("   ")));
	}

	[TestMethod]
	public void Constructor_PaddedValues_MatchStringConstructedCommand()
	{
		Command expected = new("edit.copy", " Copy ", "   ", "Editing ");
		Command actual = CreatePaddedCommand();

		Assert.AreEqual(expected.Name.ToString(), actual.Name.ToString());
		Assert.AreEqual(expected.Description?.ToString(), actual.Description?.ToString());
		Assert.AreEqual(expected.Category?.ToString(), actual.Category?.ToString());
	}

	[TestMethod]
	public void GetCommandsByCategory_PaddedSemanticCategory_FindsCommandWithoutReload()
	{
		CommandRegistry registry = new();
		Assert.IsTrue(registry.RegisterCommand(CreatePaddedCommand()));

		Assert.AreEqual(1, registry.GetCommandsByCategory("Editing").Count);
	}
}
