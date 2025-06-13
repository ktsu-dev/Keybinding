using ktsu.Keybinding.Core;
using ktsu.Keybinding.Core.Contracts;
using ktsu.Keybinding.Core.Models;
using ktsu.Keybinding.Core.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register Keybinding services with dependency injection
builder.Services.AddSingleton<ICommandRegistry, CommandRegistry>();
builder.Services.AddSingleton<IProfileManager, ProfileManager>();
builder.Services.AddSingleton<IKeybindingRepository>(provider =>
    new JsonKeybindingRepository("./keybinding-data"));
builder.Services.AddSingleton<IKeybindingService, KeybindingService>();
builder.Services.AddSingleton<KeybindingManager>();

var app = builder.Build();

// Initialize keybinding manager on startup
using (var scope = app.Services.CreateScope())
{
    var keybindingManager = scope.ServiceProvider.GetRequiredService<KeybindingManager>();
    await keybindingManager.InitializeAsync();

    // Set up default data if needed
    keybindingManager.CreateDefaultProfile();

    // Register some sample commands for the API
    var sampleCommands = new[]
    {
        new Command("api.users.list", "List Users", "Get all users from the system", "API"),
        new Command("api.users.create", "Create User", "Create a new user", "API"),
        new Command("api.products.search", "Search Products", "Search for products", "API"),
        new Command("ui.refresh", "Refresh UI", "Refresh the user interface", "UI"),
        new Command("ui.save", "Save", "Save current changes", "UI")
    };

    keybindingManager.RegisterCommands(sampleCommands);

    // Set up some default keybindings
    var defaultBindings = new Dictionary<string, Chord>
    {
        ["ui.refresh"] = keybindingManager.Keybindings.ParseChord("F5"),
        ["ui.save"] = keybindingManager.Keybindings.ParseChord("Ctrl+S"),
        ["api.users.list"] = keybindingManager.Keybindings.ParseChord("Ctrl+U"),
        ["api.products.search"] = keybindingManager.Keybindings.ParseChord("Ctrl+F")
    };

    keybindingManager.SetChords(defaultBindings);
    await keybindingManager.SaveAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Add some simple endpoints to demonstrate keybinding functionality
app.MapGet("/api/keybindings", (KeybindingManager manager) =>
{
    var activeProfile = manager.Profiles.GetActiveProfile();
    var allCommands = manager.Commands.GetAllCommands();

    var keybindings = allCommands.Select(cmd => new
    {
        CommandId = cmd.Id,
        Name = cmd.Name,
        Description = cmd.Description,
        Category = cmd.Category,
        Keybinding = manager.Keybindings.GetChordForCommand(cmd.Id)?.ToString() ?? "None"
    });

    return Results.Ok(new
    {
        ActiveProfile = activeProfile?.Name ?? "None",
        TotalCommands = allCommands.Count,
        Keybindings = keybindings
    });
})
.WithName("GetKeybindings")
.WithOpenApi();

app.MapPost("/api/keybindings/{commandId}", (string commandId, string keybinding, KeybindingManager manager) =>
{
    try
    {
        var chord = manager.Keybindings.ParseChord(keybinding);
        var success = manager.Keybindings.BindChord(commandId, chord);

        if (success)
        {
            _ = Task.Run(async () => await manager.SaveAsync());
            return Results.Ok(new { Success = true, Message = $"Keybinding set: {keybinding} -> {commandId}" });
        }
        else
        {
            return Results.BadRequest(new { Success = false, Message = "Failed to set keybinding" });
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
})
.WithName("SetKeybinding")
.WithOpenApi();

app.MapGet("/api/commands", (KeybindingManager manager) =>
{
    var commands = manager.Commands.GetAllCommands();
    var commandsWithBindings = commands.Select(cmd => new
    {
        Id = cmd.Id,
        Name = cmd.Name,
        Description = cmd.Description,
        Category = cmd.Category,
        Keybinding = manager.Keybindings.GetChordForCommand(cmd.Id)?.ToString() ?? "None"
    });

    return Results.Ok(commandsWithBindings);
})
.WithName("GetCommands")
.WithOpenApi();

app.MapGet("/api/profiles", (KeybindingManager manager) =>
{
    var profiles = manager.Profiles.GetAllProfiles();
    var activeProfile = manager.Profiles.GetActiveProfile();

    var profileInfo = profiles.Select(p => new
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        IsActive = p.Id == activeProfile?.Id,
        KeybindingCount = manager.Keybindings.GetAllChords().Count()
    });

    return Results.Ok(profileInfo);
})
.WithName("GetProfiles")
.WithOpenApi();

app.MapPost("/api/profiles/{profileId}/activate", (string profileId, KeybindingManager manager) =>
{
    try
    {
        var success = manager.Profiles.SetActiveProfile(profileId);
        if (success)
        {
            _ = Task.Run(async () => await manager.SaveAsync());
            return Results.Ok(new { Success = true, Message = $"Activated profile: {profileId}" });
        }
        else
        {
            return Results.NotFound(new { Success = false, Message = "Profile not found" });
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
})
.WithName("ActivateProfile")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
