using Spectre.Console.Cli;

namespace CLI.Commands;

public class CosmosCommandSettings : CommandSettings
{
    [CommandOption("-a|--account-endpoint")]
    public string? AccountEndpoint { get; set; }
    [CommandOption("-d|--database")]
    public string? DatabaseName { get; set; }
    [CommandOption("-c|--container")]
    public string? ContainerName { get; set; }
        
    [CommandOption("-p|--partitionKey")]
    public string? PartitionKey { get; set; }
}