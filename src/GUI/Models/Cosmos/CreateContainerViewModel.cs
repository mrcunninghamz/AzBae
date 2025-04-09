using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentValidation;
using GUI.Configuration;
using Microsoft.Extensions.Options;

namespace GUI.Models.Cosmos;

public partial class CreateContainerViewModel : ObservableObject
{
    public RelayCommand CreateContainerCommand { get; }
    
    private readonly CosmosAppSettings _cosmosAppSettings;

    [ObservableProperty]
    private string _accountEndpoint;

    [ObservableProperty] 
    private string _accountKey;
    
    [ObservableProperty]
    private string _databaseName;
    
    [ObservableProperty]
    private string _containerName;
    
    [ObservableProperty]
    private string _partitionKey;
    
    [ObservableProperty]
    private bool _canCreateContainer;

    public CreateContainerViewModel(IOptions<CosmosAppSettings> cosmosAppSettings)
    {
        _cosmosAppSettings = cosmosAppSettings.Value;

        CreateContainerCommand = new RelayCommand(async void () =>
        {
            await Create();
        });
    }
    
    public void Initialized ()
    {
        AccountEndpoint = _cosmosAppSettings.AccountEndpoint;
        AccountKey = _cosmosAppSettings.AccountKey;
        DatabaseName = _cosmosAppSettings.DatabaseName;
        ContainerName = _cosmosAppSettings.ContainerName;
        PartitionKey = _cosmosAppSettings.PartitionKey;
        Validate();
        
        SendMessage(CreateContainerActionType.Initialize);
    }

    public void Validate()
    {
        var validator = new CreateContainerViewModelValidator();
        CanCreateContainer = validator.Validate(this).IsValid;
        SendMessage(CreateContainerActionType.Validate);
    }

    private async Task Create()
    {
        SendMessage (CreateContainerActionType.CreateProgress);
        await Task.Delay (TimeSpan.FromSeconds (1));
    }
    
    
    
    private void SendMessage (CreateContainerActionType actionType, string message = "")
    {
        WeakReferenceMessenger.Default.Send(new Message<CreateContainerActionType> { Value = actionType });
    }
}

public class CreateContainerViewModelValidator : AbstractValidator<CreateContainerViewModel>
{
    public CreateContainerViewModelValidator()
    {
        RuleFor(x => x.AccountEndpoint).NotNull().NotEmpty();
        RuleFor(x => x.AccountKey).NotNull().NotEmpty();
        RuleFor(x => x.DatabaseName).NotNull().NotEmpty();
        RuleFor(x => x.ContainerName).NotNull().NotEmpty();
        RuleFor(x => x.PartitionKey).NotNull().NotEmpty();
    }
}

public enum CreateContainerActionType
{
    CreateProgress,
    ClearForm,
    Validate,
    Initialize
}
