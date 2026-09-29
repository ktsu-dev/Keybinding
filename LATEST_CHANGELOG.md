## v2.1.0 (minor)

Changes since v2.0.0:

- Cover Profile's locked chord accessors directly ([@matt-edmondson](https://github.com/matt-edmondson))
- Lock a profile's chords so concurrent binds cannot corrupt them [minor] ([@matt-edmondson](https://github.com/matt-edmondson))
- Split note construction out of NoteJsonConverter.Read and use Assert.AreSequenceEqual ([@matt-edmondson](https://github.com/matt-edmondson))
- Only prune stored profiles this manager loaded or saved [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Trim the id in the Command(CommandId) constructor as the string one does [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Serialize Note as {"Key":"CTRL"} so it round-trips through System.Text.Json [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Reject blank input in ParsePhrase with its own argument name [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Guard blank command ids in the active-profile lookup overloads [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Return snapshots from GetAllChords and BoundCommands [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Normalize the key in the Note(NoteName) constructor as the string one does ([@matt-edmondson](https://github.com/matt-edmondson))
- Normalize modifier aliases in the Note constructors [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Skip unregistered commands when executing a shared chord [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Address SonarCloud findings on the CreateProfile fix [patch] ([@Claude](https://github.com/Claude))
- Return the stored profile from concurrent CreateProfile calls [patch] ([@Claude](https://github.com/Claude))
- Rename profiles in place and keep their description [patch] ([@Claude](https://github.com/Claude))
- Reduce KeyStringTokenizer.Split cognitive complexity ([@Claude](https://github.com/Claude))
- Persist profile deletion in KeybindingManager.SaveAsync ([@Claude](https://github.com/Claude))
- Parse "Ctrl++" and "Ctrl+," as the plus and comma keys ([@Claude](https://github.com/Claude))

