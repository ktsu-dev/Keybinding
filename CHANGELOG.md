## v2.1.8 (patch)

Changes since v2.1.7:

- Trim the semantic Command constructor's name, description and category [patch] (Claude)

## v2.1.8-pre.1 (prerelease)

Changes since v2.1.7:

- Bump the ktsu group with 1 update (dependabot[bot])

## v2.1.7 (patch)

Changes since v2.1.6:

- Activate an existing default profile when none is active [patch] (matt-edmondson)
- Round-trip Chord, Phrase, Command and Profile through System.Text.Json [patch] (matt-edmondson)

## v2.1.7-pre.2 (prerelease)

Changes since v2.1.7-pre.1:

- Bump the ktsu group with 1 update (dependabot[bot])

## v2.1.7-pre.1 (prerelease)

Changes since v2.1.6:

- Bump MSTest.Sdk from 4.4.1 to 4.5.1 (dependabot[bot])
- Bump the ktsu group with 1 update (dependabot[bot])

## v2.1.6 (patch)

Changes since v2.1.5:

- Migrate the demo off the obsolete Profile.Chords property (Claude)

## v2.1.6-pre.1 (prerelease)

Changes since v2.1.5:

- Canonicalize key aliases and remove unrelated metadata edits ([@Copilot](https://github.com/Copilot))
- Add key alias regression tests ([@Copilot](https://github.com/Copilot))
- Initial plan ([@Copilot](https://github.com/Copilot))

## v2.1.5 (patch)

Changes since v2.1.4:

- Move the chord-separator check out of Split to keep its complexity under the limit (Claude)
- Reject phrase strings and keys with spaces or commas when parsing a chord [patch] (Claude)

## v2.1.4 (patch)

Changes since v2.1.3:

- Skip invalid stored commands instead of failing the load [patch] (Claude)
- Skip unregistered commands in FindCommandByChord, matching ExecuteChord [patch] (Claude)
- Return uncategorized commands for a null category in GetCommandsByCategory [patch] (Claude)

## v2.1.3 (patch)

Changes since v2.1.2:

- Comment the intentionally empty activation loop in the race test (Claude)
- Lock SetActiveProfile and DeleteProfile so a delete cannot leave a stale active id [patch] (Claude)
- Skip invalid stored profiles and chords instead of failing the load [patch] (Claude)

## v2.1.2 (patch)

Changes since v2.1.1:

- Move CI onto the shared ci-shared.yml pipeline (Claude)

## v2.1.1 (patch)

Changes since v2.1.0:

- Filter the cleanup directories with Where (Claude)
- Give each CreateManager(dataDirectory) its own profiles and commands [patch] (Claude)

## v2.1.0 (minor)

Changes since v2.0.0:

- Cover Profile's locked chord accessors directly (matt-edmondson)
- Lock a profile's chords so concurrent binds cannot corrupt them [minor] (matt-edmondson)
- Split note construction out of NoteJsonConverter.Read and use Assert.AreSequenceEqual (matt-edmondson)
- Only prune stored profiles this manager loaded or saved [patch] (matt-edmondson)
- Trim the id in the Command(CommandId) constructor as the string one does [patch] (matt-edmondson)
- Serialize Note as {"Key":"CTRL"} so it round-trips through System.Text.Json [patch] (matt-edmondson)
- Reject blank input in ParsePhrase with its own argument name [patch] (matt-edmondson)
- Guard blank command ids in the active-profile lookup overloads [patch] (matt-edmondson)
- Return snapshots from GetAllChords and BoundCommands [patch] (matt-edmondson)
- Normalize the key in the Note(NoteName) constructor as the string one does (matt-edmondson)
- Normalize modifier aliases in the Note constructors [patch] (matt-edmondson)
- Skip unregistered commands when executing a shared chord [patch] (matt-edmondson)
- Address SonarCloud findings on the CreateProfile fix [patch] (Claude)
- Return the stored profile from concurrent CreateProfile calls [patch] (Claude)
- Rename profiles in place and keep their description [patch] (Claude)
- Reduce KeyStringTokenizer.Split cognitive complexity (Claude)
- Persist profile deletion in KeybindingManager.SaveAsync (Claude)
- Parse "Ctrl++" and "Ctrl+," as the plus and comma keys (Claude)

## v2.0.10-pre.1 (prerelease)

Changes since v2.0.9:

- Bump Polyfill from 11.4.0 to 11.4.1 (dependabot[bot])
- Bump the ktsu group with 1 update (dependabot[bot])

## v2.0.9 (patch)

Changes since v2.0.8:

- Split note construction out of NoteJsonConverter.Read and use Assert.AreSequenceEqual (matt-edmondson)
- Only prune stored profiles this manager loaded or saved [patch] (matt-edmondson)
- Trim the id in the Command(CommandId) constructor as the string one does [patch] (matt-edmondson)
- Serialize Note as {"Key":"CTRL"} so it round-trips through System.Text.Json [patch] (matt-edmondson)

## v2.0.8 (patch)

Changes since v2.0.7:

- Reject blank input in ParsePhrase with its own argument name [patch] (matt-edmondson)
- Guard blank command ids in the active-profile lookup overloads [patch] (matt-edmondson)
- Return snapshots from GetAllChords and BoundCommands [patch] (matt-edmondson)
- Normalize the key in the Note(NoteName) constructor as the string one does (matt-edmondson)

## v2.0.7 (patch)

Changes since v2.0.6:

- Normalize modifier aliases in the Note constructors [patch] (matt-edmondson)
- Skip unregistered commands when executing a shared chord [patch] (matt-edmondson)

## v2.0.6 (patch)

Changes since v2.0.5:

- Address SonarCloud findings on the CreateProfile fix [patch] (Claude)
- Return the stored profile from concurrent CreateProfile calls [patch] (Claude)

## v2.0.5 (patch)

Changes since v2.0.4:

- Rename profiles in place and keep their description [patch] (Claude)
- Reduce KeyStringTokenizer.Split cognitive complexity (Claude)
- Persist profile deletion in KeybindingManager.SaveAsync (Claude)
- Parse "Ctrl++" and "Ctrl+," as the plus and comma keys (Claude)

## v2.0.4 (patch)

Changes since v2.0.3:

- Bump the ktsu group with 1 update (dependabot[bot])

## v2.0.3 (patch)

Changes since v2.0.2:

- Bump the ktsu group with 1 update (dependabot[bot])

## v2.0.2 (patch)

Changes since v2.0.1:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v2.0.1 (patch)

Changes since v2.0.0:

- Bump the ktsu group with 10 updates (dependabot[bot])

## v2.0.0 (major)

Changes since v1.0.0:

- [major] Retire .Core from the published package ID (Claude)
- Add dotnet to TAGS.md (matt-edmondson)
- Gate Dependabot auto-merge on CI actually being green (Claude)
- ci: adopt the consolidated .NET workflow [patch] (Claude)
- ci: make the SonarQube quality gate opt in [patch] (matt-edmondson)
- ci: adopt the unified dotnet workflow [patch] (matt-edmondson)
- chore: store icon.png in LFS as .gitattributes declares (matt-edmondson)
- docs: scope build badge to the default branch (matt-edmondson)
- docs: correct README, DESCRIPTION and TAGS metadata (matt-edmondson)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- Sync .editorconfig (KtsuTools)
- Sync global.json (KtsuTools)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- chore: update ktsu.Sdk to 2.21.1 [patch] (matt-edmondson)
- chore: remove unused SourceLink package versions (matt-edmondson)
- chore: trim unused package references and central versions (matt-edmondson)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- Remove legacy build scripts (matt-edmondson)
- Refactor null checks to use Ensure.NotNull for improved readability (matt-edmondson)
- Add CompatibilitySuppressions.xml for package validation diagnostics (matt-edmondson)
- Add Polyfill package and update project configurations; refactor null checks to use Ensure helper (matt-edmondson)
- Enhance test assertions with descriptive messages for clarity (matt-edmondson)
- Update ktsu.Semantics package versions to 1.0.28 (matt-edmondson)
- Remove .github\workflows\project.yml (matt-edmondson)
- Renamed demo project, removed old examples, and updated dependencies (matt-edmondson)
- Update project configuration and dependencies (matt-edmondson)
- Add Web API example project and update dependencies (matt-edmondson)
- Add Coverlet packages for enhanced code coverage analysis (matt-edmondson)
- Refactor test files to ensure proper namespace usage and enhance code clarity (matt-edmondson)
- Add Microsoft.Extensions.DependencyInjection package and enhance test coverage (matt-edmondson)
- Refactor KeybindingManager to enhance DRY compliance and improve validation (matt-edmondson)
- Add architecture analysis and usage examples for Keybinding library (matt-edmondson)
- Update DESCRIPTION.md and TAGS.md with accurate project information (matt-edmondson)
- Refactor Program.cs and ChordSequence.cs for improved readability and validation (matt-edmondson)
- Refactor keybinding repository to enhance chord handling and JSON serialization (matt-edmondson)
- Refactor keybinding management to implement musical paradigm and enhance documentation (matt-edmondson)
- Refactor KeyCombination class to improve key formatting and enhance documentation (matt-edmondson)
- Enhance KeyCombination display formatting and add pre-commit guidelines (matt-edmondson)
- Refactor command-line argument parsing and enhance documentation in DemoOptions (matt-edmondson)
- Enhance Keybinding.ConsoleApp to utilize CommandLineParser and improve exception handling (matt-edmondson)
- Update configuration files and enhance CI/CD workflow (matt-edmondson)
- Refactor Keybinding project structure and update dependencies (matt-edmondson)

## v1.0.54 (patch)

Changes since v1.0.53:

- Bump Polyfill from 11.3.0 to 11.4.0 (dependabot[bot])
- Bump the ktsu group with 10 updates (dependabot[bot])

## v1.0.53 (patch)

Changes since v1.0.52:

- Bump the ktsu group with 10 updates (dependabot[bot])

## v1.0.52 (patch)

Changes since v1.0.51:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.51 (patch)

Changes since v1.0.50:

- Add dotnet to TAGS.md (matt-edmondson)

## v1.0.50 (patch)

Changes since v1.0.49:

- Gate Dependabot auto-merge on CI actually being green (Claude)

## v1.0.49 (patch)

Changes since v1.0.48:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.48 (patch)

Changes since v1.0.47:

- Bump the ktsu group with 10 updates (dependabot[bot])

## v1.0.47 (patch)

Changes since v1.0.46:

- ci: adopt the consolidated .NET workflow [patch] (Claude)

## v1.0.46 (patch)

Changes since v1.0.45:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.45 (patch)

Changes since v1.0.44:

- Bump Polyfill from 11.2.0 to 11.3.0 (dependabot[bot])
- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.44 (patch)

Changes since v1.0.43:

- Bump Microsoft.Extensions.DependencyInjection and Microsoft.Extensions.DependencyInjection.Abstractions (dependabot[bot])
- Bump the system group with 1 update (dependabot[bot])
- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.43 (patch)

Changes since v1.0.42:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.42 (patch)

Changes since v1.0.41:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.41 (patch)

Changes since v1.0.40:

- Bump MSTest.Sdk from 4.3.3 to 4.4.0 (dependabot[bot])
- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.40 (patch)

Changes since v1.0.39:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.39 (patch)

Changes since v1.0.38:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.38 (patch)

No significant changes detected since v1.0.37.

## v1.0.37 (patch)

Changes since v1.0.36:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.36 (patch)

Changes since v1.0.35:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.35 (patch)

Changes since v1.0.34:

- ci: make the SonarQube quality gate opt in [patch] (matt-edmondson)
- ci: adopt the unified dotnet workflow [patch] (matt-edmondson)

## v1.0.34 (patch)

Changes since v1.0.33:

- Bump the ktsu group with 10 updates (dependabot[bot])

## v1.0.33 (patch)

No significant changes detected since v1.0.32.

## v1.0.32 (patch)

Changes since v1.0.31:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.0.31 (patch)

Changes since v1.0.30:

- Bump the ktsu group with 10 updates (dependabot[bot])

## v1.0.30 (patch)

Changes since v1.0.29:

- Bump the ktsu group with 10 updates (dependabot[bot])

## v1.0.29 (patch)

Changes since v1.0.28:

- chore: store icon.png in LFS as .gitattributes declares (matt-edmondson)
- docs: scope build badge to the default branch (matt-edmondson)
- docs: correct README, DESCRIPTION and TAGS metadata (matt-edmondson)

## v1.0.28 (patch)

Changes since v1.0.27:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.27 (patch)

Changes since v1.0.26:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.26 (patch)

Changes since v1.0.25:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.0.25 (patch)

Changes since v1.0.24:

- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.0.25-pre.1 (prerelease)

Changes since v1.0.24:

- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.0.24 (patch)

Changes since v1.0.23:

- Sync .editorconfig (KtsuTools)

## v1.0.23 (patch)

Changes since v1.0.22:

- Sync global.json (KtsuTools)
- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.0.22 (patch)

Changes since v1.0.21:

- Bump the system group with 1 update (dependabot[bot])
- Bump the microsoft group with 2 updates (dependabot[bot])
- Bump Polyfill from 11.0.1 to 11.0.2 (dependabot[bot])

## v1.0.21 (patch)

Changes since v1.0.20:

- chore: update ktsu.Sdk to 2.21.1 [patch] (matt-edmondson)

## v1.0.20 (patch)

Changes since v1.0.19:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.19 (patch)

Changes since v1.0.18:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.18 (patch)

Changes since v1.0.17:

- Bump Spectre.Console from 0.57.1 to 0.57.2 (dependabot[bot])

## v1.0.17 (patch)

Changes since v1.0.16:

- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.16 (patch)

Changes since v1.0.15:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.0.15 (patch)

Changes since v1.0.14:

- Bump Polyfill from 10.11.1 to 10.11.2 (dependabot[bot])

## v1.0.14 (patch)

Changes since v1.0.13:

- Bump Polyfill from 10.11.0 to 10.11.1 (dependabot[bot])
- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.0.13 (patch)

Changes since v1.0.12:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.0.12 (patch)

Changes since v1.0.11:

- chore: remove unused SourceLink package versions (matt-edmondson)
- chore: trim unused package references and central versions (matt-edmondson)

## v1.0.11 (patch)

Changes since v1.0.10:

- Bump the ktsu group with 1 update (dependabot[bot])
- Bump Spectre.Console from 0.56.0 to 0.57.0 (dependabot[bot])
- Bump the ktsu group with 1 update (dependabot[bot])

## v1.0.10 (patch)

Changes since v1.0.9:

- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.0.10-pre.1 (prerelease)

Changes since v1.0.9:

- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.0.9 (patch)

Changes since v1.0.8:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])

## v1.0.9-pre.1 (prerelease)

Changes since v1.0.8:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])

## v1.0.8 (patch)

Changes since v1.0.7:

- Remove legacy build scripts (matt-edmondson)

## v1.0.7 (patch)

Changes since v1.0.6:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync global.json (ktsu[bot])
- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])
- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync global.json (ktsu[bot])
- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync global.json (ktsu[bot])
- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.0.7-pre.4 (prerelease)

Changes since v1.0.7-pre.3:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])

## v1.0.7-pre.3 (prerelease)

Changes since v1.0.7-pre.2:

- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.0.7-pre.2 (prerelease)

Changes since v1.0.7-pre.1:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.0.7-pre.1 (prerelease)

Changes since v1.0.6:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.0.6 (patch)

Changes since v1.0.5:

- Refactor null checks to use Ensure.NotNull for improved readability (matt-edmondson)

## v1.0.5 (patch)

Changes since v1.0.4:

- Add CompatibilitySuppressions.xml for package validation diagnostics (matt-edmondson)

## v1.0.4 (patch)

Changes since v1.0.3:

- Add Polyfill package and update project configurations; refactor null checks to use Ensure helper (matt-edmondson)
- Enhance test assertions with descriptive messages for clarity (matt-edmondson)
- Update ktsu.Semantics package versions to 1.0.28 (matt-edmondson)
- Remove .github\workflows\project.yml (matt-edmondson)
- Renamed demo project, removed old examples, and updated dependencies (matt-edmondson)
- Update project configuration and dependencies (matt-edmondson)
- Add Web API example project and update dependencies (matt-edmondson)
- Add Coverlet packages for enhanced code coverage analysis (matt-edmondson)

## v1.0.3 (patch)

Changes since v1.0.2:

- Refactor test files to ensure proper namespace usage and enhance code clarity (matt-edmondson)
- Add Microsoft.Extensions.DependencyInjection package and enhance test coverage (matt-edmondson)

## v1.0.2 (patch)

Changes since v1.0.1:

- Refactor KeybindingManager to enhance DRY compliance and improve validation (matt-edmondson)
- Add architecture analysis and usage examples for Keybinding library (matt-edmondson)

## v1.0.1 (patch)

Changes since v1.0.0:

- Update DESCRIPTION.md and TAGS.md with accurate project information (matt-edmondson)
- Refactor Program.cs and ChordSequence.cs for improved readability and validation (matt-edmondson)
- Refactor keybinding repository to enhance chord handling and JSON serialization (matt-edmondson)
- Refactor keybinding management to implement musical paradigm and enhance documentation (matt-edmondson)
- Refactor KeyCombination class to improve key formatting and enhance documentation (matt-edmondson)
- Enhance KeyCombination display formatting and add pre-commit guidelines (matt-edmondson)
- Refactor command-line argument parsing and enhance documentation in DemoOptions (matt-edmondson)
- Enhance Keybinding.ConsoleApp to utilize CommandLineParser and improve exception handling (matt-edmondson)
- Update configuration files and enhance CI/CD workflow (matt-edmondson)
- Refactor Keybinding project structure and update dependencies (matt-edmondson)

## v1.0.0 (major)

- Add initial implementation of Keybinding library with core features, including command and profile management, keybinding functionality, and comprehensive tests. (matt-edmondson)
- Initial commit for Keybinding (matt-edmondson)

