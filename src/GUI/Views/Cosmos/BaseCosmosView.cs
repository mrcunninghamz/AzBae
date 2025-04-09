using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views.Cosmos;

public abstract class BaseCosmosView<T> : BaseView<T> where T : Enum
{
    private readonly BaseCosmosViewModel _viewModel;
    protected readonly TextField AccountEndpointField = new();
    protected readonly Label AccountEndpointLabel = new();
    protected readonly Label DatabaseNamelabel = new();
    protected readonly TextField DatabaseNameField = new();
    protected readonly Label ContainerNamelabel = new();
    protected readonly TextField ContainerNameField = new();
    protected readonly Label PartitionKeylabel = new();
    protected readonly TextField PartitionKeyField = new();
    protected BaseCosmosView(BaseCosmosViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public override void InitializeComponent()
    {
        // Account Endpoint
        SetLabelAndField(label: AccountEndpointLabel, labelText: "Account Endpoint:", textField: AccountEndpointField);
        
        // Database name
        SetLabelAndField(label: DatabaseNamelabel, labelText: "Database Name:", textField: DatabaseNameField, topLabel: AccountEndpointLabel);
        
        // Container Name
        SetLabelAndField(label: ContainerNamelabel, labelText: "Container Name:", textField: ContainerNameField, topLabel: DatabaseNamelabel);
       
        // PartitionKey
        SetLabelAndField(label: PartitionKeylabel, labelText: "Partition Key:", textField: PartitionKeyField, topLabel: ContainerNamelabel);
        

        
        AccountEndpointField.TextChanged += (_, __) =>
        {
            _viewModel.AccountEndpoint = AccountEndpointField.Text;
        };
        
        DatabaseNameField.TextChanged += (_, __) =>
        {
            _viewModel.DatabaseName = DatabaseNameField.Text;
        };
        ContainerNameField.TextChanged += (_, __) =>
        {
            _viewModel.ContainerName = ContainerNameField.Text;
        };
        PartitionKeyField.TextChanged += (_, __) =>
        {
            _viewModel.PartitionKey = PartitionKeyField.Text;
        };
        
        base.InitializeComponent();
    }
}
