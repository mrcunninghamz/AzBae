using CommunityToolkit.Mvvm.ComponentModel;
using FluentValidation;
using GUI.Configuration;

namespace GUI.Models.Cosmos;

public abstract partial class BaseCosmosViewModel<T> :  BaseViewModel<T> where T : Enum
{
    protected readonly CosmosAppSettings CosmosAppSettings;

    [ObservableProperty] 
    private string _accountEndpoint;
    
    [ObservableProperty]
    private string _databaseName;
    
    [ObservableProperty]
    private string _containerName;
    
    [ObservableProperty]
    private string _partitionKey;

    public BaseCosmosViewModel(CosmosAppSettings cosmosAppSettings)
    {
        CosmosAppSettings = cosmosAppSettings;
    }
    public override void Initialize()
    {
        AccountEndpoint = CosmosAppSettings.AccountEndpoint;
        DatabaseName = CosmosAppSettings.DatabaseName;
        ContainerName = CosmosAppSettings.ContainerName;
        PartitionKey = CosmosAppSettings.PartitionKey;
    }
}

public abstract class BaseCosmosViewModelValidator<T, TType> : AbstractValidator<T> 
    where T : BaseCosmosViewModel<TType>
    where TType : Enum
{
    protected BaseCosmosViewModelValidator()
    {
        RuleFor(x => x.AccountEndpoint).NotNull().NotEmpty();
        RuleFor(x => x.DatabaseName).NotNull().NotEmpty();
        RuleFor(x => x.ContainerName).NotNull().NotEmpty();
        RuleFor(x => x.PartitionKey).NotNull().NotEmpty();
    }
}
