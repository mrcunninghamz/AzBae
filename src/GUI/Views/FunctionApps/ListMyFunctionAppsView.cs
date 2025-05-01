using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GUI.Models.FunctionApps;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views.FunctionApps;

public class FunctionAppView : BaseView<ListMyFunctionAppsActionTypes>
{
    private readonly ListMyFunctionAppsViewModel _viewModel;
    private TableView _functionAppsTableView = new();
    private Label _filterStatusLabel;
    private TextField _filterPatternField;
    private Label _filterPatternLabel;
    private Button _applyFilterButton;
    private Button _clearFilterButton;
    private Button _refreshButton;
    private Label _filterIcon;
    private bool _isInitialized;

    public FunctionAppView(ListMyFunctionAppsViewModel viewModel)
    {
        Title = "Azure Function Apps";
        _viewModel = viewModel;
    }

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        if (_isInitialized) return;
        
        // Filter pattern controls - Row 1
        _filterPatternLabel = new Label
        {
            Text = "Filter Pattern:",
            X = 1,
            Y = 0
        };
        
        _filterPatternField = new TextField
        {
            Text = _viewModel.FilterPattern,
            X = Pos.Right(_filterPatternLabel) + 1,
            Y = 0,
            Width = 30
        };
        
        _filterPatternField.TextChanged += (_, _) =>
        {
            _viewModel.FilterPattern = _filterPatternField.Text?.ToString() ?? string.Empty;
        };
        
        // Buttons - Row 2
        _applyFilterButton = new Button
        {
            Text = "_Apply Filter",
            X = 1,
            Y = 1,
        };
        
        _clearFilterButton = new Button
        {
            Text = "_Clear Filter",
            X = Pos.Right(_applyFilterButton) + 1,
            Y = 1,
        };
        
        _refreshButton = new Button
        {
            Text = "_Refresh",
            X = Pos.Right(_clearFilterButton) + 1,
            Y = 1,
        };
        
        // Filter status - Row 3
        _filterIcon = new Label
        {
            Text = "🔍",
            X = 1,
            Y = 2
        };
        
        _filterStatusLabel = new Label
        {
            Text = "",
            X = Pos.Right(_filterIcon) + 1,
            Y = 2,
            Width = Dim.Fill(),
            ColorScheme = Colors.ColorSchemes["Dialog"],
        };
        
        // Table view - Starting at Row 4, filling the rest of the space
        _functionAppsTableView.X = 0;
        _functionAppsTableView.Y = 3;
        _functionAppsTableView.Width = Dim.Fill();
        _functionAppsTableView.Height = Dim.Fill();
        
        // Adding action handlers
        _applyFilterButton.Accepting += (_,_) => _viewModel.ApplyFilter();
        _clearFilterButton.Accepting += (_,_)  => _viewModel.ClearFilter();
        _refreshButton.Accepting += async (_,_)  => await _viewModel.LoadFunctionApps();
        
        _functionAppsTableView.CellActivated += (_, args) =>
        {
            var table = args.Table as EnumerableTableSource<FunctionApp>;
            var functionApp = table!.GetObjectOnRow(args.Row);
            // Open the function app in the browser when clicked
            OpenBrowser(functionApp.PortalUri);
        };
        
        // Add elements to the view
        Add(_filterPatternLabel);
        Add(_filterPatternField);
        Add(_applyFilterButton);
        Add(_clearFilterButton);
        Add(_refreshButton);
        Add(_filterIcon);
        Add(_filterStatusLabel);
        Add(_functionAppsTableView);
        
        // Set initial visibility of filter icon
        _filterIcon.Visible = _viewModel.IsFilterActive;
        
        
        _isInitialized = true;
    }

    public void LoadData()
    {
        // Initial data load
        Task.Run(async () => await _viewModel.LoadFunctionApps());
        UpdateTable();
    }

   private void UpdateTable()
    {
        // Update the table view with current data
        var apps = _viewModel.FunctionApps.ToList();
        
        // Set up the table source with columns for function app properties
        _functionAppsTableView.Table = new EnumerableTableSource<FunctionApp>(
            apps,
            new Dictionary<string, Func<FunctionApp, object>>
            {
                { "Name", app => app.Name ?? string.Empty },
                { "ResourceGroup", app => app.ResourceGroup ?? string.Empty },
                { "Location", app => app.Location ?? string.Empty },
                { "Status", app => app.Status ?? string.Empty },
            }
        );
    }
    
    public override void Receive(Message<ListMyFunctionAppsActionTypes> message)
    {
        switch (message.Value)
        {
            case ListMyFunctionAppsActionTypes.Initialize:
                _filterPatternField.Text = _viewModel.FilterPattern;
                break;
                
            case ListMyFunctionAppsActionTypes.ListFunctionAppsProgress:
                Add(AzureDialog);
                AzureDialog.Text = "Loading function apps...";
                break;
                
            case ListMyFunctionAppsActionTypes.ListFunctionAppsFinished:
                Remove(AzureDialog);
                // Update the status label based on filter status
                UpdateFilterStatus();
                break;
                
            case ListMyFunctionAppsActionTypes.FilterApplied:
                UpdateFilterStatus();
                _filterIcon.Visible = true;
                UpdateTable();
                break;
                
            case ListMyFunctionAppsActionTypes.FilterCleared:
                _filterStatusLabel.Text = string.Empty;
                _filterIcon.Visible = false;
                _filterPatternField.Text = string.Empty;
                UpdateTable();
                break;
                
            case ListMyFunctionAppsActionTypes.Error:
                ShowErrorDialog(_viewModel.Errors);
                break;
        }
    }
    
    private void UpdateFilterStatus()
    {
        _filterStatusLabel.Text = _viewModel.FilterStatusMessage;
        _filterStatusLabel.Visible = !string.IsNullOrEmpty(_viewModel.FilterStatusMessage);
        _filterIcon.Visible = _viewModel.IsFilterActive;
    }
}
