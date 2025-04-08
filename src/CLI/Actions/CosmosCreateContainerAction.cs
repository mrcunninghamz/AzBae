using AutoMapper;
using CLI.Infrastructure;
using CLI.Commands;
using CLI.Configuration;
using CLI.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Actions;

public class CosmosCreateContainerAction : AzBaeAction<CosmosCreateContainerCommand.Settings>
{
    private readonly IMapper _mapper;
    private readonly CosmosAppSettings _cosmosAppSettings;
    public override string CommandName => AzBaeCommands.CosmosCreateContainerommandName;
    public override string ParentCommand => AzBaeCommands.CosmosCommandName;
    
    public CosmosCreateContainerAction(IOptions<CosmosAppSettings> cosmosSettings, IMapper mapper)
    {
        _mapper = mapper;
        _cosmosAppSettings = cosmosSettings.Value;
    }

    public async override Task<int> ExecuteAsync()
    {
        Settings = _mapper.Map(_cosmosAppSettings, Settings);
        Settings.PromptForSettings();
        
        if (string.IsNullOrEmpty(Settings.AccountKey))
        {
            Settings.AccountKey = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the account key:")
            );
        }
        
        using CosmosClient cosmosClient = new(Settings.AccountEndpoint, Settings.AccountKey);
        await AnsiConsole.Status()
            .StartAsync("Creating things...", async ctx =>
            {
                AnsiConsole.MarkupLine($"creating new database if it doesn't exist {Settings.DatabaseName}");
                Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                    id: Settings.DatabaseName
                );
                
                AnsiConsole.MarkupLine($"creating new container if it doesn't exist {Settings.ContainerName}");
                Container container = await database.CreateContainerIfNotExistsAsync(
                    id: Settings.ContainerName,
                    partitionKeyPath: $"/{Settings.PartitionKey}"
                );
                    
                AnsiConsole.MarkupLine($"Creation of {Settings.ContainerName} in {Settings.DatabaseName} [green]completed[/]");
            });
        
        return 0;
    }
}