using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using GUI.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace GUI.Models.Cosmos;

public partial class CreateContainerViewModel : BaseCosmosViewModel<CreateContainerActionTypes>
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
    
    public override void Initialize()
    {
        base.Initialize();
        
        AccountKey = CosmosAppSettings.AccountKey;
        Validate();
        
        SendMessage(CreateContainerActionTypes.Initialize);
    }

    public override void Validate()
    {
        var validator = new CreateContainerViewModelValidator();
        var validationResult = validator.Validate(this);
        CanCreateContainer = validationResult.IsValid;
        SendMessage(CreateContainerActionTypes.Validate);
        
        Errors = string.Join(Environment.NewLine, validationResult.Errors);
    }
    protected override void DisposeManaged()
    {
    }
    protected override void DisposeUnmanaged()
    {
    }

    private async Task Create()
    {
        SendMessage (CreateContainerActionTypes.CreateProgress);
        
        using CosmosClient cosmosClient = new(AccountEndpoint, AccountKey);
        SendMessage (CreateContainerActionTypes.CreateDatabaseProgress);
        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            id: DatabaseName
        );
        SendMessage (CreateContainerActionTypes.CreateContainerProgress);
        Container container = await database.CreateContainerIfNotExistsAsync(
            id: ContainerName,
            partitionKeyPath: $"/{PartitionKey}"
        );
        SendMessage (CreateContainerActionTypes.CreateFinished);
    }
}

public class CreateContainerViewModelValidator : BaseCosmosViewModelValidator<CreateContainerViewModel, CreateContainerActionTypes>
{
    public CreateContainerViewModelValidator()
    {
        RuleFor(x => x.AccountKey).NotNull().NotEmpty();
    }
}

public enum CreateContainerActionTypes
{
    CreateProgress,
    CreateDatabaseProgress,
    CreateContainerProgress,
    CreateFinished,
    ClearForm,
    Validate,
    Initialize
}
