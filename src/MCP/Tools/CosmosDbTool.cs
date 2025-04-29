using System.ComponentModel;
using AzBae.MCP.Models;
using AzBae.Core.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace AzBae.MCP.Tools
{
    
    [McpServerToolType]
    public class CosmosDbTool
    {
        private readonly ILogger<CosmosDbTool> _logger;
        private readonly CosmosAppSettings _cosmosAppSettings;

        public CosmosDbTool(ILogger<CosmosDbTool> logger, IOptions<CosmosAppSettings> cosmosAppSettings)
        {
            _logger = logger;
            _cosmosAppSettings = cosmosAppSettings.Value;
        }

        [McpServerTool(Name = "azb_create_cosmos_container"), Description("Creates a new CosmosDB container with specified configuration")]
        public async Task<CreateCosmosContainerResponse> CreateCosmosContainerAsync(
            [Description("Database to create or use (optional if configured in settings)")] string databaseName,
            [Description("Name of the container to create (optional if configured in settings)")] string containerName,
            [Description("Partition key path for the container (optional if configured in settings)")] string partitionKey,
            CancellationToken cancellationToken = default)
        {
            // Try to use provided values first, fall back to configuration if not provided
            var effectiveAccountEndpoint = _cosmosAppSettings.AccountEndpoint;
            var effectiveAccountKey = _cosmosAppSettings.AccountKey;
            var effectiveDatabaseName = string.IsNullOrWhiteSpace(databaseName) ? _cosmosAppSettings.DatabaseName : databaseName;
            var effectiveContainerName = string.IsNullOrWhiteSpace(containerName) ? _cosmosAppSettings.ContainerName : containerName;
            var effectivePartitionKey = string.IsNullOrWhiteSpace(partitionKey) ? _cosmosAppSettings.PartitionKey : partitionKey;
            
            _logger.LogInformation("Creating CosmosDB container {ContainerName} in database {DatabaseName}", effectiveContainerName, effectiveDatabaseName);
            
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(effectiveAccountEndpoint))
                    throw new ArgumentException("Account endpoint is required", nameof(_cosmosAppSettings.AccountEndpoint));
                
                if (string.IsNullOrWhiteSpace(effectiveAccountKey))
                    throw new ArgumentException("Account key is required", nameof(_cosmosAppSettings.AccountKey));
                
                if (string.IsNullOrWhiteSpace(effectiveDatabaseName))
                    throw new ArgumentException("Database name is required", nameof(databaseName));
                
                if (string.IsNullOrWhiteSpace(effectiveContainerName))
                    throw new ArgumentException("Container name is required", nameof(containerName));
                
                if (string.IsNullOrWhiteSpace(effectivePartitionKey))
                    throw new ArgumentException("Partition key is required", nameof(partitionKey));
                
                // Ensure partition key is properly formatted
                if (!effectivePartitionKey.StartsWith("/"))
                {
                    effectivePartitionKey = $"/{effectivePartitionKey}";
                }

                // Create a CosmosClient instance
                using CosmosClient cosmosClient = new(effectiveAccountEndpoint, effectiveAccountKey);
                
                _logger.LogInformation("Creating database if it doesn't exist: {DatabaseName}", effectiveDatabaseName);
                
                // Create database if it doesn't exist
                Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                    id: effectiveDatabaseName,
                    cancellationToken: cancellationToken);
                
                _logger.LogInformation("Creating container if it doesn't exist: {ContainerName}", effectiveContainerName);
                
                // Create container if it doesn't exist
                ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(
                    id: effectiveContainerName,
                    partitionKeyPath: effectivePartitionKey,
                    cancellationToken: cancellationToken);
                
                bool containerCreated = containerResponse.StatusCode == System.Net.HttpStatusCode.Created;
                
                _logger.LogInformation(
                    containerCreated 
                        ? "Container {ContainerName} created successfully in database {DatabaseName}" 
                        : "Container {ContainerName} already exists in database {DatabaseName}", 
                    effectiveContainerName, 
                    effectiveDatabaseName);

                // Return success response
                return new CreateCosmosContainerResponse
                {
                    Success = true,
                    Message = containerCreated 
                        ? $"Container '{effectiveContainerName}' created successfully in database '{effectiveDatabaseName}'" 
                        : $"Container '{effectiveContainerName}' already exists in database '{effectiveDatabaseName}'",
                    DatabaseName = effectiveDatabaseName,
                    ContainerName = effectiveContainerName
                };
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "CosmosDB error occurred: {Message}", ex.Message);
                return new CreateCosmosContainerResponse
                {
                    Success = false,
                    Message = $"CosmosDB error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CosmosDB container: {Message}", ex.Message);
                return new CreateCosmosContainerResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}