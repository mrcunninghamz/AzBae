# AzBae

AzBae is a tool for managing and interacting with Azure resources. The project consists of both a CLI and GUI interface.

## Project Status

### GUI
The GUI component is currently the main focus of development. It provides a user-friendly interface for managing Azure resources and is more feature-complete compared to the CLI.

### CLI
**Note:** The CLI functionality is currently in early development and many features may be limited or unavailable. We are prioritizing the GUI development at this time, but will expand CLI support in future releases.

## Getting Started

### Prerequisites
- .NET 9.0
- Azure subscription and credentials

### Installation
1. Clone the repository
2. Build the solution using `dotnet build AzBae.sln`
3. Configure your Azure credentials in the appropriate appsettings files

## Future Features

We're constantly working to improve AzBae. Here are some exciting features we're considering for future releases:

### MCP (Model Context Protocol) Server
We're planning to implement an MCP server for AzBae that will allow users to interact with their Azure resources using natural language through tools like GitHub Copilot. This will enable functionality such as:
- Deleting CosmosDB records through natural language commands
- Managing other Azure resources using simple language instructions
- Automating complex Azure operations with conversational AI

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.