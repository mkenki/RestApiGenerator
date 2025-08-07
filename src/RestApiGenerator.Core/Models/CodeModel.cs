using System.Collections.Generic;

namespace RestApiGenerator.Core.Models
{
    public class CodeModel
    {
        public string Namespace { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public AuthenticationConfig Authentication { get; set; } = new();
        public List<ModelClass> Models { get; set; } = new();
        public List<ApiMethod> Methods { get; set; } = new();
    }

    public class ApiMethod
    {
        public string Name { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<ApiParameter> Parameters { get; set; } = new();
        public ApiParameter? RequestBody { get; set; }
        public string ResponseType { get; set; } = "object";
        public bool IsAsync { get; set; } = true;
    }

    public class ApiParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // path, query, header, body
        public bool IsRequired { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ModelClass
    {
        public string Name { get; set; } = string.Empty;
        public List<ModelProperty> Properties { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public bool IsPolymorphic { get; set; } = false;
        public string? DiscriminatorProperty { get; set; }
        public List<ModelClass> SubTypes { get; set; } = new();
    }

    public class ModelProperty
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string JsonPropertyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
