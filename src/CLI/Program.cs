// See https://aka.ms/new-console-template for more information

using System.Management.Automation;
using System.Reflection;
using CLI.Actions;
using CLI.Commands;
using CLI.Configuration;
using CLI.Infrastructure;
using CLI.Models;
using Azure.Identity;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.local.json", optional: false, reloadOnChange: true);
 
IConfiguration config = builder.Build();
var settings = new AzBaeSettings
{
    CosmosSettings = config.GetSection(CosmosAppSettings.SectionName).Get<CosmosAppSettings>()
};
var settingsValidator = new AzBaeSettingsValidator();
settingsValidator.Validate(settings, options => options.ThrowOnFailures());

var credentials = new DefaultAzureCredential();

var services = new ServiceCollection();

services.Configure<CosmosAppSettings>(
    config.GetSection(CosmosAppSettings.SectionName));

var commandActions = Assembly.GetAssembly(typeof(Program))!.GetTypes()
    .Where(type => 
        type.GetInterfaces().Any(i => 
            i.IsGenericType 
            && i.GetGenericTypeDefinition() == typeof(IAzBaeAction<>)
        )
        && !type.IsAbstract
    ).ToList();

foreach(var commandAction in commandActions)
{
    services.AddSingleton(typeof(IAzBaeAction<CommandSettings>), commandAction);
}

services.AddSingleton(credentials);
services.AddSingleton<IActionProvider, ActionProvider>();

services.AddScoped(_ => PowerShell.Create());

services.AddAutoMapper(typeof(Program).Assembly);

// Create a type registrar and register any dependencies.
// A type registrar is an adapter for a DI framework.
var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.PropagateExceptions(); 
    config.SetApplicationName("azbae");

    config.AddCommand<MenuCommand>("menu");

    config.AddBranch<CosmosCommandSettings>("cosmos",
        cosmos =>
        {
            cosmos.AddBranch("container",
                container =>
                {
                    container.AddCommand<CosmosCreateContainerCommand>("create");
                });
        });

    // config.AddCommand<DeleteInventoryRecordsCommand>("delete-inventory-records");
    //     .IsHidden()
    //     .WithAlias("file-size")
    //     .WithDescription("Gets the file size for a directory.")
    //     .WithExample(new[] {"size", "c:\\windows", "--pattern", "*.dll"});
});

return app.Run(args);