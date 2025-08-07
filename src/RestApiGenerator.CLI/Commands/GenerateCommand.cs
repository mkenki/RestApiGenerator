#nullable enable

using System.CommandLine;
using System;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using RestApiGenerator.Core.Converters;
using RestApiGenerator.Core.Generators;
using RestApiGenerator.Core.Models; // Added this line
using RestApiGenerator.Core.Parsers;
using static System.Console; // Added this line

namespace RestApiGenerator.CLI.Commands
{
    public class GenerateCommand : Command
    {
        public GenerateCommand() : base("generate", "Generate a C# client from a Swagger/OpenAPI specification")
        {
            var swaggerOption = new Option<FileInfo>(
                new[] { "--swagger", "-s" },
                "The path to the Swagger/OpenAPI specification file.")
            {
                IsRequired = true
            };

            var outputOption = new Option<DirectoryInfo>(
                new[] { "--output", "-o" },
                "The directory where the generated C# files will be saved.")
            {
                IsRequired = true
            };

            var namespaceOption = new Option<string>(
                new[] { "--namespace", "-n" },
                "The namespace for the generated client.")
            {
                IsRequired = false
            };

            var clientNameOption = new Option<string>(
                new[] { "--client", "-c" },
                "The name of the generated client class.")
            {
                IsRequired = false
            };

            // Add new authentication options
            var authTypeOption = new Option<AuthenticationType>(
                new[] { "--auth-type" },
                "Authentication type (ApiKey or Bearer)")
            {
                IsRequired = false
            };

            var authLocationOption = new Option<AuthenticationLocation>(
                new[] { "--auth-location" },
                "Location for API Key (header or query)")
            {
                IsRequired = false
            };

            var authNameOption = new Option<string>(
                new[] { "--auth-name" },
                "Name of the authentication header or query parameter")
            {
                IsRequired = false
            };

            AddOption(swaggerOption);
            AddOption(outputOption);
            AddOption(namespaceOption);
            AddOption(clientNameOption); // Add this line
            AddOption(authTypeOption);
            AddOption(authLocationOption);
            AddOption(authNameOption);

            this.SetHandler(async (context) =>
            {
                var swaggerFile = context.ParseResult.GetValueForOption(swaggerOption);
                var outputDir = context.ParseResult.GetValueForOption(outputOption);
                var namespaceName = context.ParseResult.GetValueForOption(namespaceOption);
                var clientName = context.ParseResult.GetValueForOption(clientNameOption); // Get client name
                var authType = context.ParseResult.GetValueForOption(authTypeOption);
                var authLocation = context.ParseResult.GetValueForOption(authLocationOption);
                var authName = context.ParseResult.GetValueForOption(authNameOption);

                var config = new RestApiGenerator.Core.Models.GeneratorConfig
                {
                    NamespaceName = namespaceName ?? "GeneratedApi",
                    ClientName = clientName ?? "GeneratedClient", // Pass client name to config
                    Authentication = new AuthenticationConfig
                    {
                        Type = authType,
                        Location = authLocation,
                        Name = authName ?? string.Empty
                    }
                };

                await HandleGeneration(swaggerFile!, outputDir!, namespaceName, clientName, authType, authLocation, authName); // Pass clientName
            });
        }

        private async Task HandleGeneration(
            FileInfo swaggerFile,
            DirectoryInfo outputDir,
            string? namespaceName,
            string? clientName, // Add clientName parameter
            AuthenticationType? authType,
            AuthenticationLocation? authLocation,
            string? authName)
        {
            try
            {
                var config = new RestApiGenerator.Core.Models.GeneratorConfig
                {
                    NamespaceName = namespaceName ?? "GeneratedApi",
                    ClientName = clientName ?? "GeneratedClient", // Set client name in config
                    Authentication = new AuthenticationConfig
                    {
                        Type = authType ?? AuthenticationType.None,
                        Location = authLocation ?? AuthenticationLocation.None,
                        Name = authName ?? string.Empty
                    }
                };

                config.Validate();
                
                if (!swaggerFile.Exists)
                {
                    await Error.WriteLineAsync($"Error: Swagger file not found at '{swaggerFile.FullName}'");
                    return;
                }

                if (!outputDir.Exists)
                {
                    outputDir.Create();
                }

                var parser = new SwaggerParser();
                var swaggerDocument = await parser.ParseFromFileAsync(swaggerFile.FullName);

                var converter = new ModelConverter();
                var codeModel = converter.ConvertToCodeModel(swaggerDocument, config);

                var generator = new CSharpGenerator();
                var generatedCode = await generator.GenerateAllAsync(codeModel);

                foreach (var entry in generatedCode)
                {
                    var fileName = entry.Key;
                    var content = entry.Value;
                    var filePath = Path.Combine(outputDir.FullName, fileName);
                    await File.WriteAllTextAsync(filePath, content);
                    WriteLine($"Generated file: {filePath}");
                }

                WriteLine("Code generation completed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                await Error.WriteLineAsync($"Configuration error: {ex.Message}");
                return;
            }
        }
    }
}
