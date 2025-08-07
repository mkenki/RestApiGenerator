// tests/RestApiGenerator.Core.Tests/IntegrationTests.cs
using RestApiGenerator.Core.Converters;
using RestApiGenerator.Core.Generators;
using RestApiGenerator.Core.Parsers;
using RestApiGenerator.Core.Models; // Added this line
using Xunit;
using Xunit.Abstractions;

namespace RestApiGenerator.Core.Tests
{
    public class IntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public IntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Should_Generate_Complete_Client_From_Swagger()
        {
            // Arrange
            var swaggerJson = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""Pet Store API"",
                    ""version"": ""1.0.0"",
                    ""description"": ""A simple pet store API""
                },
                ""servers"": [
                    {
                        ""url"": ""https://petstore.swagger.io/v2""
                    }
                ],
                ""paths"": {
                    ""/pet/{petId}"": {
                        ""get"": {
                            ""summary"": ""Find pet by ID"",
                            ""operationId"": ""getPetById"",
                            ""parameters"": [
                                {
                                    ""name"": ""petId"",
                                    ""in"": ""path"",
                                    ""required"": true,
                                    ""schema"": {
                                        ""type"": ""integer"",
                                        ""format"": ""int64""
                                    }
                                }
                            ],
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""successful operation"",
                                    ""content"": {
                                        ""application/json"": {
                                            ""schema"": {
                                                ""$ref"": ""#/components/schemas/Pet""
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ""/pet"": {
                        ""post"": {
                            ""summary"": ""Add a new pet"",
                            ""operationId"": ""addPet"",
                            ""requestBody"": {
                                ""required"": true,
                                ""content"": {
                                    ""application/json"": {
                                        ""schema"": {
                                            ""$ref"": ""#/components/schemas/Pet""
                                        }
                                    }
                                }
                            },
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""successful operation"",
                                    ""content"": {
                                        ""application/json"": {
                                            ""schema"": {
                                                ""$ref"": ""#/components/schemas/Pet""
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                ""components"": {
                    ""schemas"": {
                        ""Pet"": {
                            ""type"": ""object"",
                            ""required"": [""name""],
                            ""properties"": {
                                ""id"": {
                                    ""type"": ""integer"",
                                    ""format"": ""int64""
                                },
                                ""name"": {
                                    ""type"": ""string""
                                },
                                ""status"": {
                                    ""type"": ""string"",
                                    ""enum"": [""available"", ""pending"", ""sold""]
                                }
                            }
                        }
                    }
                }
            }";

            var parser = new SwaggerParser();
            var converter = new ModelConverter();
            var generator = new CSharpGenerator();

            // Act
            var swaggerDoc = await parser.ParseAsync(swaggerJson);
            var config = new GeneratorConfig { NamespaceName = "PetStoreApiClient" };
            var codeModel = converter.ConvertToCodeModel(swaggerDoc, config);
            var generatedFiles = await generator.GenerateAllAsync(codeModel);

            // Assert
            Assert.NotNull(swaggerDoc);
            Assert.NotNull(codeModel);
            Assert.NotEmpty(generatedFiles);

            // Verify generated files
            Assert.True(generatedFiles.ContainsKey("IPetStoreApiClient.cs"));
            Assert.True(generatedFiles.ContainsKey("PetStoreApiClient.cs"));
            Assert.True(generatedFiles.ContainsKey("Models.cs"));

            // Verify interface content
            var interfaceCode = generatedFiles["IPetStoreApiClient.cs"];
            _output.WriteLine(interfaceCode);
            Assert.Contains("interface IPetStoreApiClient", interfaceCode);
            Assert.Contains("Task<Pet> GetPetById(long petId, CancellationToken cancellationToken = default);", interfaceCode);
            Assert.Contains("Task<Pet> AddPet(Pet request, CancellationToken cancellationToken = default);", interfaceCode);

            // Verify client content
            var clientCode = generatedFiles["PetStoreApiClient.cs"];
            Assert.Contains("class PetStoreApiClient : IPetStoreApiClient", clientCode);
            Assert.Contains("public async Task<Pet> GetPetById(long petId)", clientCode);
            Assert.Contains("public async Task<Pet> AddPet(Pet request)", clientCode);
            Assert.Contains("HttpMethod.GET", clientCode);
            Assert.Contains("HttpMethod.POST", clientCode);

            // Verify models content
            var modelsCode = generatedFiles["Models.cs"];
            Assert.Contains("class Pet", modelsCode);
            Assert.Contains("public long? Id { get; set; }", modelsCode);
            Assert.Contains("public string Name { get; set; }", modelsCode);
            Assert.Contains("public string? Status { get; set; }", modelsCode);

            // Output for manual inspection
            _output.WriteLine("=== Generated Interface ===");
            _output.WriteLine(interfaceCode);
            _output.WriteLine("=== Generated Client ===");
            _output.WriteLine(clientCode);
            _output.WriteLine("=== Generated Models ===");
            _output.WriteLine(modelsCode);
        }
    }
}
