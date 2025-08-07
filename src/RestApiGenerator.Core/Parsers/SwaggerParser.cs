using RestApiGenerator.Core.Models;
using System.Text;
using System.Text.Json;

namespace RestApiGenerator.Core.Parsers
{
    public class SwaggerParser : IApiParser
    {
        private readonly HttpClient _httpClient;

        public SwaggerParser(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<SwaggerDocument> ParseAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));

            try
            {
                // Try to parse as JSON first
                if (IsJson(input))
                {
                    return ParseJson(input);
                }

                // Check if it's a URL
                if (IsUrl(input))
                {
                    return await ParseFromUrlAsync(input);
                }

                // Check if it's a file path
                if (File.Exists(input))
                {
                    return await ParseFromFileAsync(input);
                }

                throw new ArgumentException("Input is not a valid JSON, URL, or file path", nameof(input));
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse Swagger document: {ex.Message}", ex);
            }
        }

        public async Task<SwaggerDocument> ParseFromUrlAsync(string url)
        {
            if (!IsUrl(url))
                throw new ArgumentException("Invalid URL format", nameof(url));

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return ParseJson(content);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch Swagger document from URL: {ex.Message}", ex);
            }
        }

        public async Task<SwaggerDocument> ParseFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            try
            {
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                return ParseJson(content);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"Failed to read file: {ex.Message}", ex);
            }
        }

        public bool CanParse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return IsJson(input) || IsUrl(input) || File.Exists(input);
        }

        private SwaggerDocument ParseJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var document = JsonSerializer.Deserialize<SwaggerDocument>(json, options);
            
            if (document == null)
                throw new InvalidOperationException("Failed to deserialize Swagger document");

            ValidateDocument(document);
            return document;
        }

        private void ValidateDocument(SwaggerDocument document)
        {
            var errors = new List<string>();

            // Check if it's a valid Swagger/OpenAPI document
            if (string.IsNullOrEmpty(document.OpenApi) && string.IsNullOrEmpty(document.Swagger))
            {
                errors.Add("Document must specify either 'openapi' or 'swagger' version");
            }

            // Check required info section
            if (document.Info == null || string.IsNullOrEmpty(document.Info.Title))
            {
                errors.Add("Document must have an 'info' section with a title");
            }

            // Check if there are any paths
            if (document.Paths == null || !document.Paths.Any())
            {
                errors.Add("Document must have at least one path defined");
            }

            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Swagger document: {string.Join(", ", errors)}");
            }
        }

        private static bool IsJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}")) ||
                   (input.StartsWith("[") && input.EndsWith("]"));
        }

        private static bool IsUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
