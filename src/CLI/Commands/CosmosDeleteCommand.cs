using CLI.Infrastructure;
using Azure.Identity;
using CLI.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace CLI.Commands;

public class CosmosDeleteCommand : AsyncCommand<CosmosDeleteCommand.Settings>, IAzBaeCommand
{
    public string CommandName => AzBaeCommands.CosmosDeleteCommandName;
    public string ParentCommand => AzBaeCommands.CosmosCommandName;
    public Type SettingsType => typeof(CosmosDeleteCommand.Settings);
    private readonly DefaultAzureCredential _credentials;
    private Container _container;
    private List<JObject> _results;
    private int _page;
    private int _pageSize = 10;
    private Settings _settings;
    
    public class Settings : CosmosCommandSettings
    {
        [CommandOption("-q|--query")]
        public string? Query { get; set; }
    }

    public CosmosDeleteCommand(DefaultAzureCredential credentials)
    {
        _credentials = credentials;

    }
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _settings = settings;
        
        if (string.IsNullOrEmpty(_settings.PartitionKey))
        {
            _settings.PartitionKey = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the container's partition key:")
            );
        }
        
        await QueryPromptAsync();
        return 0;
    }
    
    private async Task QueryPromptAsync()
    {
        if (string.IsNullOrEmpty(_settings.Query))
        {
            _settings.Query = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide a sql query:")
            );
        }
        
        using CosmosClient cosmosClient = new(
            accountEndpoint: _settings.AccountEndpoint,
            tokenCredential: _credentials
        );
        _container = cosmosClient.GetContainer(_settings.DatabaseName, _settings.ContainerName);

        _results = new List<JObject>();
        using var resultSet = _container.GetItemQueryIterator<JObject>(new QueryDefinition(_settings.Query), requestOptions: new QueryRequestOptions
        {
            MaxItemCount = -1
        });
        while (resultSet.HasMoreResults)
        {
            _results.AddRange(await resultSet.ReadNextAsync());
        }
        
        await MainPromptAsync();
    }

    private async Task MainPromptAsync()
    {
        _page = 0;
        var choices = new List<string>
        {
            "New Query"
        };

        if (_results.Any())
        {
            choices = choices.Prepend("Delete All").ToList();
            choices = choices.Prepend("Review").ToList();
        }
        
        var nextAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Your query returned [green]{_results.Count}[/] item(s).")
                .PageSize(10)
                .MoreChoicesText("What should we do next?")
                .AddChoices(choices)
        );

        await DoNextActionAsync(nextAction);
    }

    private async Task DoNextActionAsync(string action)
    {
        switch (action)
        {
            case "Review":
                await ShowJsonAsync(_results, _page, _pageSize);
                break;
            case "Delete All":
                await DeleteAllAsync();
                break;
            case "New Query":
                _settings.Query = string.Empty;
                await QueryPromptAsync();
                break;
        }
    }

    private async Task ShowJsonAsync(List<JObject> jObjects, int skip, int take)
    {
        if (jObjects.Count <= skip)
        {
            await MainPromptAsync();
        }
        
        var combined = string.Join(", ", jObjects.Skip(skip).Take(take).ToList());
        AnsiConsole.Write(
            new Panel(new JsonText($"[{combined}]"))
                .Header($"Result Page {_page + 1} of {Math.Ceiling(_results.Count/(double)_pageSize)} ")
                .Collapse()
                .RoundedBorder()
                .BorderColor(Color.Yellow));
        
        var nextAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"What should we do next?")
                .PageSize(10)
                .AddChoices(new[] {
                    "Next", "Exit"
                })
        );

        if (!nextAction.Equals("Exit"))
        {
            await ShowJsonAsync(_results, ++_page * _pageSize, _pageSize);
        }
        
        await MainPromptAsync();
    }

    private async Task DeleteAllAsync()
    {
        await AnsiConsole.Status()
            .StartAsync("Deleting records...", async ctx =>
            {
                var totalRecords = _results.Count;
                var recordNumber = 1;
                foreach (var toDelete in _results)
                {
                    var id = toDelete["id"].ToString();
                    var partitionKey = !string.IsNullOrEmpty(toDelete[_settings.PartitionKey!]?.ToString()) ? new PartitionKey(toDelete[_settings.PartitionKey!]!.ToString()) : PartitionKey.None;
                    AnsiConsole.MarkupLine($"deleting record {recordNumber} of {totalRecords}... {id}");
                    await _container.DeleteItemStreamAsync(id, partitionKey);
                    
                    AnsiConsole.MarkupLine($"deleting record {recordNumber} of {totalRecords}...[green]completed[/]");
                    recordNumber++;
                }
            });
        
        _results.Clear();
        await MainPromptAsync();
    }
}
