using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Models;


public interface IAzBaeCommand
{
    string CommandName { get; }
    
    string ParentCommand { get; }
}

public interface IAzBaeAction<out T> where T : CommandSettings
{
    string CommandName { get; }
    
    string ParentCommand { get; }
    
    // T Settings { get; set; }
    IEnumerable<IAzBaeAction<CommandSettings>>? AvailableActions { get; set; }
    
    Task<int> RunActionAsync();
}

public static class AzBaeCommands
{
    public const string CosmosCommandName = "Cosmos";
    public const string CosmosCreateContainerommandName = "Create Container";
    public const string CosmosDeleteCommandName = "Delete";
    public const string CosmosRBACCommandName = "Set RBAC";
}