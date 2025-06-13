---
status: draft
---

# Keybinding Library Usage Examples

This document demonstrates how to use the Keybinding library in various scenarios, including standalone usage and with dependency injection, following SOLID and DRY principles.

## Table of Contents
1. [Standalone Usage](#standalone-usage)
2. [Dependency Injection Usage](#dependency-injection-usage)
3. [Custom Repository Implementation](#custom-repository-implementation)
4. [Advanced Configuration](#advanced-configuration)
5. [Factory Pattern Usage](#factory-pattern-usage)

## Standalone Usage

### Basic Standalone Usage

```csharp
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;

// Create a keybinding manager with default JSON repository
var manager = new KeybindingManager("./keybinding-data");
await manager.InitializeAsync();

// Create a default profile if none exist
manager.CreateDefaultProfile();

// Register commands
var commands = new[]
{
    new Command("file.new", "New File", "Create a new file", "File"),
    new Command("file.save", "Save File", "Save the current file", "File"),
    new Command("edit.copy", "Copy", "Copy selected text", "Edit")
};

manager.RegisterCommands(commands);

// Set keybindings
manager.Keybindings.BindChord("file.new", manager.Keybindings.ParseChord("Ctrl+N"));
manager.Keybindings.BindChord("file.save", manager.Keybindings.ParseChord("Ctrl+S"));
manager.Keybindings.BindChord("edit.copy", manager.Keybindings.ParseChord("Ctrl+C"));

// Save changes
await manager.SaveAsync();

// Later, find commands by chord
var chord = manager.Keybindings.ParseChord("Ctrl+S");
var commandId = manager.Keybindings.FindCommandByChord(chord);
Console.WriteLine($"Ctrl+S is bound to: {commandId}");
```

### Using Configuration Objects

```csharp
using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Contracts;

// Create configuration with custom settings
var config = new KeybindingConfiguration(
    dataDirectory: "./my-keybinding-data",
    defaultProfileId: "my-default",
    defaultProfileName: "My Default Profile",
    autoSave: true,
    autoSaveIntervalMilliseconds: 3000
);

// Use configuration with services
var commandRegistry = new CommandRegistry();
var profileManager = new ProfileManager();
var repository = new JsonKeybindingRepository(config.DataDirectory);

var manager = new KeybindingManager(commandRegistry, profileManager, repository);
await manager.InitializeAsync();
```

## Dependency Injection Usage

### ASP.NET Core Integration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ktsu.Keybinding.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add keybinding services with default configuration
builder.Services.AddKeybinding("./app-keybindings");

// Or with custom repository
builder.Services.AddKeybinding<CustomKeybindingRepository>();

var app = builder.Build();

// Use in controllers or services
[ApiController]
[Route("[controller]")]
public class KeybindingController : ControllerBase
{
    private readonly KeybindingManager _keybindingManager;
    private readonly IKeybindingManagerFactory _factory;

    public KeybindingController(
        KeybindingManager keybindingManager,
        IKeybindingManagerFactory factory)
    {
        _keybindingManager = keybindingManager;
        _factory = factory;
    }

    [HttpPost("commands")]
    public async Task<IActionResult> RegisterCommand([FromBody] Command command)
    {
        if (_keybindingManager.Commands.RegisterCommand(command))
        {
            await _keybindingManager.SaveAsync();
            return Ok();
        }
        return Conflict("Command already exists");
    }

    [HttpPost("bindings")]
    public async Task<IActionResult> SetBinding([FromBody] SetBindingRequest request)
    {
        var chord = _keybindingManager.Keybindings.ParseChord(request.ChordString);
        if (_keybindingManager.Keybindings.BindChord(request.CommandId, chord))
        {
            await _keybindingManager.SaveAsync();
            return Ok();
        }
        return BadRequest("Failed to set binding");
    }
}

public record SetBindingRequest(string CommandId, string ChordString);
```

### Console Application with DI

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ktsu.Keybinding.Core.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add keybinding services as scoped (for per-operation isolation)
        services.AddKeybindingScoped("./console-keybindings");
        
        // Register your application services
        services.AddScoped<IMyApplicationService, MyApplicationService>();
    })
    .Build();

// Get services and use them
using var scope = host.Services.CreateScope();
var keybindingManager = scope.ServiceProvider.GetRequiredService<KeybindingManager>();
var appService = scope.ServiceProvider.GetRequiredService<IMyApplicationService>();

await keybindingManager.InitializeAsync();
await appService.RunAsync();

public interface IMyApplicationService
{
    Task RunAsync();
}

public class MyApplicationService : IMyApplicationService
{
    private readonly KeybindingManager _keybindingManager;
    private readonly ILogger<MyApplicationService> _logger;

    public MyApplicationService(
        KeybindingManager keybindingManager,
        ILogger<MyApplicationService> logger)
    {
        _keybindingManager = keybindingManager;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Initializing keybinding system...");
        
        _keybindingManager.CreateDefaultProfile();
        
        // Your application logic here
        var commands = GetApplicationCommands();
        _keybindingManager.RegisterCommands(commands);
        
        await _keybindingManager.SaveAsync();
        _logger.LogInformation("Keybinding system initialized successfully");
    }

    private IEnumerable<Command> GetApplicationCommands()
    {
        // Return your application's commands
        return new[]
        {
            new Command("app.quit", "Quit Application", "Exit the application", "Application"),
            new Command("app.help", "Show Help", "Display help information", "Application")
        };
    }
}
```

## Custom Repository Implementation

### Database Repository Example

```csharp
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;
using Microsoft.EntityFrameworkCore;

public class DatabaseKeybindingRepository : IKeybindingRepository
{
    private readonly KeybindingDbContext _context;

    public DatabaseKeybindingRepository(KeybindingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> IsInitializedAsync()
    {
        return await _context.Database.CanConnectAsync();
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task SaveProfileAsync(Profile profile)
    {
        var existing = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id);

        if (existing != null)
        {
            // Update existing
            _context.Entry(existing).CurrentValues.SetValues(profile);
        }
        else
        {
            // Add new
            _context.Profiles.Add(profile);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Profile?> LoadProfileAsync(string profileId)
    {
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == profileId);
    }

    public async Task<IReadOnlyCollection<Profile>> LoadAllProfilesAsync()
    {
        var profiles = await _context.Profiles.ToListAsync();
        return profiles.AsReadOnly();
    }

    public async Task DeleteProfileAsync(string profileId)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == profileId);
        
        if (profile != null)
        {
            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveCommandsAsync(IEnumerable<Command> commands)
    {
        // Clear existing commands
        _context.Commands.RemoveRange(_context.Commands);
        
        // Add new commands
        _context.Commands.AddRange(commands);
        
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Command>> LoadCommandsAsync()
    {
        var commands = await _context.Commands.ToListAsync();
        return commands.AsReadOnly();
    }

    public async Task SaveActiveProfileAsync(string? profileId)
    {
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == "ActiveProfile");

        if (setting != null)
        {
            setting.Value = profileId;
        }
        else
        {
            _context.Settings.Add(new Setting
            {
                Key = "ActiveProfile",
                Value = profileId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string?> LoadActiveProfileAsync()
    {
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == "ActiveProfile");
        
        return setting?.Value;
    }
}

// Register with DI
builder.Services.AddDbContext<KeybindingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddKeybinding<DatabaseKeybindingRepository>();
```

## Advanced Configuration

### Configuration with Options Pattern

```csharp
using Microsoft.Extensions.Options;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;

// Configuration class
public class KeybindingOptions
{
    public const string SectionName = "Keybinding";
    
    public string DataDirectory { get; set; } = "";
    public string DefaultProfileId { get; set; } = "default";
    public string DefaultProfileName { get; set; } = "Default";
    public bool AutoSave { get; set; } = true;
    public int AutoSaveIntervalMilliseconds { get; set; } = 5000;
}

// Configuration adapter
public class OptionsKeybindingConfiguration : IKeybindingConfiguration
{
    private readonly KeybindingOptions _options;

    public OptionsKeybindingConfiguration(IOptions<KeybindingOptions> options)
    {
        _options = options.Value;
    }

    public string DataDirectory => _options.DataDirectory;
    public string DefaultProfileId => _options.DefaultProfileId;
    public string DefaultProfileName => _options.DefaultProfileName;
    public bool AutoSave => _options.AutoSave;
    public int AutoSaveIntervalMilliseconds => _options.AutoSaveIntervalMilliseconds;
}

// Register with DI
builder.Services.Configure<KeybindingOptions>(
    builder.Configuration.GetSection(KeybindingOptions.SectionName));

builder.Services.AddSingleton<IKeybindingConfiguration, OptionsKeybindingConfiguration>();
builder.Services.AddKeybinding<JsonKeybindingRepository>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IKeybindingConfiguration>();
    return new JsonKeybindingRepository(config.DataDirectory);
});
```

### appsettings.json Configuration

```json
{
  "Keybinding": {
    "DataDirectory": "./app-data/keybindings",
    "DefaultProfileId": "application-default",
    "DefaultProfileName": "Application Default",
    "AutoSave": true,
    "AutoSaveIntervalMilliseconds": 3000
  }
}
```

## Factory Pattern Usage

### Using Factory for Dynamic Manager Creation

```csharp
public class MultiTenantKeybindingService
{
    private readonly IKeybindingManagerFactory _factory;
    private readonly ConcurrentDictionary<string, KeybindingManager> _tenantManagers;

    public MultiTenantKeybindingService(IKeybindingManagerFactory factory)
    {
        _factory = factory;
        _tenantManagers = new ConcurrentDictionary<string, KeybindingManager>();
    }

    public async Task<KeybindingManager> GetManagerForTenantAsync(string tenantId)
    {
        return _tenantManagers.GetOrAdd(tenantId, async id =>
        {
            var dataDirectory = Path.Combine("./tenant-data", id, "keybindings");
            var manager = _factory.CreateManager(dataDirectory);
            await manager.InitializeAsync();
            return manager;
        });
    }

    public async Task<bool> SetTenantKeybindingAsync(
        string tenantId,
        string commandId,
        string chordString)
    {
        var manager = await GetManagerForTenantAsync(tenantId);
        var chord = manager.Keybindings.ParseChord(chordString);
        
        if (manager.Keybindings.BindChord(commandId, chord))
        {
            await manager.SaveAsync();
            return true;
        }
        
        return false;
    }
}

// Register in DI
builder.Services.AddKeybinding();
builder.Services.AddScoped<MultiTenantKeybindingService>();
```

## Best Practices

1. **Use Dependency Injection**: Register services with the DI container for better testability and maintainability.

2. **Follow SOLID Principles**: The library is designed with SOLID principles in mind - use interfaces for dependencies and follow single responsibility.

3. **Handle Async Operations**: Always await async operations and handle exceptions appropriately.

4. **Dispose Resources**: The KeybindingManager implements IDisposable - ensure proper disposal in long-running applications.

5. **Configuration Management**: Use the configuration abstraction for flexible setup across different environments.

6. **Testing**: Mock interfaces for unit testing rather than concrete implementations.

```csharp
// Example unit test setup
[Test]
public async Task Should_Register_Command_Successfully()
{
    // Arrange
    var mockCommandRegistry = new Mock<ICommandRegistry>();
    var mockProfileManager = new Mock<IProfileManager>();
    var mockRepository = new Mock<IKeybindingRepository>();
    
    mockCommandRegistry.Setup(x => x.RegisterCommand(It.IsAny<Command>()))
        .Returns(true);
    
    var manager = new KeybindingManager(
        mockCommandRegistry.Object,
        mockProfileManager.Object,
        mockRepository.Object);
    
    var command = new Command("test.command", "Test Command", "Test", "Test");
    
    // Act
    var result = manager.Commands.RegisterCommand(command);
    
    // Assert
    Assert.IsTrue(result);
    mockCommandRegistry.Verify(x => x.RegisterCommand(command), Times.Once);
}
```

This comprehensive example demonstrates how the Keybinding library implements and can be used with SOLID and DRY principles in both standalone and dependency injection scenarios. 
