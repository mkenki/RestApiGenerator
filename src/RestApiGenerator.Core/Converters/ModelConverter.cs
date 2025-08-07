// src/RestApiGenerator.Core/Converters/ModelConverter.cs
using RestApiGenerator.Core.Models;
using System.Text.RegularExpressions;

namespace RestApiGenerator.Core.Converters
{
    public class ModelConverter
    {
        public CodeModel ConvertToCodeModel(SwaggerDocument swagger, RestApiGenerator.Core.Models.GeneratorConfig config)
        {
            var model = new CodeModel
            {
                Namespace = config.NamespaceName,
                ClientName = config.NamespaceName, // Set ClientName here
                BaseUrl = GetBaseUrl(swagger),
            };
            model.Authentication = config.Authentication;

            // Convert schemas to models
            model.Models = ConvertSchemas(swagger);

            // Convert paths to methods
            model.Methods = ConvertPaths(swagger, model.Models);

            return model;
        }

        private string GetBaseUrl(SwaggerDocument swagger)
        {
            if (swagger.Servers?.Any() == true)
            {
                return swagger.Servers.First().Url;
            }

            return string.Empty;
        }

        private List<ModelClass> ConvertSchemas(SwaggerDocument swagger)
        {
            var models = new List<ModelClass>();

            // Convert OpenAPI 3.0 schemas
            if (swagger.Components?.Schemas?.Any() == true)
            {
                foreach (var schema in swagger.Components.Schemas)
                {
                    var model = ConvertSchemaToModel(schema.Key, schema.Value);
                    if (model != null)
                        models.Add(model);
                }
            }

            // Convert Swagger 2.0 definitions
            if (swagger.Definitions?.Any() == true)
            {
                foreach (var definition in swagger.Definitions)
                {
                    var model = ConvertSchemaToModel(definition.Key, definition.Value);
                    if (model != null)
                        models.Add(model);
                }
            }

            return models;
        }

        private ModelClass? ConvertSchemaToModel(string name, SwaggerSchema schema)
        {
            // Handle oneOf
            if (schema.OneOf?.Any() == true)
            {
                return CreateOneOfModel(name, schema.OneOf);
            }

            // Handle anyOf
            if (schema.AnyOf?.Any() == true)
            {
                return CreateAnyOfModel(name, schema.AnyOf);
            }

            // Handle allOf
            if (schema.AllOf?.Any() == true)
            {
                return CreateAllOfModel(name, schema.AllOf);
            }

            // Existing object handling
            if (schema.Type != "object" || schema.Properties == null)
                return null;

            var model = new ModelClass
            {
                Name = ToPascalCase(name),
                Description = string.Empty
            };

            foreach (var property in schema.Properties)
            {
                var modelProperty = new ModelProperty
                {
                    Name = ToPascalCase(property.Key),
                    JsonPropertyName = property.Key,
                    Type = ConvertSchemaTypeToCSharp(property.Value),
                    IsRequired = schema.Required?.Contains(property.Key) == true,
                    Description = string.Empty
                };

                model.Properties.Add(modelProperty);
            }

            return model;
        }

        private List<ApiMethod> ConvertPaths(SwaggerDocument swagger, List<ModelClass> models)
        {
            var methods = new List<ApiMethod>();

            if (swagger.Paths == null)
                return methods;

            foreach (var path in swagger.Paths)
            {
                foreach (var operation in path.Value)
                {
                    var method = ConvertOperationToMethod(path.Key, operation.Key, operation.Value, models);
                    if (method != null)
                        methods.Add(method);
                }
            }

            return methods;
        }

        private ApiMethod? ConvertOperationToMethod(string path, string httpMethod, SwaggerOperation operation, List<ModelClass> models)
        {
            var methodName = GenerateMethodName(operation.OperationId, httpMethod, path);

            var method = new ApiMethod
            {
                Name = methodName,
                HttpMethod = httpMethod.ToUpperInvariant(),
                Path = path,
                Summary = operation.Summary ?? string.Empty,
                IsAsync = true
            };

            // Convert parameters
            if (operation.Parameters?.Any() == true)
            {
                foreach (var param in operation.Parameters)
                {
                    var apiParam = new ApiParameter
                    {
                        Name = param.Name,
                        Type = ConvertSchemaTypeToCSharp(param.Schema),
                        Location = param.In,
                        IsRequired = param.Required,
                        Description = param.Description ?? string.Empty
                    };

                    method.Parameters.Add(apiParam);
                }
            }

            // Convert request body
            if (operation.RequestBody?.Content?.Any() == true)
            {
                var content = operation.RequestBody.Content.First().Value;
                if (content.Schema != null)
                {
                    method.RequestBody = new ApiParameter
                    {
                        Name = "request",
                        Type = ConvertSchemaTypeToCSharp(content.Schema),
                        Location = "body",
                        IsRequired = operation.RequestBody.Required
                    };
                }
            }

            // Convert response type
            method.ResponseType = GetResponseType(operation.Responses, models);

            return method;
        }

        private string GenerateMethodName(string? operationId, string httpMethod, string path)
        {
            if (!string.IsNullOrEmpty(operationId))
            {
                return operationId;
            }

            // Generate method name from HTTP method and path
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Where(part => !part.StartsWith("{"))
                .Select(ToPascalCase);

            var methodPrefix = httpMethod.ToLowerInvariant() switch
            {
                "get" => "Get",
                "post" => "Create",
                "put" => "Update",
                "patch" => "Update",
                "delete" => "Delete",
                _ => ToPascalCase(httpMethod)
            };

            return methodPrefix + string.Join("", pathParts);
        }

        private string GetResponseType(Dictionary<string, SwaggerResponse>? responses, List<ModelClass> models)
        {
            if (responses == null || !responses.Any())
                return "object";

            // Look for successful response (200, 201, etc.)
            var successResponse = responses.FirstOrDefault(r => 
                r.Key.StartsWith("2") && r.Value.Content?.Any() == true);

            if (successResponse.Value?.Content?.Any() == true)
            {
                var content = successResponse.Value.Content.First().Value;
                if (content.Schema != null)
                {
                    return ConvertSchemaTypeToCSharp(content.Schema);
                }
            }

            return "object";
        }

        private string ConvertSchemaTypeToCSharp(SwaggerSchema? schema)
        {
            if (schema == null)
                return "object";

            // Handle $ref
            if (!string.IsNullOrEmpty(schema.Ref))
            {
                var refName = schema.Ref.Split('/').Last();
                return ToPascalCase(refName);
            }

            // Handle array types
            if (schema.Type == "array" && schema.Items != null)
            {
                var itemType = ConvertSchemaTypeToCSharp(schema.Items);
                return $"List<{itemType}>";
            }

            // Handle basic types
            return schema.Type switch
            {
                "string" => schema.Format switch
                {
                    "date" => "DateTime",
                    "date-time" => "DateTime",
                    "uuid" => "Guid",
                    _ => "string"
                },
                "integer" => schema.Format switch
                {
                    "int64" => "long",
                    _ => "int"
                },
                "number" => schema.Format switch
                {
                    "float" => "float",
                    "double" => "double",
                    _ => "decimal"
                },
                "boolean" => "bool",
                "object" => "object",
                _ => "object"
            };
        }

        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove special characters and split by common delimiters
            var words = Regex.Split(input, @"[^a-zA-Z0-9]+")
                .Where(w => !string.IsNullOrEmpty(w))
                .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1));

            return string.Join("", words);
        }

        private ModelEnum? ConvertSchemaToEnum(string name, SwaggerSchema schema)
        {
            if (schema.Enum?.Any() != true)
                return null;

            return new ModelEnum
            {
                Name = ToPascalCase(name),
                Values = schema.Enum.Select(v => v.ToString()!).Where(v => v != null).ToList(),
                Description = schema.Description
            };
        }

        private ModelClass CreateOneOfModel(string name, List<SwaggerSchema> schemas)
        {
            var model = new ModelClass
            {
                Name = ToPascalCase(name),
                Description = $"Represents a schema that can be one of the following types: {string.Join(", ", schemas.Select(s => s.Ref?.Split('/').Last() ?? s.Type))}",
                IsPolymorphic = true,
                DiscriminatorProperty = schemas.FirstOrDefault(s => s.Discriminator != null)?.Discriminator?.PropertyName
            };

            foreach (var subSchema in schemas)
            {
                var subTypeName = subSchema.Ref?.Split('/').Last();
                if (subTypeName != null)
                {
                    var subTypeModel = ConvertSchemaToModel(subTypeName, subSchema);
                    if (subTypeModel != null)
                    {
                        model.SubTypes.Add(subTypeModel);
                    }
                }
            }

            // For oneOf, we can add properties from all sub-schemas, but mark them as optional
            // This is a simplified approach; a more robust solution would involve common properties or interfaces.
            foreach (var subSchema in schemas)
            {
                if (subSchema.Properties != null)
                {
                    foreach (var property in subSchema.Properties)
                    {
                        if (!model.Properties.Any(p => p.Name == ToPascalCase(property.Key)))
                        {
                            var modelProperty = new ModelProperty
                            {
                                Name = ToPascalCase(property.Key),
                                JsonPropertyName = property.Key,
                                Type = ConvertSchemaTypeToCSharp(property.Value),
                                IsRequired = false, // Mark as optional for oneOf
                                Description = string.Empty
                            };
                            model.Properties.Add(modelProperty);
                        }
                    }
                }
            }
            return model;
        }

        private ModelClass CreateAnyOfModel(string name, List<SwaggerSchema> schemas)
        {
            var model = new ModelClass
            {
                Name = ToPascalCase(name),
                Description = $"Represents a schema that can be any of the following types: {string.Join(", ", schemas.Select(s => s.Ref?.Split('/').Last() ?? s.Type))}",
                IsPolymorphic = true,
                DiscriminatorProperty = schemas.FirstOrDefault(s => s.Discriminator != null)?.Discriminator?.PropertyName
            };

            foreach (var subSchema in schemas)
            {
                var subTypeName = subSchema.Ref?.Split('/').Last();
                if (subTypeName != null)
                {
                    var subTypeModel = ConvertSchemaToModel(subTypeName, subSchema);
                    if (subTypeModel != null)
                    {
                        model.SubTypes.Add(subTypeModel);
                    }
                }
            }

            // For anyOf, similar to oneOf, we can combine properties
            // This is a simplified approach; a more robust solution would involve common properties or interfaces.
            foreach (var subSchema in schemas)
            {
                if (subSchema.Properties != null)
                {
                    foreach (var property in subSchema.Properties)
                    {
                        if (!model.Properties.Any(p => p.Name == ToPascalCase(property.Key)))
                        {
                            var modelProperty = new ModelProperty
                            {
                                Name = ToPascalCase(property.Key),
                                JsonPropertyName = property.Key,
                                Type = ConvertSchemaTypeToCSharp(property.Value),
                                IsRequired = false, // Mark as optional for anyOf
                                Description = string.Empty
                            };
                            model.Properties.Add(modelProperty);
                        }
                    }
                }
            }
            return model;
        }

        private ModelClass CreateAllOfModel(string name, List<SwaggerSchema> schemas)
        {
            var model = new ModelClass
            {
                Name = ToPascalCase(name),
                Description = $"Represents a schema that is composed of all the following types: {string.Join(", ", schemas.Select(s => s.Ref?.Split('/').Last() ?? s.Type))}"
            };

            // For allOf, we combine properties, and required properties remain required
            foreach (var subSchema in schemas)
            {
                if (subSchema.Properties != null)
                {
                    foreach (var property in subSchema.Properties)
                    {
                        var modelProperty = new ModelProperty
                        {
                            Name = ToPascalCase(property.Key),
                            JsonPropertyName = property.Key,
                            Type = ConvertSchemaTypeToCSharp(property.Value),
                            IsRequired = subSchema.Required?.Contains(property.Key) == true, // Retain required status
                            Description = string.Empty
                        };
                        model.Properties.Add(modelProperty);
                    }
                }
            }
            return model;
        }
    }
}
