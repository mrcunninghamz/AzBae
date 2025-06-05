# AzBae GUI

A graphical user interface for managing and interacting with Azure resources, built with .NET C# using [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui). AzBae GUI provides a user-friendly interface for common Azure management tasks with a primary focus on CosmosDB and Function Apps management.

## Overview

AzBae GUI offers a terminal-based user interface for interacting with Azure resources without leaving your command line environment. The application simplifies common Azure management tasks, providing focused views and streamlined workflows for managing your Azure resources.

## Features

- Integration with Azure Cosmos DB management APIs
- Management of Azure Function Apps
- User-friendly terminal interface
- Configuration management through settings
- Structured data handling for Azure resources

## Views

The AzBae GUI application includes the following views:

### Cosmos DB Views

#### Create Container View
- Creates new Cosmos DB containers with specified configuration
- Automatically creates the database if it doesn't exist
- Configurable partition key for optimal data distribution
- Real-time status updates during the creation process

#### Delete Records View
- Safely removes records from Cosmos DB containers
- Supports filtering to target specific records
- Confirmation prompts to prevent accidental deletions

### Function Apps Views

#### List My Function Apps View
- Displays all function apps that match configured filter patterns
- Shows key details like status, runtime, and region
- Allows for quick access to function app properties

## Configuration

AzBae GUI uses an `appsettings.local.json` file for configuration. You can create this file by copying the template file (`appsettings.local.template.json`) and filling in your specific values.

### Setting up the Configuration File

1. Copy the template to create your local settings file:
   ```
   cp appsettings.local.template.json appsettings.local.json
   ```
2. Edit the `appsettings.local.json` file with your specific Azure configuration values

### Configuration Options

The `appsettings.local.json` file includes the following configuration sections:

#### CosmosDb Section

| Setting | Description | Impact |
|---------|-------------|--------|
| accountEndpoint | Your Azure Cosmos DB account endpoint URL | Required for all Cosmos DB operations; defines which account to connect to |
| accountKey | Your Azure Cosmos DB account access key | Required for authentication with your Cosmos DB account |
| databaseName | Default database name to use | Pre-populates the database name in Cosmos DB views |
| containerName | Default container name to use | Pre-populates the container name in Cosmos DB views |
| partitionKey | Default partition key path (e.g., "/id") | Pre-populates the partition key in Create Container view |

#### ResourceFilters Section

| Setting | Description | Impact |
|---------|-------------|--------|
| FunctionAppFilterPattern | Regular expression pattern to filter function apps by name | Controls which function apps are displayed in the List Function Apps view; use `^team1-.*` to show only function apps with names starting with "team1-" |
| FunctionAppWhere | Additional filter criteria for Function Apps | Enables more advanced filtering of function apps beyond name patterns |

### Example Configuration

Here's an example of a completed configuration file:

```json
{
  "CosmosDb": {
    "accountEndpoint": "https://mycosmosaccount.documents.azure.com:443/",
    "accountKey": "my-cosmos-account-key==",
    "databaseName": "MyDatabase",
    "containerName": "MyContainer",
    "partitionKey": "/id"
  },
  "ResourceFilters": {
    "FunctionAppFilterPattern": "^project-.*",
    "FunctionAppWhere": ""
  }
}
```

## Getting Started

### Prerequisites
- .NET 9.0
- An Azure subscription with appropriate access permissions
- Cosmos DB account and/or Function Apps (depending on which features you plan to use)

### Installation

1. Ensure you have the .NET 9.0 SDK installed
2. Clone the repository
3. Navigate to the GUI project directory:
   ```
   cd /path/to/AzBae/src/GUI
   ```
4. Set up your configuration as described above
5. Build and run the application:
   ```
   dotnet build
   dotnet run
   ```

## Tips for Using AzBae GUI

- When creating a Cosmos container, ensure your partition key choice aligns with your query patterns
- Use the settings dialog to update your configuration without editing the JSON file directly
- For Function Apps, use specific naming patterns to make filtering more effective

## Related Projects

This project is part of the "Bae" collection of tools:

- [AzBae.MCP](../MCP/README.md) - A Model Context Protocol server for Azure resources integration, enabling AI assistants to interact with Azure resources through natural language.
- [TestBae](https://github.com/mrcunninghamz/TestBae) - A .NET testing utility library that simplifies test setup with fixtures, mocks, and common testing patterns.
- [NotionBae](https://github.com/mrcunninghamz/NotionBae) - A Model Context Protocol server for Notion.com integration.

## License

MIT License