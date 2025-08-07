// tests/RestApiGenerator.Core.Tests/SwaggerParserAdvancedTests.cs
using RestApiGenerator.Core.Parsers;
using Xunit;

namespace RestApiGenerator.Core.Tests
{
    public class SwaggerParserAdvancedTests
    {
        private readonly SwaggerParser _parser;

        public SwaggerParserAdvancedTests()
        {
            _parser = new SwaggerParser();
        }

        [Fact]
        public async Task Should_Parse_Valid_Swagger_JSON_String()
        {
            // Arrange
            var swaggerJson = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0""
                },
                ""paths"": {
                    ""/test"": {
                        ""get"": {
                            ""operationId"": ""getTest"",
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""success""
                                }
                            }
                        }
                    }
                }
            }";

            // Act
            var document = await _parser.ParseAsync(swaggerJson);

            // Assert
            Assert.NotNull(document);
            Assert.Equal("3.0.0", document.OpenApi);
            Assert.Equal("Test API", document.Info.Title);
            Assert.Single(document.Paths);
        }

        [Fact]
        public async Task Should_Parse_File_When_Valid_Path_Provided()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var swaggerJson = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""File Test API"",
                    ""version"": ""1.0.0""
                },
                ""paths"": {
                    ""/test"": {
                        ""get"": {
                            ""responses"": {""200"": {""description"": ""success""}}
                        }
                    }
                }
            }";
            
            await File.WriteAllTextAsync(tempFile, swaggerJson);

            try
            {
                // Act
                var document = await _parser.ParseAsync(tempFile);

                // Assert
                Assert.NotNull(document);
                Assert.Equal("File Test API", document.Info.Title);
            }
            finally
            {
                // Cleanup
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task Should_Throw_Exception_For_Invalid_JSON()
        {
            // Arrange
            var invalidJson = @"{ ""invalid"": json }";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _parser.ParseAsync(invalidJson)
            );
        }

        [Fact]
        public async Task Should_Throw_Exception_For_Missing_Required_Fields()
        {
            // Arrange - Missing info section
            var invalidSwagger = @"{
                ""openapi"": ""3.0.0""
            }";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _parser.ParseAsync(invalidSwagger)
            );
            
            Assert.Contains("must have an 'info' section", exception.Message);
        }

        [Fact]
        public async Task Should_Throw_Exception_For_Missing_Paths()
        {
            // Arrange - Missing paths
            var invalidSwagger = @"{
                ""openapi"": ""3.0.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0""
                }
            }";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _parser.ParseAsync(invalidSwagger)
            );
            
            Assert.Contains("must have at least one path", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Should_Throw_Exception_For_Empty_Input(string input)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _parser.ParseAsync(input)
            );
        }

        [Theory]
        [InlineData(@"{""openapi"":""3.0.0"",""info"":{""title"":""API"",""version"":""1.0""},""paths"":{""/test"":{""get"":{""responses"":{""200"":{""description"":""ok""}}}}}}")]
        [InlineData("https://api.example.com/swagger.json")]
        [InlineData("./test.json")]
        public void Should_Detect_Parseable_Input(string input)
        {
            // Create a temp file for file path test
            if (input.StartsWith("./"))
            {
                var tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, @"{""openapi"":""3.0.0"",""info"":{""title"":""API"",""version"":""1.0""},""paths"":{""/test"":{""get"":{""responses"":{""200"":{""description"":""ok""}}}}}}");
                input = tempFile;
                
                try
                {
                    // Act & Assert
                    var canParse = _parser.CanParse(input);
                    Assert.True(canParse);
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }
            else
            {
                // Act & Assert
                var canParse = _parser.CanParse(input);
                Assert.True(canParse);
            }
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("not-a-url")]
        [InlineData("./nonexistent.json")]
        public void Should_Detect_Non_Parseable_Input(string input)
        {
            // Act
            var canParse = _parser.CanParse(input);

            // Assert
            Assert.False(canParse);
        }

        [Fact]
        public async Task Should_Handle_Swagger_2_0_Format()
        {
            // Arrange
            var swagger2Json = @"{
                ""swagger"": ""2.0"",
                ""info"": {
                    ""title"": ""Swagger 2.0 API"",
                    ""version"": ""1.0.0""
                },
                ""paths"": {
                    ""/pets"": {
                        ""get"": {
                            ""operationId"": ""listPets"",
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""An array of pets""
                                }
                            }
                        }
                    }
                }
            }";

            // Act
            var document = await _parser.ParseAsync(swagger2Json);

            // Assert
            Assert.NotNull(document);
            Assert.Equal("2.0", document.Swagger);
            Assert.Equal("Swagger 2.0 API", document.Info.Title);
        }
    }
}