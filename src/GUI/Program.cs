// See https://aka.ms/new-console-template for more information

using AzBae.Core;
using AzBae.Core.Configuration;
using Azure.Identity;
using FluentValidation;
using GUI.Models.Cosmos;
using GUI.Models.FunctionApps;
using GUI.Views.Cosmos;
using GUI.Views.FunctionApps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;

HostApplicationBuilderSettings settings = new()
{
    Args = args,
    Configuration = new ConfigurationManager(),
    ContentRootPath = Directory.GetCurrentDirectory(),
};

settings.Configuration.AddJsonFile("appsettings.local.json", optional: true);

var builder = Host.CreateApplicationBuilder(settings);

builder.Logging.ClearProviders();
builder.Services.AddLogging(builder =>
{
    var logger = new LoggerConfiguration()
        .MinimumLevel.Error()
        .WriteTo.File ("logs/logfile.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.AddSerilog(logger);
});
 
var azBaeSettings = new AzBaeSettings
{
    CosmosSettings = builder.Configuration.GetSection(CosmosAppSettings.SectionName).Get<CosmosAppSettings>(),
    ResourceFilters = builder.Configuration.GetSection(ResourceFilterSettings.SectionName).Get<ResourceFilterSettings>()
};
var settingsValidator = new AzBaeSettingsValidator();
settingsValidator.Validate(azBaeSettings, options => options.ThrowOnFailures());

builder.Services.Configure<CosmosAppSettings>(
    builder.Configuration.GetSection(CosmosAppSettings.SectionName));
    
builder.Services.Configure<ResourceFilterSettings>(
    builder.Configuration.GetSection(ResourceFilterSettings.SectionName));

builder.Services.AddSingleton<MainWindow>();

builder.Services.AddSingleton<CreateContainerView>();
builder.Services.AddSingleton<CreateContainerViewModel>();
builder.Services.AddSingleton<DeleteRecordsView>();
builder.Services.AddSingleton<DeleteRecordsViewModel>();

// Register Azure Function App components
builder.Services.AddSingleton<FunctionAppView>();
builder.Services.AddSingleton<ListMyFunctionAppsViewModel>();

builder.Services.AddHostedService<AppGui>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddCore();

using IHost host = builder.Build();

// Application code should start here.

await host.RunAsync();