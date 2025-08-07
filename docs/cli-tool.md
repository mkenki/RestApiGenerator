# RestApiGenerator.CLI

The `RestApiGenerator.CLI` project provides a convenient command-line interface (CLI) for interacting with the `RestApiGenerator.Core` library. This tool is designed for developers who need to quickly generate REST API client code without setting up a programmatic environment, or for integrating code generation into automated build scripts and CI/CD pipelines.

## Key Features:

*   **Easy Integration**: Seamlessly integrates into existing development workflows and build processes.
*   **Simplified Usage**: Provides a straightforward command-line syntax for common code generation tasks.
*   **Configurable Output**: Allows specification of input Swagger/OpenAPI file, output directory, target namespace, and client class name.
*   **Cross-Platform**: As a .NET tool, it can be installed and run on any platform supported by .NET.

## Installation

The `RestApiGenerator.CLI` tool can be installed globally as a .NET tool once it's published to NuGet.org. This makes the `restapigenerator` command available directly from your terminal.

To install the tool globally, use the following command:

```bash
dotnet tool install --global RestApiGenerator.CLI
```

If you need to update to a newer version, you can use:

```bash
dotnet tool update --global RestApiGenerator.CLI
```

## Usage

Here are examples of how to use the `RestApiGenerator.CLI` tool. The primary command is `restapigenerator`, followed by options to specify your input and desired output.

### Basic Usage

To generate code with default settings (output to `./generated`, namespace `GeneratedApiClient`, client name `ApiClient`):

```bash
restapigenerator -i petstore.json
```

Or using the long-form argument:

```bash
restapigenerator --input petstore.json
```

### Specifying Output Directory, Namespace, and Client Name

You can customize the output location, the C# namespace for the generated code, and the name of the main API client class:

```bash
restapigenerator -i petstore.json -o ./src/Clients -n MyApp.ApiClients -c PetStoreApiClient
```

Using long-form arguments for clarity:

```bash
restapigenerator --input swagger.json --output ./generated --namespace MyApi --client MyApiClient
```

### Getting Help

To view all available options and a detailed usage guide, you can run the help command:

```bash
restapigenerator --help
```

This will display information similar to the following:

```
RestApiGenerator CLI - Generate C# clients from Swagger/OpenAPI specs

Usage:
  RestApiGenerator.CLI -i <input-file> [options]

Options:
  -i, --input <file>        Swagger/OpenAPI JSON file path (required)
  -o, --output <directory>  Output directory (default: ./generated)
  -n, --namespace <name>    Target namespace (default: GeneratedApiClient)
  -c, --client <name>       Client class name (default: ApiClient)
  -h, --help                Show this help message

Examples:
  RestApiGenerator.CLI -i petstore.json
  RestApiGenerator.CLI -i petstore.json -o ./src -n MyApp.Client -c PetStoreClient
  RestApiGenerator.CLI --input swagger.json --output ./generated --namespace MyApi
