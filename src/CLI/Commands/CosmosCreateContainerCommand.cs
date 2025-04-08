using CLI.Actions;
using CLI.Infrastructure;
using CLI.Models;
using Spectre.Console.Cli;

namespace CLI.Commands;

public class CosmosCreateContainerCommand : AsyncCommand<CosmosCreateContainerCommand.Settings>, IAzBaeCommand
{
    private readonly IActionProvider _actionProvider;
    public string CommandName => AzBaeCommands.CosmosCreateContainerommandName;
    public string ParentCommand => AzBaeCommands.CosmosCommandName;
    
    public class Settings : CosmosCommandSettings
    {
        [CommandOption("-k|--cosmos-key")]
        public string? AccountKey { get; set; }
    }
    
    public CosmosCreateContainerCommand(IActionProvider actionProvider)
    {
        _actionProvider = actionProvider;
    }

    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var action = _actionProvider.GetAction<CosmosCreateContainerAction>(CommandName);
        action.Settings = settings;
        return await action.RunActionAsync();
    }
}
