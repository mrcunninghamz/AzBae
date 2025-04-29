# AzBae.MCP

A Model Context Protocol (MCP) Server for Azure resources integration, built with .NET C# using [ModelContextProtocol](https://github.com/modelcontextprotocol). AzBae.MCP leverages the [Azure REST APIs](https://learn.microsoft.com/en-us/rest/api/azure/) to provide seamless interaction between AI assistants and Azure resources, with a primary focus on CosmosDB management.

## Overview

AzBae.MCP provides a bridge between Large Language Models and Azure resources through the Model Context Protocol framework. This allows AI assistants to interact with Azure resources, configuration, and services in a seamless and structured way - providing the same functionality as the AzBae GUI application but through a conversational interface.

## Features

- Integration with Azure Cosmos DB management APIs
- Management of CosmosDB resources:
  - Creating containers and databases
  - Querying and deleting records
  - Executing custom queries against containers
- MCP-compliant server implementation
- Structured data handling for Azure resources
- Authentication and secure access to Azure resources

## Available Tools

The following tools are implemented in the AzBae.MCP server:

| Tool Name | Description | Parameters | Status |
|-----------|-------------|------------|--------|
| azb_create_cosmos_container | Creates a new CosmosDB container with specified configuration | `accountEndpoint`: String - The Cosmos DB account endpoint URL<br>`accountKey`: String - The Cosmos DB account key<br>`databaseName`: String - Database to create or use<br>`containerName`: String - Name of the container to create<br>`partitionKey`: String - Partition key path for the container | ✅ Available |
| azb_query_cosmos_container | Executes a query against a CosmosDB container and returns the results | `accountEndpoint`: String - The Cosmos DB account endpoint URL<br>`accountKey`: String - The Cosmos DB account key<br>`databaseName`: String - Database name<br>`containerName`: String - Container name<br>`query`: String - SQL query to execute<br>`pageSize`: Integer (optional) - Number of records per page | ❌ Planned |
| azb_delete_cosmos_records | Deletes records from a CosmosDB container based on a query | `accountEndpoint`: String - The Cosmos DB account endpoint URL<br>`accountKey`: String - The Cosmos DB account key<br>`databaseName`: String - Database name<br>`containerName`: String - Container name<br>`query`: String - SQL query identifying records to delete | ❌ Planned |

Additional tools for managing other Azure resources will be added in future releases.

## Getting Started

### 1. Build the project

```bash
dotnet build
```

### 2. MCP Server Configuration

To use AzBae.MCP as an MCP server in VS Code, add the following configuration to your VS Code `settings.json` file:

```json
"mcp": {
  "inputs": [
    {
      "type": "promptString",
      "id": "azbae_cosmosdb_key",
      "description": "AzBae CosmosDB Key",
      "password": true
    }
  ],
  "servers": {
    "AzBae": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "PATH_TO_AZBAE_PROJECT/src/MCP/AzBae.MCP.csproj"
      ],
      "env": {
        "CosmosDb:AccountEndpoint": "YOUR_COSMOS_ENDPOINT",
        "CosmosDb:AccountKey": "${input:azbae_cosmosdb_key}"
      }
    }
  }
}
```

Replace `PATH_TO_AZBAE_PROJECT` with the path to your AzBae project folder (the folder containing the AzBae.sln file).

For example, if your AzBae project is located at `C:\Projects\AzBae\`, the configuration would be:

```json
"args": [
  "run",
  "--project",
  "C:\\Projects\\AzBae\\src\\MCP\\AzBae.MCP.csproj"
]
```

### Environment Variables

The AzBae.MCP server can be configured through the following environment variables:

- `AZBAE_COSMOSDB_ENDPOINT`: Your Azure CosmosDB account endpoint
- `AZBAE_COSMOSDB_KEY`: Your Azure CosmosDB account key

### Running the Server

There are several ways to start the AzBae.MCP server:

1. **Using VS Code Commands**:
   - Open the Command Palette (`Cmd+Shift+P` on Mac or `Ctrl+Shift+P` on Windows/Linux)
   - Type "MCP: Start Server" and select it from the dropdown
   - Choose "AzBae" from the list of available servers

2. **Using Context Menu**:
   - In your VS Code settings.json file, hover over the server name ("AzBae")
   - Click on the "Start Server" button that appears above the server configuration

3. **Direct Command Line** (alternative method):
   - You can also run the server directly using the command line from the project directory:
   ```
   dotnet run
   ```
   - Note: When using this method, you'll need to ensure the environment variables are set separately

Once the server is running, it will be available for use with compatible AI assistants that support the Model Context Protocol.

## Example Usage

Once the MCP server is running, you can interact with it through Copilot or other LLM assistants that support MCP. Examples:

```
# Creating a new container
Please create a new CosmosDB container named "products" in the "ecommerce" database with "category" as the partition key.

# Querying data
Run a query on my "ecommerce" database, "products" container to find all items where price is greater than 100.

# Deleting records
Delete all products from the "ecommerce" database where the inventory count is zero.
```

## Related Projects

This project is part of the "Bae" collection of tools:

- [TestBae](https://github.com/mrcunninghamz/TestBae) - A .NET testing utility library that simplifies test setup with fixtures, mocks, and common testing patterns for more efficient and maintainable test suites.
- [NotionBae](https://github.com/mrcunninghamz/NotionBae) - A Model Context Protocol (MCP) Server for Notion.com integration, enabling AI assistants to interact with Notion workspaces through a structured API.

## License

MIT License