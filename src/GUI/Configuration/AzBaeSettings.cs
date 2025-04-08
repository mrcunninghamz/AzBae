using FluentValidation;

namespace GUI.Configuration;

public class AzBaeSettings
{
    public CosmosAppSettings? CosmosSettings { get; init; }
}

public class AzBaeSettingsValidator : AbstractValidator<AzBaeSettings>
{
    public AzBaeSettingsValidator()
    {
        RuleFor(x => x.CosmosSettings).NotNull().SetValidator(new CosmosSettingsValidator()!);
    }
}

public class CosmosAppSettings
{
    public const string SectionName = "CosmosDb";
    public required string AccountEndpoint { get; init; }
    public required string AccountKey { get; init; }
    public required string DatabaseName { get; init; }
    public required string ContainerName { get; init; }
    public required string PartitionKey { get; init; }
}
public class CosmosSettingsValidator : AbstractValidator<CosmosAppSettings>
{
    public CosmosSettingsValidator()
    {
        RuleFor(x => x.AccountEndpoint).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:accountEndpoint"); 
        RuleFor(x => x.AccountKey).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:accountKey"); 
        RuleFor(x => x.DatabaseName).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:databaseName"); 
        RuleFor(x => x.ContainerName).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:containerName"); 
        RuleFor(x => x.PartitionKey).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:containerKey"); 
    }
}