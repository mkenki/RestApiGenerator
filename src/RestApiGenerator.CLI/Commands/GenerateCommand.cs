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
    public enum GenerationLanguage
    {
        CSharp,
        Java,
        TypeScript,
        Python
    }

    public class GenerateCommand : Command
    {
        public GenerateCommand() : base("generate", "Generate a client from a Swagger/OpenAPI specification")
        {
            var swaggerOption = new Option<FileInfo>(
                new[] { "--swagger", "-s" },
                "The path to the Swagger/OpenAPI specification file.")
            {
                IsRequired = true
            };

            var languageOption = new Option<GenerationLanguage>(
                new[] { "--language", "-l" },
                "Target programming language (CSharp, Java, TypeScript, Python)")
            {
                IsRequired = false
            };

            var outputOption = new Option<DirectoryInfo>(
                new[] { "--output", "-o" },
                "The directory where the generated files will be saved.")
            {
                IsRequired = true
            };

            var namespaceOption = new Option<string>(
                new[] { "--namespace", "-n" },
                "The namespace/package for the generated client.")
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
                "Authentication type (None, ApiKey, Bearer, OAuth2AuthorizationCode, OAuth2ClientCredentials, OAuth2Password)")
            {
                IsRequired = false
            };

            var authLocationOption = new Option<AuthenticationLocation>(
                new[] { "--auth-location" },
                "Location for API Key (None, Header, Query)")
            {
                IsRequired = false
            };

            var authNameOption = new Option<string>(
                new[] { "--auth-name" },
                "Name of the authentication header or query parameter")
            {
                IsRequired = false
            };

            // OAuth2 specific options
            var oauth2ClientIdOption = new Option<string>(
                new[] { "--oauth2-client-id" },
                "OAuth2 client ID")
            {
                IsRequired = false
            };

            var oauth2ClientSecretOption = new Option<string>(
                new[] { "--oauth2-client-secret" },
                "OAuth2 client secret")
            {
                IsRequired = false
            };

            var oauth2TokenEndpointOption = new Option<string>(
                new[] { "--oauth2-token-endpoint" },
                "OAuth2 token endpoint URL")
            {
                IsRequired = false
            };

            var oauth2ScopesOption = new Option<string>(
                new[] { "--oauth2-scopes" },
                "OAuth2 scopes (comma-separated)")
            {
                IsRequired = false
            };

            // Plugin management options
            var pluginInstallOption = new Option<string>(
                new[] { "--plugin-install", "-pi" },
                "Install a plugin by name")
            {
                IsRequired = false
            };

            var pluginListOption = new Option<bool>(
                new[] { "--plugin-list", "-pl" },
                "List all installed plugins")
            {
                IsRequired = false
            };

            var pluginUpdateOption = new Option<string>(
                new[] { "--plugin-update", "-pu" },
                "Update a plugin by name")
            {
                IsRequired = false
            };

            var pluginRemoveOption = new Option<string>(
                new[] { "--plugin-remove", "-pr" },
                "Remove a plugin by name")
            {
                IsRequired = false
            };

            AddOption(swaggerOption);
            AddOption(languageOption);
            AddOption(outputOption);
            AddOption(namespaceOption);
            AddOption(clientNameOption);
            AddOption(authTypeOption);
            AddOption(authLocationOption);
            AddOption(authNameOption);
            AddOption(oauth2ClientIdOption);
            AddOption(oauth2ClientSecretOption);
            AddOption(oauth2TokenEndpointOption);
            AddOption(oauth2ScopesOption);
            AddOption(pluginInstallOption);
            AddOption(pluginListOption);
            AddOption(pluginUpdateOption);
            AddOption(pluginRemoveOption);

            this.SetHandler(async (context) =>
            {
                var swaggerFile = context.ParseResult.GetValueForOption(swaggerOption);
                var language = context.ParseResult.GetValueForOption(languageOption);
                if (language == null)
                    language = GenerationLanguage.CSharp;
                var outputDir = context.ParseResult.GetValueForOption(outputOption);
                var namespaceName = context.ParseResult.GetValueForOption(namespaceOption);
                var clientName = context.ParseResult.GetValueForOption(clientNameOption);
                var authType = context.ParseResult.GetValueForOption(authTypeOption);
                var authLocation = context.ParseResult.GetValueForOption(authLocationOption);
                var authName = context.ParseResult.GetValueForOption(authNameOption);
                var oauth2ClientId = context.ParseResult.GetValueForOption(oauth2ClientIdOption);
                var oauth2ClientSecret = context.ParseResult.GetValueForOption(oauth2ClientSecretOption);
                var oauth2TokenEndpoint = context.ParseResult.GetValueForOption(oauth2TokenEndpointOption);
                var oauth2Scopes = context.ParseResult.GetValueForOption(oauth2ScopesOption);

                await HandleGeneration(swaggerFile!, language, outputDir!, namespaceName, clientName,
                    authType, authLocation, authName, oauth2ClientId, oauth2ClientSecret,
                    oauth2TokenEndpoint, oauth2Scopes);
            });
        }

        private async Task HandleGeneration(
            FileInfo swaggerFile,
            GenerationLanguage language,
            DirectoryInfo outputDir,
            string? namespaceName,
            string? clientName,
            AuthenticationType? authType,
            AuthenticationLocation? authLocation,
            string? authName,
            string? oauth2ClientId,
            string? oauth2ClientSecret,
            string? oauth2TokenEndpoint,
            string? oauth2Scopes)
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
                        Name = authName ?? string.Empty,
                        OAuth2 = new OAuth2Config
                        {
                            ClientId = oauth2ClientId ?? "",
                            ClientSecret = oauth2ClientSecret ?? "",
                            TokenEndpoint = oauth2TokenEndpoint ?? "",
                            Scopes = oauth2Scopes ?? ""
                        }
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

                ICodeGenerator generator;
                switch (language)
                {
                    case GenerationLanguage.CSharp:
                        generator = new CSharpGenerator();
                        break;
                    case GenerationLanguage.Java:
                        generator = new JavaGenerator();
                        break;
                    case GenerationLanguage.TypeScript:
                        generator = new TypeScriptGenerator();
                        break;
                    case GenerationLanguage.Python:
                        generator = new PythonGenerator();
                        break;
                    default:
                        generator = new CSharpGenerator();
                        break;
                }

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
