using System.Collections.ObjectModel;
using Azure.Identity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using FluentValidation;
using GUI.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Terminal.Gui;

namespace GUI.Models.Cosmos;

public partial class DeleteRecordsViewModel : BaseCosmosViewModel<DeleteRecordsActionTypes>
{
    public RelayCommand RunQueryCommand { get; }
    
    public RelayCommand NextPageCommand { get; }
    
    public RelayCommand DeleteCommand { get; }
    
    [ObservableProperty] 
    private string _query;

    [ObservableProperty] 
    private bool _canRunQuery;
    
    [ObservableProperty] 
    private bool _hasMoreTableData;
    
    [ObservableProperty] 
    private string _message;
    
    [ObservableProperty] 
    private string _dataTableTitle;
    
    [ObservableProperty]
    private List<JObject> _results = [];
    
    [ObservableProperty]
    private List<JObject> _tableData = [];

    private int _page;
    private int _pageSize = 10;
    private Container _container;
    private CosmosClient _cosmosClient;

    public DeleteRecordsViewModel(IOptions<CosmosAppSettings> cosmosAppSettings) : base(cosmosAppSettings.Value)
    {
        RunQueryCommand = new RelayCommand(async void () =>
        {
            await RunQueryAsync();
        });

        NextPageCommand = new RelayCommand(() =>
        {
            ShowJson(Results, ++_page * _pageSize, _pageSize);
        });
        
        DeleteCommand = new RelayCommand(async void () =>
        {
            SendMessage(DeleteRecordsActionTypes.DeleteRunning);
            await DeleteAllAsync();
            SendMessage(DeleteRecordsActionTypes.DeleteFinished);
        });
    }

    public override void Initialize()
    {
        base.Initialize();
        
        Validate();
        SendMessage(DeleteRecordsActionTypes.Initialize);
        
        var credentials = new DefaultAzureCredential();
        _cosmosClient = new CosmosClient(
            accountEndpoint: AccountEndpoint,
            tokenCredential: credentials
        );
    }
    
    public override void Validate()
    {
        var validator = new DeleteRecordsViewModelValidator();
        var validationResult = validator.Validate(this);
        CanRunQuery = validationResult.IsValid;

        if (!validationResult.IsValid)
        {
            Errors = string.Join("\n", validationResult.Errors.Select(x => x.ErrorMessage));
        }
    }
    protected override void DisposeManaged()
    {
        _cosmosClient.Dispose();
    }
    protected override void DisposeUnmanaged()
    {
        // No unmanaged resources to dispose.
    }

    private async Task RunQueryAsync()
    {
        SendMessage(DeleteRecordsActionTypes.QueryRunning);
        _container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        
        Results = new List<JObject>();
        using var resultSet = _container.GetItemQueryIterator<JObject>(new QueryDefinition(Query), requestOptions: new QueryRequestOptions
        {
            MaxItemCount = -1
        });
        while (resultSet.HasMoreResults)
        {
            Results.AddRange(await resultSet.ReadNextAsync());
        }
        SendMessage(DeleteRecordsActionTypes.QueryFinished);
        
        ShowJson(Results, _page, _pageSize);
    }
    
    private void ShowJson(List<JObject> jObjects, int skip, int take)
    {
        TableData = jObjects.Skip(skip).Take(take).ToList();
        var currentPage = _page + 1;
        var totalPages = Math.Ceiling(Results.Count / (double)_pageSize);
        // title of some sort
        DataTableTitle = $"Result Page {currentPage} of {totalPages} ";
        
        HasMoreTableData = currentPage < totalPages;

        SendMessage(DeleteRecordsActionTypes.TableDataUpdated);
    }

    private async Task DeleteAllAsync()
    {
        var totalRecords = Results.Count;
        var recordNumber = 1;
        foreach (var toDelete in Results)
        {
            var id = toDelete["id"].ToString();
            var partitionKey = !string.IsNullOrEmpty(toDelete[PartitionKey!]?.ToString()) ? new PartitionKey(toDelete[PartitionKey!]!.ToString()) : Microsoft.Azure.Cosmos.PartitionKey.None;

            Message = $"deleting record {recordNumber} of {totalRecords}... {id}";
            
            await _container.DeleteItemStreamAsync(id, partitionKey);
            
            Message = $"deleting record {recordNumber} of {totalRecords}...[green]completed[/]";
            recordNumber++;
        }
        
        Results.Clear();
    }
}

public class DeleteRecordsViewModelValidator : BaseCosmosViewModelValidator<DeleteRecordsViewModel, DeleteRecordsActionTypes>
{
    public DeleteRecordsViewModelValidator()
    {
        RuleFor(x => x.Query).NotEmpty();
    }
}


public enum DeleteRecordsActionTypes
{
    Initialize,
    Validate,
    QueryRunning,
    QueryFinished,
    TableDataUpdated,
    DeleteRunning,
    DeleteFinished
}