using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentValidation;
using FluentValidation.Results;
using GUI.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace GUI.Models.Cosmos;

public partial class CreateContainerViewModel : BaseCosmosViewModel
{
    public RelayCommand CreateContainerCommand { get; }

    [ObservableProperty] 
    private string _accountKey;
    
    [ObservableProperty]
    private bool _canCreateContainer;

    public CreateContainerViewModel(IOptions<CosmosAppSettings> cosmosAppSettings) : base(cosmosAppSettings.Value)
    {
        CreateContainerCommand = new RelayCommand(async void () =>
        {
            await Create();
        });
    }
    
    public override void Initialized ()
    {
        base.Initialized();
        AccountKey = CosmosAppSettings.AccountKey;
        Validate();
        
        SendMessage(CreateContainerActionType.Initialize);
    }

    public override void Validate()
    {
        var validator = new CreateContainerViewModelValidator();
        var validationResult = validator.Validate(this);
        CanCreateContainer = validationResult.IsValid;
        SendMessage(CreateContainerActionType.Validate);
        
        Errors = string.Join(Environment.NewLine, validationResult.Errors);
    }

    private async Task Create()
    {
        SendMessage (CreateContainerActionType.CreateProgress);
        
        using CosmosClient cosmosClient = new(AccountEndpoint, AccountKey);
        SendMessage (CreateContainerActionType.CreateDatabaseProgress);
        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            id: DatabaseName
        );
        SendMessage (CreateContainerActionType.CreateContainerProgress);
        Container container = await database.CreateContainerIfNotExistsAsync(
            id: ContainerName,
            partitionKeyPath: $"/{PartitionKey}"
        );
        SendMessage (CreateContainerActionType.CreateFinished);
    }
}

public class CreateContainerViewModelValidator : BaseCosmosViewModelValidator<CreateContainerViewModel>
{
    public CreateContainerViewModelValidator()
    {
        RuleFor(x => x.AccountKey).NotNull().NotEmpty();
    }
}

public enum CreateContainerActionType
{
    CreateProgress,
    CreateDatabaseProgress,
    CreateContainerProgress,
    CreateFinished,
    ClearForm,
    Validate,
    Initialize
}
