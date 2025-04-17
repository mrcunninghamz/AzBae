using System.Management.Automation;
using CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands;

public class CosmosSetRBACCommand : AsyncCommand<CosmosSetRBACCommand.Settings>, IAzBaeCommand
{
    private readonly PowerShell _powershell;
    public string CommandName => AzBaeCommands.CosmosRBACCommandName;
    public string ParentCommand => AzBaeCommands.CosmosCommandName;
    public class Settings : CommandSettings
    {
        public string ResourceGroupName { get; set; }
    }

    public CosmosSetRBACCommand(PowerShell powershell)
    {
        _powershell = powershell;

    }
    
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrEmpty(settings.ResourceGroupName))
        {
            settings.ResourceGroupName = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the resource group cosmos is in:")
            );
        }
        _powershell.AddCommand("az").AddCommand("group").AddCommand("show");
        _powershell.AddParameter("--name", settings.ResourceGroupName);

        var pipelineObjects = await _powershell.InvokeAsync();
        
        return 0;
    }
}
