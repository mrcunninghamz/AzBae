using System.Collections.Specialized;
using GUI.Models.Cosmos;
using Newtonsoft.Json.Linq;
using Terminal.Gui;

namespace GUI.Views.Cosmos;

public class DeleteRecordsView : BaseCosmosView<DeleteRecordsActionTypes>
{
    private readonly DeleteRecordsViewModel _viewModel;
    private readonly Label _queryLabel = new();
    private readonly TextField _queryField = new();
    private Button _queryButton;
    private Button _deleteRecordsButton;
    private TableView _tableView = new() { X = 0, Y = 0, Width = Dim.Fill (), Height = Dim.Fill (1) };
    private Window _tableWindow = new() { X = 0, Y = 0, Width = Dim.Fill (), Height = Dim.Fill (1) };
    
    public DeleteRecordsView(DeleteRecordsViewModel viewModel) : base(viewModel)
    {
        Title = "Delete Cosmos Records";
        _viewModel = viewModel;
    }
    public override void InitializeComponent()
    {
        base.InitializeComponent();
        
        SetLabelAndField(label: _queryLabel, labelText: "Query:", textField: _queryField);
        _queryField.TextChanged += (_, __) => _viewModel.Query = _queryField.Text;
        _queryLabel.Y = Pos.Bottom(ViewSettingsDialogButton) + 1;
        _queryField.Y = Pos.Bottom(ViewSettingsDialogButton) + 1;
        
        _queryButton = new Button
        {
            Text = "Run Query",
            Y = Pos.Bottom(_queryLabel) + 1
        };
        _queryButton.Accepting += (_, __) =>
        {
            _viewModel.Validate();
            if (!_viewModel.CanRunQuery)
            {
                ShowErrorDialog(_viewModel.Errors);
                return;
            }
            _viewModel.RunQueryCommand.Execute(null);
        };
        
        // Add Delete Records button next to Settings button
        _deleteRecordsButton = new Button
        {
            Text = "Delete Records",
            Y = Pos.Y(_queryButton),
            X = Pos.Right(_queryButton) + 1,
            Visible = _viewModel.TableData.Any()
        };
        _deleteRecordsButton.Accepting += (_, __) =>
        {
            if (_viewModel.TableData == null || !_viewModel.TableData.Any())
            {
                MessageBox.Query("Error", "No records to delete", "OK");
                return;
            }
            
            var result = MessageBox.Query(
                "Confirm Delete", 
                $"Are you sure you want to delete {_viewModel.TableData.Count} records?", 
                "Yes", "No");
                
            if (result == 0) // Yes
            {
                _viewModel.DeleteCommand.Execute(null);
            }
        };
        
        Add(_queryLabel, _queryField, _queryButton, _deleteRecordsButton);

        InitializeTableView();
        
        Initialized += (_, _) =>
        {
            _viewModel.Initialize ();
        };
    }
    
    public override void Receive(Message<DeleteRecordsActionTypes> message)
    {
        switch (message.Value)
        {
            case DeleteRecordsActionTypes.Initialize:
                AccountEndpointField.Text = _viewModel.AccountEndpoint;
                DatabaseNameField.Text = _viewModel.DatabaseName;
                ContainerNameField.Text = _viewModel.ContainerName;
                PartitionKeyField.Text = _viewModel.PartitionKey;
                break;
            
            case DeleteRecordsActionTypes.QueryRunning:
                _viewModel.TableData.Clear();
                AzureDialog.Text = $"Running the following query... \n {_viewModel.Query}";
                Add(AzureDialog);
                break;
            
            case DeleteRecordsActionTypes.QueryFinished:
                AzureDialog.Text = $"Query finished with \n {_viewModel.Results.Count} records.";
                var button = new Button
                {
                    Text = "Close",
                };
                button.Accepting += (_, _) =>
                {
                    AzureDialog.RemoveAll();
                    Remove(AzureDialog);
                };
                AzureDialog.AddButton(button);
                break;

            case DeleteRecordsActionTypes.DeleteRunning:
                AzureDialog.Text = _viewModel.Message;
                Add(AzureDialog);
                break;

            case DeleteRecordsActionTypes.DeleteFinished:
                AzureDialog.Text = _viewModel.Message;
                var closeButton = new Button
                {
                    Text = "Close",
                };
                closeButton.Accepting += (_, _) =>
                {
                    AzureDialog.RemoveAll();
                    Remove(AzureDialog);
                    // Refresh the table data after deletion
                    _viewModel.TableData.Clear();
                    _tableView.Table = new EnumerableTableSource<JObject>(
                        _viewModel.TableData,
                        new Dictionary<string, Func<JObject, object>>
                        {
                            { "ID", j => j["id"]!.ToString()},
                            { "Raw", j => j.ToString()}
                        }
                    );
                };
                AzureDialog.AddButton(closeButton);
                break;
                
            case DeleteRecordsActionTypes.TableDataUpdated:
                _tableWindow.Visible = _viewModel.TableData.Any();
                _tableWindow.Title = _viewModel.DataTableTitle;
                _tableView.Table = new EnumerableTableSource<JObject>(
                    _viewModel.TableData,
                    new Dictionary<string, Func<JObject, object>>
                    {
                        { "ID", j => j["id"]!.ToString()},
                        { "Raw", j => j.ToString()}
                    }
                );
                _deleteRecordsButton.Visible = _viewModel.TableData.Any();
                break;
            
        }
    }

    private void InitializeTableView()
    {
        _tableWindow.Y = Pos.Bottom(_queryButton) + 1;
        _tableWindow.Visible = false;
        
        var nextButton = new Button
        {
            Text = "Next Page",
            Y = 0
        };
        nextButton.Accepting += (_, __) =>
        {
            if (_viewModel.HasMoreTableData)
            {
                _viewModel.NextPageCommand.Execute(null);
            }
            else
            {
                MessageBox.Query(title: "Error", message:"No more data available.", "OK");
            }
        };
        
        _tableView.Y = Pos.Bottom(nextButton) + 1;
        
        _tableWindow.Add(_tableView, nextButton);
        
        Add(_tableWindow);
    }
}
