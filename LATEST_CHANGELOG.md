## v2.0.9 (patch)

Changes since v2.0.8:

- Split note construction out of NoteJsonConverter.Read and use Assert.AreSequenceEqual ([@matt-edmondson](https://github.com/matt-edmondson))
- Only prune stored profiles this manager loaded or saved [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Trim the id in the Command(CommandId) constructor as the string one does [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Serialize Note as {"Key":"CTRL"} so it round-trips through System.Text.Json [patch] ([@matt-edmondson](https://github.com/matt-edmondson))

