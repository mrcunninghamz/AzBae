using FluentValidation;

namespace GUI.Models.Cosmos;

public abstract class BaseCosmosViewModelValidator<T> : AbstractValidator<T> where T : BaseCosmosViewModel
{
    protected BaseCosmosViewModelValidator()
    {
        RuleFor(x => x.AccountEndpoint).NotNull().NotEmpty();
        RuleFor(x => x.DatabaseName).NotNull().NotEmpty();
        RuleFor(x => x.ContainerName).NotNull().NotEmpty();
        RuleFor(x => x.PartitionKey).NotNull().NotEmpty();
    }
}
