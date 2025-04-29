using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzBae.Core.Configuration;
using AzBae.MCP.Configuration;
using AzBae.MCP.Tools;
using FluentValidation;

// Create the application builder
var builder = Host.CreateApplicationBuilder(args);

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
    CosmosSettings = builder.Configuration.GetSection(CosmosAppSettings.SectionName).Get<CosmosAppSettings>()
};
var settingsValidator = new AzBaeMcpSettingsValidator();
settingsValidator.Validate(azBaeSettings, options => options.ThrowOnFailures());

// Register settings with DI
builder.Services.Configure<CosmosAppSettings>(builder.Configuration.GetSection(CosmosAppSettings.SectionName));

// Register CosmosDB tool
builder.Services.AddSingleton<CosmosDbTool>();

// Add the MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Build and run the host
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Configuration loaded. Any settings from appsettings.local.json are now available.");

await host.RunAsync();