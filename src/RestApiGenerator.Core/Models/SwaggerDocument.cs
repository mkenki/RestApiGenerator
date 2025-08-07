// src/RestApiGenerator.Core/Models/SwaggerDocument.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RestApiGenerator.Core.Models
{
    public class SwaggerDocument
    {
        [JsonPropertyName("openapi")]
        public string OpenApi { get; set; } = string.Empty;
        
        [JsonPropertyName("swagger")]
        public string Swagger { get; set; } = string.Empty;
        
        [JsonPropertyName("info")]
        public SwaggerInfo Info { get; set; } = new();
        
        [JsonPropertyName("servers")]
        public List<SwaggerServer> Servers { get; set; } = new();
        
        [JsonPropertyName("paths")]
        public Dictionary<string, Dictionary<string, SwaggerOperation>> Paths { get; set; } = new();
        
        [JsonPropertyName("components")]
        public SwaggerComponents Components { get; set; } = new();
        
        [JsonPropertyName("definitions")]
        public Dictionary<string, SwaggerSchema> Definitions { get; set; } = new();
    }

    public class SwaggerInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class SwaggerServer
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class SwaggerOperation
    {
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
        
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;
        
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; } = string.Empty;
        
        [JsonPropertyName("parameters")]
        public List<SwaggerParameter> Parameters { get; set; } = new();
        
        [JsonPropertyName("requestBody")]
        public SwaggerRequestBody RequestBody { get; set; } = new();
        
        [JsonPropertyName("responses")]
        public Dictionary<string, SwaggerResponse> Responses { get; set; } = new();
        
        [JsonPropertyName("security")]
        public List<Dictionary<string, List<string>>> Security { get; set; } = new();
    }

    public class SwaggerParameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("in")]
        public string In { get; set; } = string.Empty; // query, path, header
        
        [JsonPropertyName("required")]
        public bool Required { get; set; }
        
        [JsonPropertyName("schema")]
        public SwaggerSchema? Schema { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class SwaggerRequestBody
    {
        [JsonPropertyName("content")]
        public Dictionary<string, SwaggerMediaType> Content { get; set; } = new();
        
        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }

    public class SwaggerResponse
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("content")]
        public Dictionary<string, SwaggerMediaType> Content { get; set; } = new();
    }

    public class SwaggerMediaType
    {
        [JsonPropertyName("schema")]
        public SwaggerSchema? Schema { get; set; }
    }


    public class SwaggerDiscriminator
    {
        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = string.Empty;

        [JsonPropertyName("mapping")]
        public Dictionary<string, string>? Mapping { get; set; }
    }

    public class SwaggerComponents
    {
        [JsonPropertyName("schemas")]
        public Dictionary<string, SwaggerSchema> Schemas { get; set; } = new();
        
        [JsonPropertyName("securitySchemes")]
        public Dictionary<string, SwaggerSecurityScheme> SecuritySchemes { get; set; } = new();
    }

    public class SwaggerSecurityScheme
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("scheme")]
        public string Scheme { get; set; } = string.Empty;
        
        [JsonPropertyName("bearerFormat")]
        public string BearerFormat { get; set; } = string.Empty;
        
        [JsonPropertyName("in")]
        public string In { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
