using CLI.Commands;
using CLI.Models;
using Spectre.Console.Cli;

namespace CLI.Actions;

public class CosmosDeleteAction : AzBaeAction<CosmosDeleteCommand.Settings>
{
    public override string CommandName => AzBaeCommands.CosmosDeleteCommandName;
    public override string ParentCommand => AzBaeCommands.CosmosCommandName;
    public override Task<int> ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}
