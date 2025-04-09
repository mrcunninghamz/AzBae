using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views.Cosmos;

public class CreateContainerView : Window, IRecipient<Message<CreateContainerActionType>>
{
    private readonly TextField _accountEndpointField = new();
    private readonly Label _accountEndpointLabel = new();
    private readonly Label _accountKeylabel = new();
    private readonly TextField _accountKeyField = new();
    private readonly Label _databaseNamelabel = new();
    private readonly TextField _databaseNameField = new();
    private readonly Label _containerNamelabel = new();
    private readonly TextField _containerNameField = new();
    private readonly Label _partitionKeylabel = new();
    private readonly TextField _partitionKeyField = new();
    private Button _createButton;

    public CreateContainerView(CreateContainerViewModel viewModel)
    {
        WeakReferenceMessenger.Default.Register (this);
        Title = "Create Cosmos Container";
        ViewModel = viewModel;
    }
    public CreateContainerViewModel ViewModel { get; set; }

    public void Receive(Message<CreateContainerActionType> message)
    {
        switch (message.Value)
        {
            case CreateContainerActionType.Initialize:
                _accountEndpointField.Text = ViewModel.AccountEndpoint;
                _accountKeyField.Text = ViewModel.AccountKey;
                _databaseNameField.Text = ViewModel.DatabaseName;
                _containerNameField.Text = ViewModel.ContainerName;
                _partitionKeyField.Text = ViewModel.PartitionKey;
                _createButton.Enabled = ViewModel.CanCreateContainer;
                break;
            case CreateContainerActionType.Validate:
                _createButton.Enabled = ViewModel.CanCreateContainer;
                break;
        }
    }

    public void InitializeComponent()
    {
        if (IsInitialized)
        {
            return;
        }
        
        // Account Endpoint
        SetLabelAndField(label: _accountEndpointLabel, labelText: "Account Endpoint:", textField: _accountEndpointField);
        
        // Account Key
        SetLabelAndField(label: _accountKeylabel, labelText: "Account Key:", textField: _accountKeyField, topLabel: _accountEndpointLabel);
        
        // Database name
        SetLabelAndField(label: _databaseNamelabel, labelText: "Database Name:", textField: _databaseNameField, topLabel: _accountKeylabel);
        
        // Container Name
        SetLabelAndField(label: _containerNamelabel, labelText: "Container Name:", textField: _containerNameField, topLabel: _databaseNamelabel);
       
        // PartitionKey
        SetLabelAndField(label: _partitionKeylabel, labelText: "Partition Key:", textField: _partitionKeyField, topLabel: _containerNamelabel);
        

        
        _accountEndpointField.TextChanged += (_, __) =>
        {
            ViewModel.AccountEndpoint = _accountEndpointField.Text;
            ViewModel.Validate();
        };
        _accountKeyField.TextChanged += (_, __) =>
        {
            ViewModel.AccountKey = _accountKeyField.Text;
            ViewModel.Validate();
        };
        _databaseNameField.TextChanged += (_, __) =>
        {
            ViewModel.DatabaseName = _databaseNameField.Text;
            ViewModel.Validate();
        };
        _containerNameField.TextChanged += (_, __) =>
        {
            ViewModel.ContainerName = _containerNameField.Text;
            ViewModel.Validate();
        };
        _partitionKeyField.TextChanged += (_, __) =>
        {
            ViewModel.PartitionKey = _partitionKeyField.Text;
            ViewModel.Validate();
        };

        _createButton = new Button
        {
            Text = "Create Container",
            Y = Pos.Bottom(_partitionKeylabel) + 1
        };
        Add(_createButton);

        _createButton.Accepting += (_, __) =>
        {
            if (!ViewModel.CanCreateContainer) { return; }
            ViewModel.CreateContainerCommand.Execute(null);
        };
            
        
        
        Initialized += (_, _) =>
        {
            ViewModel.Initialized ();
        };
    }

    private void SetLabelAndField(Label label, string labelText, TextField textField, Label? topLabel = null)
    {
        label.Text = labelText;
        
        // Position text field adjacent to the label
        textField.X = Pos.Right(label) + 1;
        
        // Fill remaining horizontal space
        textField.Width = Dim.Fill();
        textField.Width = 100;
        
        if (topLabel != null)
        {
            label.Y = Pos.Bottom(topLabel) + 1;
            textField.Y = Pos.Bottom(topLabel) + 1;
        }

        Add(label, textField);
    }
}