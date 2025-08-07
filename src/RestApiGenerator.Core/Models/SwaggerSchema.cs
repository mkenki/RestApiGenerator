using System.Text.Json.Serialization;

namespace RestApiGenerator.Core.Models
{
    public class SwaggerSchema
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("$ref")]
        public string? Ref { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, SwaggerSchema>? Properties { get; set; }

        [JsonPropertyName("items")]
        public SwaggerSchema? Items { get; set; }

        [JsonPropertyName("required")]
        public List<string>? Required { get; set; }

        [JsonPropertyName("enum")]
        public List<object>? Enum { get; set; }

        // Add new properties for complex schemas
        [JsonPropertyName("oneOf")]
        public List<SwaggerSchema>? OneOf { get; set; }

        [JsonPropertyName("anyOf")]
        public List<SwaggerSchema>? AnyOf { get; set; }

        [JsonPropertyName("allOf")]
        public List<SwaggerSchema>? AllOf { get; set; }

        [JsonPropertyName("discriminator")]
        public DiscriminatorObject? Discriminator { get; set; }
    }

    public class DiscriminatorObject
    {
        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = string.Empty;

        [JsonPropertyName("mapping")]
        public Dictionary<string, string>? Mapping { get; set; }
    }
}
