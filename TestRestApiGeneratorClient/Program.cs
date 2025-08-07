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
