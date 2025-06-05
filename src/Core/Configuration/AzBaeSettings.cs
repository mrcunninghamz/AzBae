using FluentValidation;

namespace AzBae.Core.Configuration;

public class AzBaeSettings
{
    public CosmosAppSettings? CosmosSettings { get; init; }
    public ResourceFilterSettings? ResourceFilters { get; init; }
}

public class AzBaeSettingsValidator : AbstractValidator<AzBaeSettings>
{
    public AzBaeSettingsValidator()
    {
        RuleFor(x => x.CosmosSettings).NotNull().SetValidator(new CosmosSettingsValidator()!);
        // Resource filters are optional, but if provided should be valid
        When(x => x.ResourceFilters != null, () =>
        {
            RuleFor(x => x.ResourceFilters).SetValidator(new ResourceFilterSettingsValidator()!);
        });
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

public class ResourceFilterSettings
{
    public const string SectionName = "ResourceFilters";
    public string? FunctionAppFilterPattern { get; init; }
    public string? FunctionAppWhere { get; init; }
}

public class ResourceFilterSettingsValidator : AbstractValidator<ResourceFilterSettings>
{
    public ResourceFilterSettingsValidator()
    {
        // No validation rules are enforced for filter patterns
        // Empty or null patterns should disable filtering
    }
}