# AzBae

AzBae is a tool for managing and interacting with Azure resources. The project consists of both a CLI and GUI interface.

## Project Status

### GUI
The GUI component is currently the main focus of development. It provides a user-friendly interface for managing Azure resources and is more feature-complete compared to the CLI.

See the [GUI README](./src/GUI/README.md) for detailed information on features and configuration.

### CLI
**Note:** The CLI functionality is currently in early development and many features may be limited or unavailable. We are prioritizing the GUI development at this time, but will expand CLI support in future releases.

### MCP (Model Context Protocol) Server
The MCP server implementation is now available, allowing users to interact with Azure resources using natural language through tools like GitHub Copilot.


## Getting Started

### Prerequisites
- .NET 9.0
- Azure subscription and credentials

### Installation
1. Clone the repository
2. Build the solution using `dotnet build AzBae.sln`
3. Configure your Azure credentials in the appropriate appsettings files

### Setting up the MCP Server in VS Code
To use AzBae.MCP as an MCP server in VS Code:

1. Add the MCP server configuration to your VS Code `settings.json` file
2. Configure your Cosmos DB credentials
3. Start the server using VS Code's MCP commands

See the [MCP README](./src/MCP/README.md) for detailed setup instructions.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.