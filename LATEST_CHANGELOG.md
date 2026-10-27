## v2.2.0 (minor)

Changes since v2.1.0:

- Fix thirteen open bugs in parsing, binding, the manager and JSON storage [minor] (Claude)
- Commit the .editorconfig and .gitignore that ktsu.Sdk 2.31.2 writes on build (#170) (Claude)
- Trim the semantic Command constructor's name, description and category [patch] (Claude)
- Activate an existing default profile when none is active [patch] (matt-edmondson)
- Round-trip Chord, Phrase, Command and Profile through System.Text.Json [patch] (matt-edmondson)
- Migrate the demo off the obsolete Profile.Chords property (Claude)
- Move the chord-separator check out of Split to keep its complexity under the limit (Claude)
- Reject phrase strings and keys with spaces or commas when parsing a chord [patch] (Claude)
- Skip invalid stored commands instead of failing the load [patch] (Claude)
- Skip unregistered commands in FindCommandByChord, matching ExecuteChord [patch] (Claude)
- Return uncategorized commands for a null category in GetCommandsByCategory [patch] (Claude)
- Comment the intentionally empty activation loop in the race test (Claude)
- Lock SetActiveProfile and DeleteProfile so a delete cannot leave a stale active id [patch] (Claude)
- Skip invalid stored profiles and chords instead of failing the load [patch] (Claude)
- Move CI onto the shared ci-shared.yml pipeline (Claude)
- Filter the cleanup directories with Where (Claude)
- Give each CreateManager(dataDirectory) its own profiles and commands [patch] (Claude)

