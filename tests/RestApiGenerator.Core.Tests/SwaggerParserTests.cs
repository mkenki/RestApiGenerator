// tests/RestApiGenerator.Core.Tests/SwaggerParserTests.cs
using Newtonsoft.Json;
using RestApiGenerator.Core.Models;
using Xunit;

namespace RestApiGenerator.Core.Tests
{
    public class SwaggerParserTests
    {
        [Fact]
        public void Should_Parse_Simple_Swagger_Document()
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
                        ""url"": ""https://petstore.swagger.io/v2"",
                        ""description"": ""Pet store server""
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
                    }
                },
                ""components"": {
                    ""schemas"": {
                        ""Pet"": {
                            ""type"": ""object"",
                            ""required"": [""name"", ""photoUrls""],
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

            // Act
            var document = JsonConvert.DeserializeObject<SwaggerDocument>(swaggerJson);

            // Assert
            Assert.NotNull(document);
            Assert.Equal("3.0.0", document.OpenApi);
            Assert.Equal("Pet Store API", document.Info.Title);
            Assert.Equal("1.0.0", document.Info.Version);
            Assert.Single(document.Servers);
            Assert.Equal("https://petstore.swagger.io/v2", document.Servers[0].Url);
            
            // Path assertions
            Assert.Single(document.Paths);
            Assert.True(document.Paths.ContainsKey("/pet/{petId}"));
            
            var petPath = document.Paths["/pet/{petId}"];
            Assert.True(petPath.ContainsKey("get"));
            
            var getOperation = petPath["get"];
            Assert.Equal("getPetById", getOperation.OperationId);
            Assert.Single(getOperation.Parameters);
            Assert.Equal("petId", getOperation.Parameters[0].Name);
            Assert.Equal("path", getOperation.Parameters[0].In);
            Assert.True(getOperation.Parameters[0].Required);
            
            // Schema assertions
            Assert.Single(document.Components.Schemas);
            Assert.True(document.Components.Schemas.ContainsKey("Pet"));
            
            var petSchema = document.Components.Schemas["Pet"];
            Assert.Equal("object", petSchema.Type);
            Assert.NotNull(petSchema.Required); // Add null check
            Assert.Contains("name", petSchema.Required);
            Assert.Contains("photoUrls", petSchema.Required);
            Assert.NotNull(petSchema.Properties);
            Assert.True(petSchema.Properties.ContainsKey("id"));
            Assert.True(petSchema.Properties.ContainsKey("name"));
            Assert.True(petSchema.Properties.ContainsKey("status"));
        }

        [Fact]
        public void Should_Handle_Empty_Swagger_Document()
        {
            // Arrange
            var emptyJson = "{}";

            // Act
            var document = JsonConvert.DeserializeObject<SwaggerDocument>(emptyJson);

            // Assert
            Assert.NotNull(document);
            Assert.Empty(document.OpenApi);
            Assert.NotNull(document.Info);
            Assert.Empty(document.Paths);
            Assert.Empty(document.Servers);
        }
    }
}
