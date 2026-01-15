# Keybinding Management Library

A comprehensive .NET library for managing keyboard shortcuts and keybindings with support for multiple profiles, command registration, and persistent storage.

## Features

-   **Multi-Profile Support**: Create and manage multiple keybinding profiles
-   **Command Registry**: Register and organize commands with categories
-   **Flexible Key Combinations**: Support for complex key combinations with multiple modifiers
-   **Persistent Storage**: Automatic saving and loading of profiles and commands
-   **Thread-Safe Operations**: Concurrent access support for multi-threaded applications
-   **SOLID Architecture**: Clean, extensible design following SOLID principles
-   **Comprehensive API**: Full-featured interfaces for all operations

## Quick Start

### Installation

Add a reference to the `Keybinding.Core` project in your application.

### Basic Usage

```csharp
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

// Initialize the keybinding manager
var manager = new KeybindingManager("./keybinding-data");
await manager.InitializeAsync();

// Create a default profile if none exist
manager.CreateDefaultProfile();

// Register some commands
var commands = new[]
{
    new Command("file.new", "New File", "Create a new file", "File"),
    new Command("file.save", "Save File", "Save the current file", "File"),
    new Command("edit.copy", "Copy", "Copy selected text", "Edit")
};

manager.RegisterCommands(commands);

// Set chord bindings
manager.Keybindings.BindChord("file.new", manager.Keybindings.ParseChord("Ctrl+N"));
manager.Keybindings.BindChord("file.save", manager.Keybindings.ParseChord("Ctrl+S"));
manager.Keybindings.BindChord("edit.copy", manager.Keybindings.ParseChord("Ctrl+C"));

// Find commands by chord
var chord = manager.Keybindings.ParseChord("Ctrl+S");
var commandId = manager.Keybindings.FindCommandByChord(chord);

// Save changes
await manager.SaveAsync();
```

## Core Concepts

### Commands

Commands represent actions that can be bound to keyboard shortcuts:

```csharp
var command = new Command(
    id: "file.save",           // Unique identifier
    name: "Save File",         // Display name
    description: "Save the current file",  // Optional description
    category: "File"           // Optional category for organization
);
```

### Chords (Key Combinations)

Chords represent musical-style key combinations using the Note and Chord classes:

```csharp
// Parse from string
var chord = manager.Keybindings.ParseChord("Ctrl+Alt+S");

// Create programmatically
var chord = new Chord([
    new Note("CTRL"),
    new Note("ALT"), 
    new Note("S")
]);

// Supported modifiers: Ctrl, Alt, Shift, Meta (Windows/Cmd key)
```

### Profiles

Profiles allow different keybinding configurations:

```csharp
// Create a new profile
var profile = new Profile("gaming", "Gaming Profile", "Keybindings for gaming");
manager.Profiles.CreateProfile(profile);

// Switch active profile
manager.Profiles.SetActiveProfile("gaming");

// Duplicate a profile
var newProfile = manager.Profiles.DuplicateProfile("default", "custom", "Custom Profile");
```

## Architecture

The library follows a clean architecture with clear separation of concerns:

### Models

-   **`Command`**: Represents a named action that can be bound to keys
-   **`Chord`**: Represents a keyboard shortcut with modifiers and primary key using musical paradigm
-   **`Note`**: Represents individual keys in a chord
-   **`Phrase`**: Represents sequences of chords for complex key combinations
-   **`Profile`**: Contains a set of chord bindings for commands

### Contracts (Interfaces)

-   **`ICommandRegistry`**: Command management operations
-   **`IProfileManager`**: Profile management operations
-   **`IKeybindingService`**: Keybinding coordination and validation
-   **`IKeybindingRepository`**: Persistence operations

### Services

-   **`CommandRegistry`**: Thread-safe command storage and retrieval
-   **`ProfileManager`**: Profile lifecycle management
-   **`KeybindingService`**: Coordinates keybinding operations across profiles and commands
-   **`JsonKeybindingRepository`**: JSON-based persistence implementation

### Main Facade

-   **`KeybindingManager`**: Main entry point that coordinates all services

## Advanced Usage

### Custom Repository

Implement your own storage mechanism:

```csharp
public class DatabaseKeybindingRepository : IKeybindingRepository
{
    // Implement async persistence methods
    public async Task SaveProfileAsync(Profile profile) { /* ... */ }
    public async Task<IEnumerable<Profile>> LoadAllProfilesAsync() { /* ... */ }
    // ... other methods
}

// Use with custom repository
var manager = new KeybindingManager(
    new CommandRegistry(),
    new ProfileManager(),
    new DatabaseKeybindingRepository()
);
```

### Batch Operations

```csharp
// Register multiple commands at once
var commands = new[]
{
    new Command("edit.cut", "Cut", "Cut to clipboard", "Edit"),
    new Command("edit.copy", "Copy", "Copy to clipboard", "Edit"),
    new Command("edit.paste", "Paste", "Paste from clipboard", "Edit")
};
int registered = manager.RegisterCommands(commands);

// Set multiple chord bindings
var chords = new Dictionary<string, Chord>
{
    { "edit.cut", manager.Keybindings.ParseChord("Ctrl+X") },
    { "edit.copy", manager.Keybindings.ParseChord("Ctrl+C") },
    { "edit.paste", manager.Keybindings.ParseChord("Ctrl+V") }
};
int set = manager.SetChords(chords);
```

### Profile Management

```csharp
// Create specialized profiles
var vimProfile = new Profile("vim", "Vim Emulation", "Vim-style keybindings");
manager.Profiles.CreateProfile(vimProfile);

// Switch between profiles
manager.Profiles.SetActiveProfile("vim");
// ... set vim-style keybindings

manager.Profiles.SetActiveProfile("default");
// ... back to default keybindings

// Duplicate and customize
var customProfile = manager.Profiles.DuplicateProfile("default", "custom", "My Custom Profile");
```

### Command Organization

```csharp
// Search commands by name
var fileCommands = manager.Commands.SearchCommands("file");

// Filter by category
var editCommands = manager.Commands.GetCommandsByCategory("Edit");

// Check if command exists
if (manager.Commands.CommandExists("file.save"))
{
    // Command is registered
}
```

## Sample Application

### Interactive Demo (`Keybinding.Demo`)

An interactive console application demonstrating all library features:

```bash
dotnet run --project Keybinding.Demo
```

The demo includes:

-   Status overview
-   Profile management
-   Command registration
-   Keybinding configuration
-   Phrase binding and lookup
-   Profile switching demonstration

## Data Storage

By default, the library stores data in JSON format in the specified directory:

```
data-directory/
├── commands.json          # Registered commands
├── active-profile.json    # Currently active profile ID
└── profiles/
    ├── default.json       # Default profile keybindings
    ├── gaming.json        # Gaming profile keybindings
    └── custom.json        # Custom profile keybindings
```

## Thread Safety

All services are designed to be thread-safe:

-   `CommandRegistry` uses `ConcurrentDictionary` for command storage
-   `ProfileManager` uses locking for profile operations
-   `KeybindingService` coordinates safely across services

## Error Handling

The library provides comprehensive error handling:

-   Invalid key combinations are rejected during parsing
-   Duplicate command IDs are prevented
-   Profile operations validate existence and constraints
-   Repository operations handle I/O errors gracefully

## Testing

The solution includes comprehensive unit tests in the `Keybinding.Test` project:

```bash
dotnet test
```

Tests cover:

-   Key combination parsing and validation
-   Command registration and retrieval
-   Profile management operations
-   Keybinding service coordination
-   Repository persistence operations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Architecture Principles

This library follows SOLID principles:

-   **Single Responsibility**: Each class has a focused purpose
-   **Open/Closed**: Extensible through interfaces without modification
-   **Liskov Substitution**: Implementations are interchangeable
-   **Interface Segregation**: Focused, specific interfaces
-   **Dependency Inversion**: Depends on abstractions, not concretions

The design also follows DRY (Don't Repeat Yourself) principles with consistent patterns across the codebase.
