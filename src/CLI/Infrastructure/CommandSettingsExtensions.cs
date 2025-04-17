using CLI.Commands;
using Spectre.Console;

namespace CLI.Infrastructure;

public static class CommandSettingsExtensions
{
    public static void PromptForSettings(this CosmosCommandSettings commandSettings)
    {
        
        if (string.IsNullOrEmpty(commandSettings.AccountEndpoint))
        {
            commandSettings.AccountEndpoint = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the cosmos account endpoint:")
            );
        }
        
        if (string.IsNullOrEmpty(commandSettings.DatabaseName))
        {
            commandSettings.DatabaseName = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the cosmos Database:")
            );
        }
        
        if (string.IsNullOrEmpty(commandSettings.ContainerName))
        {
            commandSettings.ContainerName = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the cosmos Container:")
            );
        }
        
        if (string.IsNullOrEmpty(commandSettings.PartitionKey))
        {
            commandSettings.PartitionKey = AnsiConsole.Prompt(
                new TextPrompt<string>("Please provide the container partition key:")
            );
        }
    }
}
