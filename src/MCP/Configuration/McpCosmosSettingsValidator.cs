using AzBae.Core.Configuration;
using FluentValidation;

namespace AzBae.MCP.Configuration;

public class AzBaeMcpSettingsValidator : AbstractValidator<AzBaeSettings>
{
    public AzBaeMcpSettingsValidator()
    {
        RuleFor(x => x.CosmosSettings).NotNull().SetValidator(new McpCosmosSettingsValidator()!);
    }
}

public class McpCosmosSettingsValidator : AbstractValidator<CosmosAppSettings>
{
    public McpCosmosSettingsValidator()
    {
        RuleFor(x => x.AccountEndpoint).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:accountEndpoint"); 
        RuleFor(x => x.AccountKey).NotNull().NotEmpty().WithMessage("Please ensure that you have entered CosmosDb:accountKey"); 
    }
}