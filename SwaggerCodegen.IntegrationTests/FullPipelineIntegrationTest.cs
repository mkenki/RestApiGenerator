using Xunit;
using RestApiGenerator.Core.Parsers;
using RestApiGenerator.Core.Converters;
using RestApiGenerator.Core.Generators;
using System.Threading.Tasks;
using System.IO;

namespace SwaggerCodegen.IntegrationTests;

public class FullPipelineIntegrationTest
{
    private const string PetStoreSwaggerJson = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""Pet Store API"",
    ""version"": ""1.0.0"",
    ""description"": ""A simple Pet Store API""
  },
  ""servers"": [
    {
      ""url"": ""https://petstore.swagger.io/v2""
    }
  ],
  ""paths"": {
    ""/pets"": {
      ""get"": {
        ""summary"": ""List all pets"",
        ""operationId"": ""listPets"",
        ""responses"": {
          ""200"": {
            ""description"": ""An array of pets"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""array"",
                  ""items"": {
                    ""$ref"": ""#/components/schemas/Pet""
                  }
                }
              }
            }
          }
        }
      },
      ""post"": {
        ""summary"": ""Create a pet"",
        ""operationId"": ""createPet"",
        ""requestBody"": {
          ""required"": true,
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""$ref"": ""#/components/schemas/CreatePetRequest""
              }
            }
          }
        },
        ""responses"": {
          ""201"": {
            ""description"": ""Pet created successfully"",
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
    ""/pets/{petId}"": {
      ""get"": {
        ""summary"": ""Get a pet by ID"",
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
            ""description"": ""Pet details"",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/Pet""
                }
              }
            }
          },
          ""404"": {
            ""description"": ""Pet not found""
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {
      ""Pet"": {
        ""type"": ""object"",
        ""required"": [""id"", ""name""],
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
          },
          ""category"": {
            ""$ref"": ""#/components/schemas/Category""
          }
        }
      },
      ""Category"": {
        ""type"": ""object"",
        ""properties"": {
          ""id"": {
            ""type"": ""integer"",
            ""format"": ""int64""
          },
          ""name"": {
            ""type"": ""string""
          }
        }
      },
      ""CreatePetRequest"": {
        ""type"": ""object"",
        ""required"": [""name""],
        ""properties"": {
          ""name"": {
            ""type"": ""string""
          },
          ""status"": {
            ""type"": ""string"",
            ""enum"": [""available"", ""pending"", ""sold""]
          },
          ""categoryId"": {
            ""type"": ""integer"",
            ""format"": ""int64""
          }
        }
      }
    }
  }
}";

    [Fact]
    public async Task FullPipeline_PetStoreSwagger_GeneratesValidCSharpCode()
    {
        // Arrange
        var parser = new SwaggerParser();
        var converter = new ModelConverter();
        var generator = new CSharpGenerator();

        // Arrange - GeneratorConfig
        var config = new RestApiGenerator.Core.Models.GeneratorConfig
        {
            NamespaceName = "PetStoreClient",
            ClientName = "PetStoreApiClient", // Set ClientName explicitly
            Authentication = new RestApiGenerator.Core.Models.AuthenticationConfig() // Initialize properly
        };

        // Act - Step 1: Parse Swagger JSON to SwaggerDocument
        var swaggerDocument = await parser.ParseAsync(PetStoreSwaggerJson);
        
        // Act - Step 2: Convert SwaggerDocument to CodeModel
        var codeModel = converter.ConvertToCodeModel(swaggerDocument, config);
        
        // Act - Step 3: Generate C# code from CodeModel
        var generatedFiles = await generator.GenerateAllAsync(codeModel);

        // Assert - Pipeline completed successfully
        Assert.NotNull(swaggerDocument);
        Assert.NotNull(codeModel);
        Assert.NotNull(generatedFiles);
        Assert.True(generatedFiles.Count > 0);

        // Assert - Verify SwaggerDocument parsing
        Assert.Equal("Pet Store API", swaggerDocument.Info.Title);
        Assert.Equal("1.0.0", swaggerDocument.Info.Version);
        Assert.True(swaggerDocument.Paths.Count > 0);
        Assert.True(swaggerDocument.Components.Schemas.Count > 0);

        // Assert - Verify CodeModel conversion
        Assert.NotNull(codeModel.ClientName);
        Assert.True(codeModel.Methods.Count > 0);
        Assert.True(codeModel.Models.Count > 0);

        // Output what keys are available
        System.Console.WriteLine("Available generated file keys:");
        foreach (var key in generatedFiles.Keys)
        {
            System.Console.WriteLine($"- {key}");
        }

        // Output generated code to files for inspection
        foreach (var file in generatedFiles)
        {
            var fileName = $"generated-{file.Key.ToLower().Replace(" ", "-")}.cs";
            await File.WriteAllTextAsync(fileName, file.Value);
            System.Console.WriteLine($"Generated file: {fileName}");
        }
        
        // Output to console for immediate viewing
        foreach (var file in generatedFiles)
        {
            System.Console.WriteLine($"\n=== {file.Key} ===");
            System.Console.WriteLine(file.Value);
            System.Console.WriteLine("=".PadRight(50, '='));
        }
    }

    [Fact]
    public async Task FullPipeline_InvalidJson_ThrowsException()
    {
        // Arrange
        var parser = new SwaggerParser();
        var invalidJson = "{ invalid json }";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => parser.ParseAsync(invalidJson));
    }

    [Fact]
    public async Task FullPipeline_EmptySwagger_HandlesGracefully()
    {
        // Arrange
        var parser = new SwaggerParser();
        var converter = new ModelConverter();
        var generator = new CSharpGenerator();
        
        var emptySwagger = @"{
            ""openapi"": ""3.0.0"",
            ""info"": { ""title"": ""Empty API"", ""version"": ""1.0.0"" },
            ""paths"": {
                ""/test"": {
                    ""get"": {
                        ""summary"": ""Test endpoint"",
                        ""responses"": {
                            ""200"": {
                                ""description"": ""Success""
                            }
                        }
                    }
                }
            },
            ""components"": { ""schemas"": {} }
        }";

        // Arrange - GeneratorConfig for empty swagger test
        var emptyConfig = new RestApiGenerator.Core.Models.GeneratorConfig
        {
            NamespaceName = "EmptyClient",
            ClientName = "EmptyApiClient", // Set ClientName explicitly
            Authentication = new RestApiGenerator.Core.Models.AuthenticationConfig() // Initialize properly
        };

        // Act
        var swaggerDocument = await parser.ParseAsync(emptySwagger);
        var codeModel = converter.ConvertToCodeModel(swaggerDocument, emptyConfig);
        var generatedFiles = await generator.GenerateAllAsync(codeModel);

        // Assert
        Assert.NotNull(swaggerDocument);
        Assert.NotNull(codeModel);
        Assert.NotNull(generatedFiles);
        Assert.Equal("Empty API", swaggerDocument.Info.Title);
    }
}
