using AzBae.Core.Configuration;
using GUI.Models.Cosmos;
using GUI.Views.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Terminal.Gui;

namespace AzBae.Tests.GUI
{
    public class DeleteRecordsViewTests : BaseViewTest<DeleteRecordsView>
    {
        private Mock<IOptions<CosmosAppSettings>> _mockOptions;
        private DeleteRecordsViewModel _viewModel;
        private readonly CosmosAppSettings _cosmosSettings;

        public DeleteRecordsViewTests()
        {
            // Setup test cosmosAppSettings
            _cosmosSettings = new CosmosAppSettings
            {
                AccountEndpoint = "https://test-endpoint.com",
                AccountKey = "test-key",
                ContainerName = "test-container",
                DatabaseName = "test-db",
                PartitionKey = "id"
            };
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            // Call the base implementation first to set up AutoMapper and the subject
            base.ConfigureServices(services);
            
            // Create and configure mocks
            _mockOptions = new Mock<IOptions<CosmosAppSettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_cosmosSettings);
            
            // Create view model
            _viewModel = new DeleteRecordsViewModel(_mockOptions.Object);
            
            // Register mocked dependencies
            services.AddSingleton(_mockOptions.Object);
            services.AddSingleton(_viewModel);
        }

        [Fact]
        public void InitializeComponent_SetsUpView_WithCorrectComponents()
        {
            // Act - Initialize component
            TestSubject.InitializeComponent();

            // Assert - Verify components exist on the view
            Assert.Equal("Delete Cosmos Records", TestSubject.Title);
            
            // Check using reflection to access private fields
            var queryLabel = GetPrivateField<Label>(TestSubject, "_queryLabel");
            var queryField = GetPrivateField<TextField>(TestSubject, "_queryField");
            var queryButton = GetPrivateField<Button>(TestSubject, "_queryButton");
            var deleteRecordsButton = GetPrivateField<Button>(TestSubject, "_deleteRecordsButton");
            var tableView = GetPrivateField<TableView>(TestSubject, "_tableView");
            var tableWindow = GetPrivateField<Window>(TestSubject, "_tableWindow");
            
            Assert.NotNull(queryLabel);
            Assert.NotNull(queryField);
            Assert.NotNull(queryButton);
            Assert.NotNull(deleteRecordsButton);
            Assert.NotNull(tableView);
            Assert.NotNull(tableWindow);
            Assert.Equal("Run Query", queryButton.Text);
            Assert.Equal("Delete Records", deleteRecordsButton.Text);
        }

        [Fact]
        public void Receive_InitializeMessage_SetsFieldValues()
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Initialize the message that would come from the ViewModel
            var message = new Message<DeleteRecordsActionTypes> 
            { 
                Value = DeleteRecordsActionTypes.Initialize 
            };

            // Set properties on the view model
            _viewModel.AccountEndpoint = "test-endpoint";
            _viewModel.DatabaseName = "test-db";
            _viewModel.ContainerName = "test-container";
            _viewModel.PartitionKey = "id";

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check fields were properly updated
            var accountEndpointField = GetPrivateField<TextField>(TestSubject, "AccountEndpointField", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            var databaseNameField = GetPrivateField<TextField>(TestSubject, "DatabaseNameField", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            var containerNameField = GetPrivateField<TextField>(TestSubject, "ContainerNameField", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            var partitionKeyField = GetPrivateField<TextField>(TestSubject, "PartitionKeyField", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            
            Assert.Equal("test-endpoint", accountEndpointField.Text);
            Assert.Equal("test-db", databaseNameField.Text);
            Assert.Equal("test-container", containerNameField.Text);
            Assert.Equal("id", partitionKeyField.Text);
        }

        [Theory]
        [InlineData(DeleteRecordsActionTypes.QueryRunning)]
        [InlineData(DeleteRecordsActionTypes.DeleteRunning)]
        public void Receive_RunningMessages_ShowsAzureDialog(DeleteRecordsActionTypes actionType)
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Set up viewModel properties
            _viewModel.Query = "SELECT * FROM c";
            if (actionType == DeleteRecordsActionTypes.DeleteRunning)
            {
                _viewModel.Message = "Deleting records...";
            }
            
            // Initialize the message
            var message = new Message<DeleteRecordsActionTypes> 
            { 
                Value = actionType 
            };

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check Azure dialog is visible
            var azureDialog = GetPrivateField<Dialog>(TestSubject, "AzureDialog", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            
            // Check dialog has content
            Assert.NotEmpty(azureDialog.Text);
            
            // For QueryRunning, check that it contains the query
            if (actionType == DeleteRecordsActionTypes.QueryRunning)
            {
                Assert.Contains(_viewModel.Query, azureDialog.Text);
            }
            // For DeleteRunning, check that it contains the message
            else if (actionType == DeleteRecordsActionTypes.DeleteRunning)
            {
                Assert.Equal(_viewModel.Message, azureDialog.Text);
            }
        }

        [Fact]
        public void Receive_QueryFinished_AddsCloseButton()
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Set up results in viewModel
            _viewModel.Results = new List<JObject> { new JObject() };
            
            // Initialize the message
            var message = new Message<DeleteRecordsActionTypes> 
            { 
                Value = DeleteRecordsActionTypes.QueryFinished
            };

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check Azure dialog has a button added
            var azureDialog = GetPrivateField<Dialog>(TestSubject, "AzureDialog", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            
            // Check that results count is in the text
            Assert.Contains(_viewModel.Results.Count.ToString(), azureDialog.Text);
            
            // Check for button in dialog's descendants
            bool buttonFound = false;
            foreach (var child in azureDialog.SubViews)
            {
                if (child is Button button && button.Text == "Close")
                {
                    buttonFound = true;
                    break;
                }
            }
            
            Assert.True(buttonFound, "Close button should be added to the dialog");
        }

        [Fact]
        public void Receive_DeleteFinished_AddsCloseButtonAndClearsTable()
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Set up message in viewModel
            _viewModel.Message = "Delete operation completed";
            
            // Initialize the message
            var message = new Message<DeleteRecordsActionTypes> 
            { 
                Value = DeleteRecordsActionTypes.DeleteFinished
            };

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check Azure dialog has a button added
            var azureDialog = GetPrivateField<Dialog>(TestSubject, "AzureDialog", typeof(BaseCosmosView<DeleteRecordsActionTypes>));
            
            // Check that the message is set
            Assert.Equal(_viewModel.Message, azureDialog.Text);
            
            // Check for button in dialog's descendants
            bool buttonFound = false;
            foreach (var child in azureDialog.SubViews)
            {
                if (child is Button button && button.Text == "Close")
                {
                    buttonFound = true;
                    break;
                }
            }
            
            Assert.True(buttonFound, "Close button should be added to the dialog");
        }

        [Fact]
        public void Receive_TableDataUpdated_UpdatesTableViewAndVisibility()
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Create sample table data with JObjects
            var sampleData = new List<JObject>
            {
                new JObject { ["id"] = "1", ["name"] = "Test Item 1" },
                new JObject { ["id"] = "2", ["name"] = "Test Item 2" }
            };
            
            // Update view model properties
            _viewModel.TableData = sampleData;
            _viewModel.DataTableTitle = "Test Results";
            
            // Initialize the message
            var message = new Message<DeleteRecordsActionTypes> 
            { 
                Value = DeleteRecordsActionTypes.TableDataUpdated
            };

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check table window and delete button visibility
            var tableWindow = GetPrivateField<Window>(TestSubject, "_tableWindow");
            var deleteRecordsButton = GetPrivateField<Button>(TestSubject, "_deleteRecordsButton");
            
            Assert.True(tableWindow.Visible);
            Assert.Equal("Test Results", tableWindow.Title);
            Assert.True(deleteRecordsButton.Visible);
        }
    }
}