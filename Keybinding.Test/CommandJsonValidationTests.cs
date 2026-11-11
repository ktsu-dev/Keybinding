// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.Keybinding.Test;

using System.Text.Json;
using ktsu.Keybinding.Core.Models;

[TestClass]
public class CommandJsonValidationTests
{
	[TestMethod]
	[DataRow("""{"Name":"Save"}""")]
	[DataRow("""{"Id":null,"Name":"Save"}""")]
	[DataRow("""{"Id":"  ","Name":"Save"}""")]
	[DataRow("""{"Id":"file.save"}""")]
	[DataRow("""{"Id":"file.save","Name":null}""")]
	[DataRow("""{"Id":"file.save","Name":""}""")]
	public void MissingOrBlankIdOrName_ThrowsJsonException(string json) =>
		Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<Command>(json));

	[TestMethod]
	public void ValidJson_StillNormalizesLikeThePublicConstructor()
	{
		Command? command = JsonSerializer.Deserialize<Command>("""{"Id":" file.save ","Name":" Save ","Description":"  ","Category":" File "}""");

		Assert.IsNotNull(command);
		Assert.AreEqual("file.save", command.Id.ToString());
		Assert.AreEqual("Save", command.Name.ToString());
		Assert.IsNull(command.Description);
		Assert.AreEqual("File", command.Category?.ToString());
	}

	[TestMethod]
	public void PublicConstructor_StillThrowsArgumentException() =>
		Assert.ThrowsExactly<ArgumentException>(() => new Command(CommandId.Create("  "), CommandName.Create("Save")));
}
