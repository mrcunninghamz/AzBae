using CLI.Commands;
using CLI.Models;
using Spectre.Console.Cli;

namespace CLI.Actions;

public class CosmosUtilityAction : AzBaeAction<CosmosCommandSettings>
{
    public override string CommandName => AzBaeCommands.CosmosCommandName;
    public override string ParentCommand => "Menu";
    public override Task<int> ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}
