// tests/RestApiGenerator.Core.Tests/SwaggerParserTests.cs
using RestApiGenerator.Core.Converters;
using RestApiGenerator.Core.Generators;
using RestApiGenerator.Core.Models;
using RestApiGenerator.Core.Parsers;
using Xunit;
using Xunit.Abstractions;

namespace RestApiGenerator.Core.Tests
{
    public class SwaggerParserTests
    {
        private readonly ITestOutputHelper _output;

        public SwaggerParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Should_Parse_AllOf_Schema_Composition()
        {
            // Arrange - Test allOf with basic property merging
            var swaggerJson = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0""
                },
                ""servers"": [{""url"": ""https://api.example.com""}],
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
                ""components"": {
                    ""schemas"": {
                        ""BaseModel"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""id"": {""type"": ""string""},
                                ""createdAt"": {""type"": ""string"", ""format"": ""date-time""}
                            },
                            ""required"": [""id""]
                        },
                        ""User"": {
                            ""allOf"": [
                                {""$ref"": ""#/components/schemas/BaseModel""},
                                {
                                    ""type"": ""object"",
                                    ""properties"": {
                                        ""name"": {""type"": ""string""},
                                        ""email"": {""type"": ""string""}
                                    },
                                    ""required"": [""name"", ""email""]
                                }
                            ]
                        }
                    }
                }
            }";

            var parser = new SwaggerParser();
            var converter = new ModelConverter();
            var generator = new CSharpGenerator();

            // Act
            var swaggerDoc = await parser.ParseAsync(swaggerJson);
            var config = new GeneratorConfig { NamespaceName = "TestApi" };
            var codeModel = converter.ConvertToCodeModel(swaggerDoc, config);
            var generatedFiles = await generator.GenerateAllAsync(codeModel);

            // Assert
            var userModel = codeModel.Models.FirstOrDefault(m => m.Name == "User");
            Assert.NotNull(userModel);
            Assert.Contains(userModel.Properties, p => p.Name == "Id" && p.Type == "string" && p.IsRequired);
            Assert.Contains(userModel.Properties, p => p.Name == "CreatedAt" && p.Type == "DateTime");
            Assert.Contains(userModel.Properties, p => p.Name == "Name" && p.Type == "string" && p.IsRequired);
            Assert.Contains(userModel.Properties, p => p.Name == "Email" && p.Type == "string" && p.IsRequired);
        }

        [Fact]
        public async Task Should_Parse_OneOf_With_Discriminator()
        {
            // Arrange - Test oneOf with discriminator
            var swaggerJson = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0""
                },
                ""servers"": [{""url"": ""https://api.example.com""}],
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
                ""components"": {
                    ""schemas"": {
                        ""Pet"": {
                            ""type"": ""object"",
                            ""discriminator"": {
                                ""propertyName"": ""petType""
                            },
                            ""properties"": {
                                ""petType"": {""type"": ""string""},
                                ""name"": {""type"": ""string""}
                            },
                            ""required"": [""petType"", ""name""]
                        },
                        ""Cat"": {
                            ""allOf"": [
                                {""$ref"": ""#/components/schemas/Pet""},
                                {
                                    ""type"": ""object"",
                                    ""properties"": {
                                        ""huntingSkill"": {""type"": ""string"", ""enum"": [""clueless"", ""lazy"", ""adventurous"", ""aggressive""]}
                                    },
                                    ""required"": [""huntingSkill""]
                                }
                            ]
                        },
                        ""Dog"": {
                            ""allOf"": [
                                {""$ref"": ""#/components/schemas/Pet""},
                                {
                                    ""type"": ""object"",
                                    ""properties"": {
                                        ""packSize"": {""type"": ""integer"", ""format"": ""int32"", ""minimum"": 1}
                                    },
                                    ""required"": [""packSize""]
                                }
                            ]
                        }
                    }
                }
            }";

            var parser = new SwaggerParser();
            var converter = new ModelConverter();

            // Act
            var swaggerDoc = await parser.ParseAsync(swaggerJson);
            var config = new GeneratorConfig { NamespaceName = "TestApi" };
            var codeModel = converter.ConvertToCodeModel(swaggerDoc, config);

            // Assert
            var petModel = codeModel.Models.FirstOrDefault(m => m.Name == "Pet");
            Assert.NotNull(petModel);
            Assert.True(petModel.IsPolymorphic);
            Assert.Equal("petType", petModel.DiscriminatorProperty);

            var catModel = petModel.SubTypes.FirstOrDefault(st => st.Name == "Cat");
            Assert.NotNull(catModel);
            Assert.Contains(catModel.Properties, p => p.Name == "HuntingSkill");

            var dogModel = petModel.SubTypes.FirstOrDefault(st => st.Name == "Dog");
            Assert.NotNull(dogModel);
            Assert.Contains(dogModel.Properties, p => p.Name == "PackSize");
        }

        [Fact]
        public async Task Should_Parse_Complex_Schema_With_Nested_Refs()
        {
            // Arrange - Test complex schema with nested $ref resolution
            var swaggerJson = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0""
                },
                ""servers"": [{""url"": ""https://api.example.com""}],
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
                ""components"": {
                    ""schemas"": {
                        ""Address"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""street"": {""type"": ""string""},
                                ""city"": {""type"": ""string""}
                            }
                        },
                        ""Person"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""name"": {""type"": ""string""},
                                ""address"": {""$ref"": ""#/components/schemas/Address""}
                            }
                        },
                        ""Employee"": {
                            ""allOf"": [
                                {""$ref"": ""#/components/schemas/Person""},
                                {
                                    ""type"": ""object"",
                                    ""properties"": {
                                        ""salary"": {""type"": ""number""},
                                        ""department"": {""type"": ""string""}
                                    }
                                }
                            ]
                        }
                    }
                }
            }";

            var parser = new SwaggerParser();
            var converter = new ModelConverter();

            // Act
            var swaggerDoc = await parser.ParseAsync(swaggerJson);
            var config = new GeneratorConfig { NamespaceName = "TestApi" };
            var codeModel = converter.ConvertToCodeModel(swaggerDoc, config);

            // Assert
            var employeeModel = codeModel.Models.FirstOrDefault(m => m.Name == "Employee");
            Assert.NotNull(employeeModel);
            Assert.Contains(employeeModel.Properties, p => p.Name == "Name" && p.Type == "string");
            Assert.Contains(employeeModel.Properties, p => p.Name == "Address" && p.Type == "Address");
            Assert.Contains(employeeModel.Properties, p => p.Name == "Salary" && p.Type == "decimal");
            Assert.Contains(employeeModel.Properties, p => p.Name == "Department" && p.Type == "string");
        }
    }
}
