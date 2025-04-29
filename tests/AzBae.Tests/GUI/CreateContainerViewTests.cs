using AzBae.Core.Configuration;
using GUI.Models.Cosmos;
using GUI.Views.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Terminal.Gui;

namespace AzBae.Tests.GUI
{
    public class CreateContainerViewTests : BaseViewTest<CreateContainerView>
    {
        private Mock<IOptions<CosmosAppSettings>> _mockOptions;
        private CreateContainerViewModel _viewModel;
        private readonly CosmosAppSettings _cosmosSettings;

        public CreateContainerViewTests()
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
            _viewModel = new CreateContainerViewModel(_mockOptions.Object);
            
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
            Assert.Equal("Create Cosmos Container", TestSubject.Title);
            
            // Check using reflection to access private fields
            var accountKeyLabelField = GetPrivateField<Label>(TestSubject, "_accountKeylabel");
            var accountKeyTextField = GetPrivateField<TextField>(TestSubject, "_accountKeyField");
            var createButtonField = GetPrivateField<Button>(TestSubject, "_createButton");
            
            Assert.NotNull(accountKeyLabelField);
            Assert.NotNull(accountKeyTextField);
            Assert.NotNull(createButtonField);
            Assert.Equal("Create Container", createButtonField.Text);
        }

        [Fact]
        public void Receive_InitializeMessage_SetsFieldValues()
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Initialize the message that would come from the ViewModel
            var message = new Message<CreateContainerActionTypes> 
            { 
                Value = CreateContainerActionTypes.Initialize 
            };

            // Set properties on the view model
            _viewModel.AccountEndpoint = "test-endpoint";
            _viewModel.AccountKey = "test-key";
            _viewModel.DatabaseName = "test-db";
            _viewModel.ContainerName = "test-container";
            _viewModel.PartitionKey = "id";

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check fields were properly updated
            var accountKeyTextField = GetPrivateField<TextField>(TestSubject, "_accountKeyField");
            var accountEndpointField = GetPrivateField<TextField>(TestSubject, "AccountEndpointField", typeof(BaseCosmosView<CreateContainerActionTypes>));
            var databaseNameField = GetPrivateField<TextField>(TestSubject, "DatabaseNameField", typeof(BaseCosmosView<CreateContainerActionTypes>));
            var containerNameField = GetPrivateField<TextField>(TestSubject, "ContainerNameField", typeof(BaseCosmosView<CreateContainerActionTypes>));
            var partitionKeyField = GetPrivateField<TextField>(TestSubject, "PartitionKeyField", typeof(BaseCosmosView<CreateContainerActionTypes>));
            
            Assert.Equal("test-endpoint", accountEndpointField.Text);
            Assert.Equal("test-key", accountKeyTextField.Text);
            Assert.Equal("test-db", databaseNameField.Text);
            Assert.Equal("test-container", containerNameField.Text);
            Assert.Equal("id", partitionKeyField.Text);
        }

        [Theory]
        [InlineData(CreateContainerActionTypes.CreateProgress, "Connecting to Azure...")]
        [InlineData(CreateContainerActionTypes.CreateDatabaseProgress, "Creating Cosmos Database if it doesn't already exist...")]
        [InlineData(CreateContainerActionTypes.CreateContainerProgress, "Creating cosmos container if it doesn't already exist...")]
        [InlineData(CreateContainerActionTypes.CreateFinished, "Finished")]
        public void Receive_ProgressMessages_UpdatesAzureDialogText(CreateContainerActionTypes actionType, string expectedText)
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Initialize the message that would come from the ViewModel
            var message = new Message<CreateContainerActionTypes> 
            { 
                Value = actionType 
            };

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check Azure dialog has correct text
            var azureDialog = GetPrivateField<Dialog>(TestSubject, "AzureDialog", typeof(BaseCosmosView<CreateContainerActionTypes>));
            Assert.Equal(expectedText, azureDialog.Text);
        }

        [Fact]
        public void Receive_CreateFinished_AddsCloseButton()
        {
            // Arrange
            TestSubject.InitializeComponent();
            
            // Initialize the message that would come from the ViewModel
            var message = new Message<CreateContainerActionTypes> 
            { 
                Value = CreateContainerActionTypes.CreateFinished
            };

            // Act - Send message to view
            TestSubject.Receive(message);

            // Assert - Check Azure dialog has a button added
            var azureDialog = GetPrivateField<Dialog>(TestSubject, "AzureDialog", typeof(BaseCosmosView<CreateContainerActionTypes>));
            
            // Check that a button was added to the dialog
            Assert.Contains("Finished", azureDialog.Text);
            
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
    }
}