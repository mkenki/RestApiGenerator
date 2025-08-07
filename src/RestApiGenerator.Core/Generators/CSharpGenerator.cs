using RestApiGenerator.Core.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization; // Added for JsonConverter
using System.Text.RegularExpressions;
using System.Web;
using RestApiGenerator.Core.Generators.JsonConverters; // Added for PolymorphicConverter

namespace RestApiGenerator.Core.Generators
{
    public class CSharpGenerator : ICodeGenerator
    {
        public async Task<string> GenerateClientAsync(CodeModel model)
        {
            var sb = new StringBuilder();
            
            // Usings
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Text.Json;");
            sb.AppendLine("using System.Text.Json.Serialization;"); // Ensure this is present
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine($"using {model.Namespace}.Models;"); // Added for model types
            sb.AppendLine($"using {typeof(PolymorphicConverter<>).Namespace};"); // Added for PolymorphicConverter
            sb.AppendLine();
            
            // Namespace
            sb.AppendLine($"namespace {model.Namespace}");
            sb.AppendLine("{");
            
            // Client class
            sb.AppendLine($"    public class {model.ClientName} : I{model.ClientName}");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly HttpClient _httpClient;");
            sb.AppendLine("        private readonly JsonSerializerOptions _jsonOptions;");
            sb.AppendLine("        private readonly AuthenticationConfig _authenticationConfig;");
            sb.AppendLine("        private readonly string? _authenticationValue;");
            sb.AppendLine();
            
            // Constructor
            sb.AppendLine($"        public {model.ClientName}(HttpClient httpClient, AuthenticationConfig authenticationConfig, string? authenticationValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));");
            sb.AppendLine("            _authenticationConfig = authenticationConfig ?? throw new ArgumentNullException(nameof(authenticationConfig));");
            sb.AppendLine("            _authenticationValue = authenticationValue;");
            sb.AppendLine("            _jsonOptions = new JsonSerializerOptions");
            sb.AppendLine("            {");
            sb.AppendLine("                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,");
            sb.AppendLine("                WriteIndented = true");
            sb.AppendLine("            };");
            sb.AppendLine();
            sb.AppendLine("            if (_authenticationConfig.Type == AuthenticationType.Bearer)");
            sb.AppendLine("            {");
            sb.AppendLine("                _httpClient.DefaultRequestHeaders.Authorization = ");
            sb.AppendLine("                    new System.Net.Http.Headers.AuthenticationHeaderValue(\"Bearer\", _authenticationValue);");
            sb.AppendLine("            }");
            sb.AppendLine("            else if (_authenticationConfig.Type == AuthenticationType.ApiKey && ");
            sb.AppendLine("                     _authenticationConfig.Location == AuthenticationLocation.Header)");
            sb.AppendLine("            {");
            sb.AppendLine("                _httpClient.DefaultRequestHeaders.Add(_authenticationConfig.Name, _authenticationValue);");
            sb.AppendLine("            }");
            
            if (!string.IsNullOrEmpty(model.BaseUrl))
            {
                sb.AppendLine($"            ");
                sb.AppendLine($"            if (_httpClient.BaseAddress == null)");
                sb.AppendLine($"                _httpClient.BaseAddress = new Uri(\"{model.BaseUrl}\");");
            }
            
            sb.AppendLine("        }");
            sb.AppendLine();
            
            // Generate methods
            foreach (var method in model.Methods)
            {
                await GenerateMethod(sb, method);
                sb.AppendLine();
            }
            
            // Helper methods
            sb.AppendLine("        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (_authenticationConfig.Type == AuthenticationType.ApiKey && ");
            sb.AppendLine("                _authenticationConfig.Location == AuthenticationLocation.Query && ");
            sb.AppendLine("                !string.IsNullOrEmpty(_authenticationValue))");
            sb.AppendLine("            {");
            sb.AppendLine("                var uriBuilder = new UriBuilder(request.RequestUri);");
            sb.AppendLine("                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);");
            sb.AppendLine("                query[_authenticationConfig.Name] = _authenticationValue;");
            sb.AppendLine("                uriBuilder.Query = query.ToString();");
            sb.AppendLine("                request.RequestUri = uriBuilder.Uri;");
            sb.AppendLine("            }");
            sb.AppendLine("            ");
            sb.AppendLine("            var response = await _httpClient.SendAsync(request, cancellationToken);");
            sb.AppendLine("            response.EnsureSuccessStatusCode();");
            sb.AppendLine("            ");
            sb.AppendLine("            var content = await response.Content.ReadAsStringAsync();");
            sb.AppendLine("            ");
            sb.AppendLine("            if (string.IsNullOrEmpty(content))");
            sb.AppendLine("                return default(T);");
            sb.AppendLine("                ");
            sb.AppendLine("            return JsonSerializer.Deserialize<T>(content, _jsonOptions);");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            sb.AppendLine("        private StringContent CreateJsonContent(object obj)");
            sb.AppendLine("        {");
            sb.AppendLine("            var json = JsonSerializer.Serialize(obj, _jsonOptions);");
            sb.AppendLine("            return new StringContent(json, Encoding.UTF8, \"application/json\");");
            sb.AppendLine("        }");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public async Task<string> GenerateModelsAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();
            
            // Usings
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text.Json.Serialization;");
            sb.AppendLine();
            
            // Namespace
            sb.AppendLine($"namespace {model.Namespace}.Models");
            sb.AppendLine("{");
            
            foreach (var modelClass in model.Models)
            {
                if (modelClass.IsPolymorphic)
                {
                    // Generate interface for polymorphic base type
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.AppendLine("    /// <summary>");
                        sb.AppendLine($"    /// {modelClass.Description}");
                        sb.AppendLine("    /// </summary>");
                    }
                    sb.AppendLine($"    public interface I{modelClass.Name}");
                    sb.AppendLine("    {");
                    foreach (var property in modelClass.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            sb.AppendLine("        /// <summary>");
                            sb.AppendLine($"        /// {property.Description}");
                            sb.AppendLine("        /// </summary>");
                        }
                        sb.AppendLine($"        {property.Type}{(property.IsRequired ? "" : "?")} {property.Name} {{ get; set; }}");
                        sb.AppendLine();
                    }
                    sb.AppendLine("    }");
                    sb.AppendLine();

                    // Generate concrete classes for each subtype
                    foreach (var subType in modelClass.SubTypes)
                    {
                        if (!string.IsNullOrEmpty(subType.Description))
                        {
                            sb.AppendLine("    /// <summary>");
                            sb.AppendLine($"    /// {subType.Description}");
                            sb.AppendLine("    /// </summary>");
                        }
                        sb.AppendLine($"    public class {subType.Name} : I{modelClass.Name}");
                        sb.AppendLine("    {");
                        foreach (var property in subType.Properties)
                        {
                            if (!string.IsNullOrEmpty(property.Description))
                            {
                                sb.AppendLine("        /// <summary>");
                                sb.AppendLine($"        /// {property.Description}");
                                sb.AppendLine("        /// </summary>");
                            }
                            if (!string.IsNullOrEmpty(property.JsonPropertyName))
                            {
                                sb.AppendLine($"        [JsonPropertyName(\"{property.JsonPropertyName}\")]");
                            }
                            var nullabilityOperator = property.IsRequired ? "" : "?";
                            sb.AppendLine($"        public {property.Type}{nullabilityOperator} {property.Name} {{ get; set; }}");
                            sb.AppendLine();
                        }
                        sb.AppendLine("    }");
                        sb.AppendLine();
                    }
                }
                else
                {
                    // Existing class generation logic for non-polymorphic models
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.AppendLine("    /// <summary>");
                        sb.AppendLine($"    /// {modelClass.Description}");
                        sb.AppendLine("    /// </summary>");
                    }
                    
                    sb.AppendLine($"    public class {modelClass.Name}");
                    sb.AppendLine("    {");
                    
                    foreach (var property in modelClass.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            sb.AppendLine("        /// <summary>");
                            sb.AppendLine($"        /// {property.Description}");
                            sb.AppendLine("        /// </summary>");
                        }
                        
                        if (!string.IsNullOrEmpty(property.JsonPropertyName))
                        {
                            sb.AppendLine($"        [JsonPropertyName(\"{property.JsonPropertyName}\")]");
                        }
                        
                        var nullabilityOperator = property.IsRequired ? "" : "?";
                        sb.AppendLine($"        public {property.Type}{nullabilityOperator} {property.Name} {{ get; set; }}");
                        sb.AppendLine();
                    }
                    
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
            }
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public async Task<string> GenerateInterfaceAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();
            
            // Usings
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine($"using {model.Namespace}.Models;");
            sb.AppendLine();
            
            // Namespace
            sb.AppendLine($"namespace {model.Namespace}");
            sb.AppendLine("{");
            
            // Interface
            sb.AppendLine($"    public interface I{model.ClientName}");
            sb.AppendLine("    {");
            
            foreach (var method in model.Methods)
            {
                var parameters = new List<string>();
                foreach (var p in method.Parameters)
                {
                    parameters.Add($"{p.Type}{(p.IsRequired ? "" : "?")} {p.Name}");
                }
                
                if (method.RequestBody != null)
                {
                    parameters.Add($"{method.RequestBody.Type} {method.RequestBody.Name}");
                }

                parameters.Add("CancellationToken cancellationToken = default");
                
                var parameterString = string.Join(", ", parameters);
                
                var returnType = method.IsAsync ? $"Task<{method.ResponseType}>" : method.ResponseType;
                
                if (!string.IsNullOrEmpty(method.Summary))
                {
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine($"        /// {method.Summary}");
                    sb.AppendLine("        /// </summary>");
                }
                
                sb.AppendLine($"        {returnType} {ToPascalCase(method.Name)}({parameterString});");
                sb.AppendLine();
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }

        public async Task<Dictionary<string, string>> GenerateAllAsync(CodeModel model)
        {
            var result = new Dictionary<string, string>();
            
            result[$"I{model.ClientName}.cs"] = await GenerateInterfaceAsync(model).ConfigureAwait(false);
            result[$"{model.ClientName}.cs"] = await GenerateClientAsync(model).ConfigureAwait(false);
            result["Models.cs"] = await GenerateModelsAsync(model).ConfigureAwait(false);
            
            return result;
        }

        private async Task GenerateMethod(StringBuilder sb, ApiMethod method)
        {
            await Task.Yield();
            // Method summary
            if (!string.IsNullOrEmpty(method.Summary))
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {method.Summary}");
                sb.AppendLine("        /// </summary>");
            }
            
            // Method signature
            var parameters = string.Join(", ", method.Parameters.Select(p => 
                $"{p.Type}{(p.IsRequired ? "" : "?")} {p.Name}"));
            
            if (method.RequestBody != null)
            {
                if (!string.IsNullOrEmpty(parameters))
                    parameters += ", ";
                parameters += $"{method.RequestBody.Type} {method.RequestBody.Name}";
            }
            
            var returnType = method.IsAsync ? $"Task<{method.ResponseType}>" : method.ResponseType;
            var asyncKeyword = method.IsAsync ? "async " : "";

            sb.AppendLine($"        public {asyncKeyword}{returnType} {ToPascalCase(method.Name)}({parameters})");
            sb.AppendLine("        {");
            
            // Build URL with path parameters
            var url = method.Path;
            var pathParams = method.Parameters.Where(p => p.Location == "path").ToList();
            
            foreach (var param in pathParams)
            {
                url = url.Replace($"{{{param.Name}}}", $"\" + {param.Name} + \"");
            }
            
            // Add query parameters
            var queryParams = method.Parameters.Where(p => p.Location == "query").ToList();
            if (queryParams.Any())
            {
                sb.AppendLine("            var queryParams = new List<string>();");
                
                foreach (var param in queryParams)
                {
                    var paramName = param.Name;
                    if (param.IsRequired)
                    {
                        sb.AppendLine($"            queryParams.Add($\"{param.Name}={{{paramName}}}\");");
                    }
                    else
                    {
                        sb.AppendLine($"            if ({paramName} != null)");
                        sb.AppendLine($"                queryParams.Add($\"{param.Name}={{{paramName}}}\");");
                    }
                }
                
                sb.AppendLine("            ");
                sb.AppendLine("            var queryString = queryParams.Any() ? \"?\" + string.Join(\"&\", queryParams) : \"\";");
                url += "\" + queryString + \"";
            }
            
            sb.AppendLine($"            var url = \"{url}\";");
            sb.AppendLine();
            
            // Create request
            sb.AppendLine($"            var request = new HttpRequestMessage(HttpMethod.{ToPascalCase(method.HttpMethod)}, url);");
            
            // Add headers
            var headerParams = method.Parameters.Where(p => p.Location == "header").ToList();
            foreach (var param in headerParams)
            {
                var paramName = param.Name;
                if (param.IsRequired)
                {
                    sb.AppendLine($"            request.Headers.Add(\"{param.Name}\", {paramName}.ToString());");
                }
                else
                {
                    sb.AppendLine($"            if ({paramName} != null)");
                    sb.AppendLine($"                request.Headers.Add(\"{param.Name}\", {paramName}.ToString());");
                }
            }
            
            // Add request body
            if (method.RequestBody != null)
            {
                var bodyParamName = method.RequestBody.Name;
                sb.AppendLine($"            request.Content = CreateJsonContent({bodyParamName});");
            }
            
            sb.AppendLine();
            
            // Send request and return response
            if (method.ResponseType == "void")
            {
                sb.AppendLine("            var response = await _httpClient.SendAsync(request, cancellationToken);");
                sb.AppendLine("            response.EnsureSuccessStatusCode();");
            }
            else
            {
                sb.AppendLine($"            return await SendRequestAsync<{method.ResponseType}>(request, cancellationToken);");
            }
            
            sb.AppendLine("        }");
        }

        private static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
                
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }
    }
}
