using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views.Cosmos;

public class CreateContainerView : BaseCosmosView<CreateContainerActionType>
{
    private readonly Label _accountKeylabel = new();
    private readonly TextField _accountKeyField = new();
    private Button _createButton;
    private Dialog _azureDialog;
    private readonly CreateContainerViewModel _viewModel;

    public CreateContainerView(CreateContainerViewModel viewModel) : base(viewModel)
    {
        WeakReferenceMessenger.Default.Register (this);
        Title = "Create Cosmos Container";
        _viewModel = viewModel;
    }

    public override void Receive(Message<CreateContainerActionType> message)
    {
        switch (message.Value)
        {
            case CreateContainerActionType.Initialize:
                AccountEndpointField.Text = _viewModel.AccountEndpoint;
                _accountKeyField.Text = _viewModel.AccountKey;
                DatabaseNameField.Text = _viewModel.DatabaseName;
                ContainerNameField.Text = _viewModel.ContainerName;
                PartitionKeyField.Text = _viewModel.PartitionKey;
                _createButton.Enabled = _viewModel.CanCreateContainer;
                break;
            case CreateContainerActionType.Validate:
                // _createButton.Enabled = _viewModel.CanCreateContainer;
                break;
            case CreateContainerActionType.CreateProgress:
                _azureDialog = new Dialog
                {
                    Title = "Working with Azure...",
                    ButtonAlignment = Alignment.Center
                };
                _azureDialog.Add(new Label
                {
                    Text = "Connecting to Azure...",
                });
                Add(_azureDialog);
                break;
            case CreateContainerActionType.CreateDatabaseProgress:
                _azureDialog.RemoveAll();
                _azureDialog.Add(new Label
                {
                    Text = "Creating Cosmos Database if it doesn't already exist...",
                });
                break;
            case CreateContainerActionType.CreateContainerProgress:
                _azureDialog.RemoveAll();
                _azureDialog.Add(new Label
                {
                    Text = "Creating cosmos container if it doesn't already exist...",
                });
                break;
            case CreateContainerActionType.CreateFinished:
                _azureDialog.RemoveAll();
                var label = new Label
                {
                    Text = "Finished!",
                };
                _azureDialog.Add(label);
                var button = new Button
                {
                    Text = "Close",
                    Y = Pos.Bottom(label) + 1
                };
                _azureDialog.AddButton(button);
                
                button.Accepting += (_, _) => Remove(_azureDialog);
                break;
        }
    }

    public override void InitializeComponent()
    {
        if (IsInitialized)
        {
            return;
        }
        
        base.InitializeComponent();
        
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
            Y = Pos.Bottom(PartitionKeylabel) + 1
        };
        Add(_createButton);

        _createButton.Accepting += (_, __) =>
        {
            _viewModel.Validate();
            if (!_viewModel.CanCreateContainer)
            {
                ErrorDialogLabel.Text = _viewModel.Errors;
                Add(ErrorDialog);
                return;
            }
            _viewModel.CreateContainerCommand.Execute(null);
        };
        
        Initialized += (_, _) =>
        {
            _viewModel.Initialized ();
        };
    }
}