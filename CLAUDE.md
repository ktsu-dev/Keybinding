# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

### Building
```bash
# Build all projects
dotnet build

# Build specific solution
dotnet build Keybinding.sln
dotnet build Keybinding.Examples.sln

# Format code
dotnet format
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutput=./TestResults/ /p:CoverletOutputFormat=cobertura /p:Exclude="[*Test*]*"

# Run tests from test directory
cd Keybinding.Test
dotnet test /p:CollectCoverage=true /p:CoverletOutput=../TestResults/ /p:CoverletOutputFormat=cobertura /p:Exclude="[*Test*]*"
```

### Running Examples
```bash
# Run console app demo
dotnet run --project Keybinding.Demo

# Run Web API example
dotnet run --project Examples/Keybinding.WebAPI.Example
```

## Architecture Overview

### Musical Paradigm
The library uses a musical metaphor for keybinding concepts:

- **Notes** 🎵 → Individual keys (A, B, Escape, Ctrl, Alt, etc.)
  - Represented by `Note` class with a `NoteName` property
  - Keys are stored uppercase internally
  - Modifiers (Ctrl, Alt, Shift, Meta) are treated as notes

- **Chords** 🎶 → Key combinations (Ctrl+C, Alt+Tab, Ctrl+Shift+N)
  - Represented by `Chord` class containing multiple `Note` objects
  - Notes are deduplicated and ordered
  - Display format shows modifiers first (Ctrl, Alt, Shift, Meta), then regular keys
  - Parse from string: `Chord.Parse("Ctrl+Alt+S")`

- **Phrases** 🎼 → Sequences of chords (like Ctrl+R, R in Visual Studio)
  - Represented by `Phrase` class containing multiple `Chord` objects
  - Can be single chord or multi-chord sequences
  - Parse from string: `Phrase.Parse("Ctrl+R, R")`

- **Profiles** 📝 → Collections of command-to-phrase bindings
  - Represented by `Profile` class with ID, name, description
  - Store mappings from `CommandId` to `Phrase`
  - Support multiple profiles (e.g., "default", "vim", "gaming")

### Core Architecture Pattern
The codebase follows a contracts-models-services architecture with SOLID principles:

**Contracts (Interfaces):**
- `ICommandRegistry` - Command registration and retrieval
- `IProfileManager` - Profile lifecycle management
- `IKeybindingService` - Keybinding coordination between commands and profiles
- `IKeybindingRepository` - Persistence operations
- `IKeybindingConfiguration` - Configuration abstraction
- `IKeybindingManagerFactory` - Factory for creating managers

**Models:**
- Musical types: `Note`, `Chord`, `Phrase`
- Domain types: `Command`, `Profile`, `CommandId`, `ProfileId`, etc.
- Semantic string types (using ktsu.SemanticString)
- All models are immutable where possible

**Services:**
- `CommandRegistry` - Thread-safe command storage (uses `ConcurrentDictionary`)
- `ProfileManager` - Profile management with locking
- `KeybindingService` - Coordinates commands and profiles
- `JsonKeybindingRepository` - JSON-based persistence

**Facade:**
- `KeybindingManager` - Main entry point coordinating all services

### Dual Usage Pattern
The library supports both standalone and dependency injection usage:

**Standalone:**
```csharp
var manager = new KeybindingManager("./data");
await manager.InitializeAsync();
```

**Dependency Injection:**
```csharp
services.AddKeybinding("./data");              // Singleton with default repository
services.AddKeybindingScoped();                 // Scoped lifetime
services.AddKeybinding<CustomRepository>();     // Custom repository implementation
```

### Data Storage
Default JSON-based storage structure:
```
data-directory/
├── commands.json          # All registered commands
├── active-profile.json    # ID of currently active profile
└── profiles/
    ├── default.json       # Default profile phrase bindings
    └── custom.json        # Custom profile phrase bindings
```

## Key Development Principles

### SOLID and DRY
- Each class has single responsibility
- Interfaces enable extensibility without modification
- Helper classes eliminate repeated patterns:
  - `ValidationHelper` - Common validation operations
  - `DisposalHelper` - Consistent disposal checking
  - `AsyncBatchHelper` - Async iteration patterns
  - `OperationHelper` - Error-handling loops

### Code Quality Rules
- Use semantic string types from ktsu.SemanticString with validation attributes
- Convert strings using `.As<SemanticString>()` extensions (not casts)
- One type per file, avoid nested types
- Use enums, `nameof`, and consts to reduce hard-coded strings
- Cache `CompositeFormat` instances for string formatting
- Catch specific exception types (CA1031)
- Use `DisposalHelper.ThrowIfDisposed()` for disposal checking

### Key Implementation Details
- Keys are stored in uppercase internally in `NoteName`
- `Chord.ToString()` formats modifier keys with proper casing for display
- Modifier keys ordered consistently: Ctrl, Alt, Shift, Meta
- Modifiers normalized: "Control" → "Ctrl", "Win"/"Windows"/"Cmd"/"Command" → "Meta"
- Thread-safe operations in all services
- All async operations use `.ConfigureAwait(false)`

## Project Structure

```
Keybinding.Core/              # Main library
├── Contracts/                # Interfaces
├── Models/                   # Domain models and musical types
├── Services/                 # Service implementations
├── Extensions/               # Extension methods (DI registration)
├── Helpers/                  # Helper classes
└── Constants.cs              # Shared constants

Keybinding.Test/              # Unit tests
Keybinding.Demo/              # Demo console application
Examples/                     # Example applications
└── Keybinding.WebAPI.Example/  # Web API DI example
```

## Development Workflow

### Before Committing
```bash
dotnet format
dotnet test
```

### Project Configuration
- Uses .NET 9.0 (`global.json`)
- Central Package Management (`Directory.Packages.props`)
- Custom SDK: `ktsu.Sdk.Lib` for library projects
- SonarQube integration for code quality
- Code coverage via Coverlet
- `.runsettings` configured for coverage collection

### CI/CD Pipeline
- GitHub Actions workflow in `.github/workflows/dotnet.yml`
- Custom PowerShell build module: `scripts/PSBuild.psm1`
- Automated versioning, building, testing, packaging, and releasing
- SonarQube analysis when `SONAR_TOKEN` is available
- Winget manifest generation for releases
- Security scanning via component detection

## Important Notes

### Console Output
- Console apps must set UTF-8 encoding at startup
- Use Spectre.Console's `AnsiConsole` for all console writing
- Core library should not output to console (functionality belongs in Demo app)

### Semantic Types
- Use validation attributes on semantic strings (see ktsu.SemanticString examples)
- Follow patterns from `https://github.com/ktsu-dev/SemanticString/blob/main/SemanticString/SemanticStringValidationAttributes.cs`

### Metadata Files
- `TAGS.md` and `DESCRIPTION.md` should have `status: draft` frontmatter
- Update README before releasing
- `CHANGELOG.md` tracked for version management
