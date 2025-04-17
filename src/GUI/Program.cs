// See https://aka.ms/new-console-template for more information

using FluentValidation;
using GUI.Configuration;
using GUI.Models.Cosmos;
using GUI.Views.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;

HostApplicationBuilderSettings settings = new()
{
    Args = args,
    Configuration = new ConfigurationManager(),
    ContentRootPath = Directory.GetCurrentDirectory(),
};

settings.Configuration.AddJsonFile("appsettings.local.json", optional: true);

var builder = Host.CreateApplicationBuilder(settings);
 
var azBaeSettings = new AzBaeSettings
{
    CosmosSettings = builder.Configuration.GetSection(CosmosAppSettings.SectionName).Get<CosmosAppSettings>()
};
var settingsValidator = new AzBaeSettingsValidator();
settingsValidator.Validate(azBaeSettings, options => options.ThrowOnFailures());

builder.Services.Configure<CosmosAppSettings>(
    builder.Configuration.GetSection(CosmosAppSettings.SectionName));

builder.Services.AddSingleton<MainWindow>();

builder.Services.AddSingleton<CreateContainerView>();
builder.Services.AddSingleton<CreateContainerViewModel>();
builder.Services.AddSingleton<DeleteRecordsView>();
builder.Services.AddSingleton<DeleteRecordsViewModel>();

builder.Services.AddHostedService<AppGui>();


using IHost host = builder.Build();

// Application code should start here.

await host.RunAsync();