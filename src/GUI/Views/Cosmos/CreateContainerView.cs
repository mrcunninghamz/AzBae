using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views.Cosmos;

public class CreateContainerView : BaseCosmosView<CreateContainerActionTypes>
{
    private readonly Label _accountKeylabel = new();
    private readonly TextField _accountKeyField = new();
    private Button _createButton;
    private readonly CreateContainerViewModel _viewModel;

    public CreateContainerView(CreateContainerViewModel viewModel) : base(viewModel)
    {
        _viewModel = viewModel;
    }

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        
        Title = "Create Cosmos Container";
        // Account Key
        SetLabelAndField(label: _accountKeylabel, labelText: "Account Key:", textField: _accountKeyField, topLabel: AccountEndpointLabel);
        
        // Need to move the fields down so that account key is between account endpoioint and databasename
        DatabaseNamelabel.Y = Pos.Bottom(_accountKeylabel) + 1;
        DatabaseNameField.Y = Pos.Bottom(_accountKeylabel) + 1;
        
        _accountKeyField.TextChanged += (_, __) =>
        {
            _viewModel.AccountKey = _accountKeyField.Text;
        };

        _createButton = new Button
        {
            Text = "Create Container",
            X = Pos.Right(ViewSettingsDialogButton) + 1
        };
        Add(_createButton);

        _createButton.Accepting += (_, __) =>
        {
            _viewModel.Validate();
            if (!_viewModel.CanCreateContainer)
            {
                ShowErrorDialog(_viewModel.Errors);
                return;
            }
            _viewModel.CreateContainerCommand.Execute(null);
        };

        SettingsDialog.Add(_accountKeylabel, _accountKeyField);
        
        Initialized += (_, _) =>
        {
            _viewModel.Initialize ();
        };
    }

    public override void Receive(Message<CreateContainerActionTypes> message)
    {
        switch (message.Value)
        {
            case CreateContainerActionTypes.Initialize:
                AccountEndpointField.Text = _viewModel.AccountEndpoint;
                _accountKeyField.Text = _viewModel.AccountKey;
                DatabaseNameField.Text = _viewModel.DatabaseName;
                ContainerNameField.Text = _viewModel.ContainerName;
                PartitionKeyField.Text = _viewModel.PartitionKey;
                _createButton.Enabled = _viewModel.CanCreateContainer;
                break;
            case CreateContainerActionTypes.Validate:
                // _createButton.Enabled = _viewModel.CanCreateContainer;
                break;
            case CreateContainerActionTypes.CreateProgress:
                AzureDialog.Text = "Connecting to Azure...";
                Add(AzureDialog);
                break;
            case CreateContainerActionTypes.CreateDatabaseProgress:
                AzureDialog.Text = "Creating Cosmos Database if it doesn't already exist...";
                break;
            case CreateContainerActionTypes.CreateContainerProgress:
                AzureDialog.Text = "Creating cosmos container if it doesn't already exist...";
                break;
            case CreateContainerActionTypes.CreateFinished:
                AzureDialog.Text = "Finished";
                var button = new Button
                {
                    Text = "Close"
                };
                AzureDialog.AddButton(button);
                
                button.Accepting += (_, _) => Remove(AzureDialog);
                break;
        }
    }
}