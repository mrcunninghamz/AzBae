using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views.Cosmos;

public class CreateContainerView : Window, IRecipient<RequestMessage<CreateContainerActions>>
{
    public  MenuBar MenuBar { get; set; }
    private Label _titleLabel;
    private TextField _accountEndpointField;
    private Label _accountEndpointLabel;

    public CreateContainerView(CreateContainerViewModel viewModel)
    {
        WeakReferenceMessenger.Default.Register (this);
        Title = "Create Cosmos Container";
        ViewModel = viewModel;
    }
    public CreateContainerViewModel ViewModel { get; set; }

    public void Receive(RequestMessage<CreateContainerActions> message)
    {
        throw new NotImplementedException();
    }

    public void InitializeComponent()
    {
        Add(MenuBar);
        _accountEndpointLabel = new Label
        {
            Text = "Accoutn Endpoint:",
            
            Y = Pos.Bottom (MenuBar) + 1,
        };
        Add(_accountEndpointLabel);
        _accountEndpointField = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right (_accountEndpointLabel) + 1,
            Y = Pos.Bottom (MenuBar) + 1,

            // Fill remaining horizontal space
            Width = Dim.Fill ()
        };
        _accountEndpointField.Width = 40;
        Add(_accountEndpointField);
    }
}



public enum CreateContainerActions
{
    CreateProgress,
    ClearForm
}