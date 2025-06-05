using AzBae.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AzBae.Core.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Builder;

// Create the application builder
var builder = WebApplication.CreateBuilder();

// Configure to use environmental variables then to use appsettings.local.json
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Configure logging
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Load and validate settings
var azBaeSettings = new AzBaeSettings
{
    CosmosSettings = builder.Configuration.GetSection(CosmosAppSettings.SectionName).Get<CosmosAppSettings>(),
    ResourceFilters = builder.Configuration.GetSection(ResourceFilterSettings.SectionName).Get<ResourceFilterSettings>()
};
var settingsValidator = new AzBaeSettingsValidator();
settingsValidator.Validate(azBaeSettings, options => options.ThrowOnFailures());

// Register settings with DI
builder.Services.Configure<CosmosAppSettings>(
    builder.Configuration.GetSection(CosmosAppSettings.SectionName));
    
builder.Services.Configure<ResourceFilterSettings>(
    builder.Configuration.GetSection(ResourceFilterSettings.SectionName));

builder.Services.AddCore();


// Add the MCP server
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Build and run the host
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Configuration loaded. Any settings from appsettings.local.json are now available.");


host.MapMcp("/mcp");

await host.RunAsync();