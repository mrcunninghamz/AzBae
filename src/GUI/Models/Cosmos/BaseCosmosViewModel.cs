using CommunityToolkit.Mvvm.ComponentModel;
using GUI.Configuration;

namespace GUI.Models.Cosmos;

public abstract partial class BaseCosmosViewModel : BaseViewModel<CreateContainerActionType>
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
    public override void Initialized()
    {
        AccountEndpoint = CosmosAppSettings.AccountEndpoint;
        DatabaseName = CosmosAppSettings.DatabaseName;
        ContainerName = CosmosAppSettings.ContainerName;
        PartitionKey = CosmosAppSettings.PartitionKey;
    }
}