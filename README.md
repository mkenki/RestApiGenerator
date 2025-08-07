# RestApiGenerator

This project aims to provide a robust and flexible solution for generating REST API client code from Swagger/OpenAPI specifications. It is designed to be easily extensible and integrateable into various development workflows.

## RestApiGenerator.Core

The `RestApiGenerator.Core` library is the core component of this project, offering functionalities for:
- Parsing Swagger/OpenAPI specifications (JSON/YAML).
- Converting API schemas into structured, language-agnostic code models.
- Generating client code in various programming languages (currently C# is fully supported).
- Providing extensibility points for custom code generation logic and plugin integration.

### Installation

You can install `RestApiGenerator.Core` via NuGet Package Manager. Once published, you can use the following command:

```bash
dotnet add package RestApiGenerator.Core
```

### Usage

To use `RestApiGenerator.Core` programmatically in your .NET project:

1.  **Add the NuGet package** to your project.
2.  **Instantiate the parser and generator**:
using RestApiGenerator.Core.Parsers;
using RestApiGenerator.Core.Generators;
using RestApiGenerator.Core.Converters;
using RestApiGenerator.Core.Models;
using System.IO;
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting RestApiGenerator.Core test application...");

        // Example Swagger/OpenAPI JSON content
        // For a real test, you can use your own petstore.json or a similar file.
        string swaggerContent = @"{
            ""swagger"": ""2.0"",
            ""info"": {
                ""version"": ""1.0.0"",
                ""title"": ""Test API""
            },
            ""paths"": {
                ""/test"": {
                    ""get"": {
                        ""summary"": ""Get test data"",
                        ""responses"": {
                            ""200"": {
                                ""description"": ""Successful response""
                            }
                        }
                    }
                }
            },
            ""definitions"": {
                ""TestModel"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""id"": { ""type"": ""integer"", ""format"": ""int64"" },
                        ""name"": { ""type"": ""string"" }
                    }
                }
            }
        }";

        string outputDirectory = "GeneratedClient";
        string namespaceName = "MyTestApiClient";

        try
        {
            // 1. Parse the Swagger/OpenAPI document
            IApiParser parser = new SwaggerParser();
            SwaggerDocument swaggerDocument = await parser.ParseAsync(swaggerContent);
            Console.WriteLine("Swagger document parsed successfully.");

            // 2. Convert Swagger schema to a generic code model
            ModelConverter converter = new ModelConverter();
            CodeModel codeModel = converter.ConvertToCodeModel(swaggerDocument, new GeneratorConfig { NamespaceName = namespaceName });
            Console.WriteLine("Code model converted successfully.");

            // 3. Generate C# code
            ICodeGenerator csharpGenerator = new CSharpGenerator();
            var generatedFiles = await csharpGenerator.GenerateAllAsync(codeModel);
            Console.WriteLine($"{generatedFiles.Count} files generated.");

            // 4. Save the generated files
            Directory.CreateDirectory(outputDirectory);
            foreach (var file in generatedFiles)
            {
                string filePath = Path.Combine(outputDirectory, file.Key);
                File.WriteAllText(filePath, file.Value);
                Console.WriteLine($"Generated file: {filePath}");
            }

            Console.WriteLine("Code generation completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}


## RestApiGenerator.CLI

The `RestApiGenerator.CLI` project provides a command-line interface for easy interaction with the core library. This is ideal for quick code generation or integration into build scripts without requiring programmatic setup.

### Installation

The `RestApiGenerator.CLI` tool can be installed globally as a .NET tool once it's published to NuGet.org:

```bash
dotnet tool install --global RestApiGenerator.CLI
```

### Usage

Here are examples of how to use the `RestApiGenerator.CLI` tool:

```bash
# Basic usage with default output and namespace
RestApiGenerator.CLI -i petstore.json

# Specify output directory, target namespace, and client class name
RestApiGenerator.CLI -i petstore.json -o ./src -n MyApp.Client -c PetStoreClient

# Using long-form arguments
RestApiGenerator.CLI --input swagger.json --output ./generated --namespace MyApi
```

For more details on available options, you can run:

```bash
RestApiGenerator.CLI --help
```

## Contributing

We welcome contributions to the RestApiGenerator project! If you'd like to contribute, please follow these steps:
1.  Fork the repository.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and ensure tests pass.
4.  Submit a pull request with a clear description of your changes.

Please ensure your code adheres to the existing coding style and includes appropriate tests.

## License

This project is licensed under the MIT License. See the `LICENSE` file in the root of the repository for more details.
