#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using RestApiGenerator.Core.Parsers;
using RestApiGenerator.Core.Converters;
using RestApiGenerator.Core.Generators;
using RestApiGenerator.Core.Models;

namespace RestApiGenerator.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var options = ParseArguments(args);
            if (options == null)
            {
                ShowUsage();
                return 1;
            }

            if (options.ShowHelp)
            {
                ShowUsage();
                return 0;
            }

            Console.WriteLine("🚀 RestApiGenerator - Swagger to C# Client Generator");
            Console.WriteLine($"📄 Input: {options.InputFile}");
            Console.WriteLine($"📁 Output: {options.OutputDirectory}");
            Console.WriteLine($"🏗️  Namespace: {options.Namespace}");
            Console.WriteLine($"⚡ Client: {options.ClientName}");
            Console.WriteLine();

            // Validate input file
            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine($"❌ Error: Input file '{options.InputFile}' not found.");
                return 1;
            }

            // Create output directory
            Directory.CreateDirectory(options.OutputDirectory);

            // Step 1: Parse Swagger document
            Console.WriteLine("🔄 Parsing Swagger document...");
            var parser = new SwaggerParser();
            var swaggerJson = await File.ReadAllTextAsync(options.InputFile);
            var swaggerDocument = await parser.ParseAsync(swaggerJson);
            Console.WriteLine($"✅ Parsed successfully: {swaggerDocument.Info.Title} v{swaggerDocument.Info.Version}");

            // Step 2: Convert to CodeModel
            Console.WriteLine("🔄 Converting to code model...");
            var converter = new ModelConverter();
            var config = new GeneratorConfig { NamespaceName = options.Namespace };
            var codeModel = converter.ConvertToCodeModel(swaggerDocument, config);
            Console.WriteLine($"✅ Generated {codeModel.Methods.Count} methods and {codeModel.Models.Count} models");

            // Step 3: Generate C# code
            Console.WriteLine("🔄 Generating C# code...");
            var generator = new CSharpGenerator();
            var generatedFiles = await generator.GenerateAllAsync(codeModel);
            Console.WriteLine($"✅ Generated {generatedFiles.Count} files");

            // Step 4: Write files to output directory
            Console.WriteLine("🔄 Writing files...");
            foreach (var file in generatedFiles)
            {
                var fileName = GetFileName(file.Key);
                var filePath = Path.Combine(options.OutputDirectory, fileName);
                await File.WriteAllTextAsync(filePath, file.Value);
                Console.WriteLine($"📄 Created: {fileName}");
            }

            Console.WriteLine();
            Console.WriteLine("🎉 Code generation completed successfully!");
            Console.WriteLine($"📁 Output directory: {Path.GetFullPath(options.OutputDirectory)}");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static string GetFileName(string fileKey)
    {
        return fileKey switch
        {
            "Interface" => "IApiClient.cs",
            "Client" => "ApiClient.cs", 
            "Models" => "Models.cs",
            _ => $"{fileKey}.cs"
        };
    }

    private static CliOptions? ParseArguments(string[] args)
    {
        var options = new CliOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-i":
                case "--input":
                    if (i + 1 < args.Length)
                        options.InputFile = args[++i];
                    else
                        return null;
                    break;

                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                        options.OutputDirectory = args[++i];
                    else
                        return null;
                    break;

                case "-n":
                case "--namespace":
                    if (i + 1 < args.Length)
                        options.Namespace = args[++i];
                    else
                        return null;
                    break;

                case "-c":
                case "--client":
                    if (i + 1 < args.Length)
                        options.ClientName = args[++i];
                    else
                        return null;
                    break;

                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                default:
                    Console.WriteLine($"Unknown argument: {args[i]}");
                    return null;
            }
        }

        // Validate required arguments
        if (!options.ShowHelp && string.IsNullOrEmpty(options.InputFile))
        {
            Console.WriteLine("Error: Input file (-i) is required.");
            return null;
        }

        return options;
    }

    private static void ShowUsage()
    {
        Console.WriteLine("RestApiGenerator CLI - Generate C# clients from Swagger/OpenAPI specs");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  RestApiGenerator.CLI -i <input-file> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -i, --input <file>        Swagger/OpenAPI JSON file path (required)");
        Console.WriteLine("  -o, --output <directory>  Output directory (default: ./generated)");
        Console.WriteLine("  -n, --namespace <name>    Target namespace (default: GeneratedApiClient)");
        Console.WriteLine("  -c, --client <name>       Client class name (default: ApiClient)");
        Console.WriteLine("  -h, --help                Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  RestApiGenerator.CLI -i petstore.json");
        Console.WriteLine("  RestApiGenerator.CLI -i petstore.json -o ./src -n MyApp.Client -c PetStoreClient");
        Console.WriteLine("  RestApiGenerator.CLI --input swagger.json --output ./generated --namespace MyApi");
    }
}

public class CliOptions
{
    public string InputFile { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = "./generated";
    public string Namespace { get; set; } = "GeneratedApiClient";
    public string ClientName { get; set; } = "ApiClient";
    public bool ShowHelp { get; set; } = false;
}
