# Keybinding Web API Example

This example demonstrates how to integrate the `ktsu.Keybinding.Core` library into an ASP.NET Core Web API application using dependency injection.

## Features Demonstrated

- **Dependency Injection Setup**: How to register Keybinding services in the DI container
- **Initialization**: Proper initialization of the KeybindingManager during application startup
- **REST API Endpoints**: RESTful endpoints for managing keybindings, commands, and profiles
- **Error Handling**: Proper error handling for keybinding operations
- **Async Operations**: Asynchronous saving and loading of keybinding data

## API Endpoints

### Get All Keybindings
```
GET /api/keybindings
```
Returns all commands with their current keybinding assignments.

### Set Keybinding
```
POST /api/keybindings/{commandId}?keybinding={chord}
```
Sets a keybinding for a specific command.

Example:
```bash
curl -X POST "https://localhost:7000/api/keybindings/ui.save?keybinding=Ctrl+S"
```

### Get All Commands
```
GET /api/commands
```
Returns all registered commands with their keybinding status.

### Get All Profiles
```
GET /api/profiles
```
Returns all keybinding profiles with their status.

### Activate Profile
```
POST /api/profiles/{profileId}/activate
```
Switches to a different keybinding profile.

## Running the Example

1. Navigate to the project directory:
   ```bash
   cd Examples/Keybinding.WebAPI.Example
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser to `https://localhost:7000/swagger` to explore the API using Swagger UI.

## Key Integration Points

### 1. Service Registration
```csharp
// Register Keybinding services with dependency injection
builder.Services.AddSingleton<ICommandRegistry, CommandRegistry>();
builder.Services.AddSingleton<IProfileManager, ProfileManager>();
builder.Services.AddSingleton<IKeybindingRepository>(provider =>
    new JsonKeybindingRepository("./keybinding-data"));
builder.Services.AddSingleton<IKeybindingService, KeybindingService>();
builder.Services.AddSingleton<KeybindingManager>();
```

### 2. Initialization During Startup
```csharp
using (var scope = app.Services.CreateScope())
{
    var keybindingManager = scope.ServiceProvider.GetRequiredService<KeybindingManager>();
    await keybindingManager.InitializeAsync();
    keybindingManager.CreateDefaultProfile();
    // Set up sample data...
}
```

### 3. Controller Injection
```csharp
app.MapGet("/api/keybindings", (KeybindingManager manager) =>
{
    // Use the injected KeybindingManager
    var activeProfile = manager.Profiles.GetActiveProfile();
    // ... rest of endpoint logic
});
```

## Architecture Benefits

- **Separation of Concerns**: Business logic is cleanly separated from web API concerns
- **Testability**: Services can be easily mocked for unit testing
- **Scalability**: Singleton pattern ensures efficient resource usage
- **Maintainability**: Clear dependency injection makes the codebase easier to maintain

## Sample Usage Scenarios

1. **Configuration Management**: Use the API to manage application keybindings remotely
2. **User Preferences**: Allow users to customize their keyboard shortcuts via web interface
3. **Admin Tools**: Provide administrative interfaces for managing global keybinding profiles
4. **Integration Testing**: Use the REST endpoints to test keybinding functionality

## Error Handling

The API includes comprehensive error handling:
- Invalid chord format parsing
- Command not found scenarios  
- Profile activation failures
- Persistence operation errors

Each endpoint returns appropriate HTTP status codes and error messages for proper client-side handling. 
