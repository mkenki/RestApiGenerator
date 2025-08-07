# RestApiGenerator.Core

The `RestApiGenerator.Core` library is the foundational component of this project. It provides the essential functionalities required for processing Swagger/OpenAPI specifications and generating client code. This library is designed to be highly modular and extensible, allowing developers to integrate its capabilities into their .NET applications or extend its features with custom logic.

## Key Functionalities:

*   **Swagger/OpenAPI Specification Parsing**:
    *   Parses API definitions from Swagger (2.0) and OpenAPI (3.0+) specifications.
    *   Supports both JSON and YAML formats for input specifications.
    *   Transforms raw specification data into a structured `SwaggerDocument` object for internal processing.

*   **Code Model Conversion**:
    *   Converts the parsed `SwaggerDocument` into a language-agnostic `CodeModel`.
    *   The `CodeModel` represents the API's structure (endpoints, methods, parameters, data models, enums) in a standardized format, independent of any specific programming language.
    *   This abstraction allows for generating client code in various languages from a single, unified model.

*   **C# Client Code Generation**:
    *   Generates C# client code based on the `CodeModel`.
    *   Produces well-structured and idiomatic C# code, including:
        *   Interfaces for API clients.
        *   Concrete client implementations.
        *   Data models (classes and enums) corresponding to the API's schemas.
    *   Includes support for common HTTP operations (GET, POST, PUT, DELETE) and request/response handling.

*   **Extensibility and Plugin Integration**:
    *   Designed with extensibility points to allow for custom code generation templates or logic.
    *   Supports integration with external plugins to extend its capabilities, such as adding support for new authentication methods or custom data transformations.

## Installation

You can install `RestApiGenerator.Core` via NuGet Package Manager. Once published, you can use the following command in your .NET project:

```bash
dotnet add package RestApiGenerator.Core
```

## Usage

To use `RestApiGenerator.Core` programmatically in your .NET project, follow these steps:

1.  **Add the NuGet package** to your project.
2.  **Instantiate the parser and generator**:

    ```csharp
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
