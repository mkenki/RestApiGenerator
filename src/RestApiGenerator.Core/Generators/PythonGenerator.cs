using RestApiGenerator.Core.Models;
using System.Text;
using System.Text.Json;

namespace RestApiGenerator.Core.Generators
{
    public class PythonGenerator : ICodeGenerator
    {
        public async Task<string> GenerateClientAsync(CodeModel model)
        {
            var sb = new StringBuilder();

            // Imports and documentation
            sb.AppendLine("\"\"\"");
            sb.AppendLine("Auto-generated REST API client");
            sb.AppendLine("Generated from OpenAPI specification");
            sb.AppendLine("\"\"\"");
            sb.AppendLine();
            sb.AppendLine("import asyncio");
            sb.AppendLine("import aiohttp");
            sb.AppendLine("from typing import Dict, List, Optional, Any, Union");
            sb.AppendLine("import json");
            sb.AppendLine("from . import models");
            sb.AppendLine();

            // Authentication enum
            sb.AppendLine("class AuthenticationType:");
            sb.AppendLine("    NONE = \"none\"");
            sb.AppendLine("    BEARER = \"bearer\"");
            sb.AppendLine("    API_KEY = \"api_key\"");
            sb.AppendLine();

            sb.AppendLine("class AuthenticationLocation:");
            sb.AppendLine("    NONE = \"none\"");
            sb.AppendLine("    HEADER = \"header\"");
            sb.AppendLine("    QUERY = \"query\"");
            sb.AppendLine();

            sb.AppendLine("class AuthenticationConfig:");
            sb.AppendLine("    def __init__(self, auth_type: str = AuthenticationType.NONE,");
            sb.AppendLine("                 location: str = AuthenticationLocation.NONE,");
            sb.AppendLine("                 name: Optional[str] = None):");
            sb.AppendLine("        self.type = auth_type");
            sb.AppendLine("        self.location = location");
            sb.AppendLine("        self.name = name");
            sb.AppendLine();

            // Main client class
            sb.Append("class ");
            sb.Append(model.ClientName);
            sb.AppendLine(":");
            sb.AppendLine("    \"\"\"");
            sb.Append("    Auto-generated REST API client for ");
            sb.Append(model.ClientName);
            sb.AppendLine();
            sb.AppendLine("    \"\"\"");
            sb.AppendLine();
            sb.AppendLine("    def __init__(self, auth_config: Optional[AuthenticationConfig] = None,");
            sb.AppendLine("                 auth_value: Optional[str] = None,");
            sb.AppendLine("                 base_url: Optional[str] = None):");
            sb.AppendLine("        self.auth_config = auth_config or AuthenticationConfig()");
            sb.AppendLine("        self.auth_value = auth_value");
            sb.AppendLine("        self.base_url = base_url");
            sb.AppendLine("        self.session: Optional[aiohttp.ClientSession] = None");
            sb.AppendLine();
            sb.AppendLine("    async def __aenter__(self):");
            sb.AppendLine("        headers = {'Content-Type': 'application/json'}");
            sb.AppendLine();
            sb.AppendLine("        # Setup authentication");
            sb.AppendLine("        if self.auth_config.type == AuthenticationType.BEARER and self.auth_value:");
            sb.AppendLine("            headers['Authorization'] = f'Bearer {self.auth_value}'");
            sb.AppendLine();
            sb.AppendLine("        self.session = aiohttp.ClientSession(headers=headers)");
            sb.AppendLine("        return self");
            sb.AppendLine();
            sb.AppendLine("    async def __aexit__(self, exc_type, exc_val, exc_tb):");
            sb.AppendLine("        if self.session:");
            sb.AppendLine("            await self.session.close()");
            sb.AppendLine();

            // Generate methods
            foreach (var method in model.Methods)
            {
                await GenerateMethod(sb, method);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GenerateModelsAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();

            sb.AppendLine("\"\"\"");
            sb.AppendLine("Auto-generated models");
            sb.AppendLine("Generated from OpenAPI specification");
            sb.AppendLine("\"\"\"");
            sb.AppendLine();
            sb.AppendLine("from typing import Dict, List, Optional, Any, Union");
            sb.AppendLine("from datetime import datetime");
            sb.AppendLine();

            foreach (var modelClass in model.Models)
            {
                if (modelClass.IsPolymorphic)
                {
                    // Generate base class for polymorphic types
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.Append("\"\"\"");
                        sb.AppendLine(modelClass.Description);
                        sb.AppendLine("\"\"\"");
                    }

                    sb.Append("class ");
                    sb.Append(modelClass.Name);
                    sb.AppendLine(":");
                    sb.AppendLine("    \"\"\"Base class for polymorphic type\"\"\"");
                    sb.AppendLine("    pass");
                    sb.AppendLine();

                    // Generate concrete classes for subtypes
                    foreach (var subType in modelClass.SubTypes)
                    {
                        if (!string.IsNullOrEmpty(subType.Description))
                        {
                            sb.Append("\"\"\"");
                            sb.AppendLine(subType.Description);
                            sb.AppendLine("\"\"\"");
                        }

                        sb.Append("class ");
                        sb.Append(subType.Name);
                        sb.Append("(");
                        sb.Append(modelClass.Name);
                        sb.AppendLine("):");

                        sb.AppendLine("    \"\"\"");
                        sb.Append(subType.Name);
                        sb.AppendLine(" model implementation");
                        sb.AppendLine("    \"\"\"");
                        sb.AppendLine();
                        sb.AppendLine("    def __init__(self,");
                        foreach (var property in subType.Properties)
                        {
                            sb.Append("                 ");
                            sb.Append(ToSnakeCase(property.Name));
                            if (!property.IsRequired)
                            {
                                sb.Append(": Optional[");
                                sb.Append(ConvertToPythonType(property.Type));
                                sb.Append("]");
                            }
                            else
                            {
                                sb.Append(": ");
                                sb.Append(ConvertToPythonType(property.Type));
                            }
                            sb.AppendLine(" = None,");
                        }

                        sb.AppendLine("                 **kwargs):");
                        sb.AppendLine("        super().__init__(**kwargs)");

                        foreach (var property in subType.Properties)
                        {
                            sb.Append("        self.");
                            sb.Append(ToSnakeCase(property.Name));
                            sb.Append(" = ");
                            sb.Append(ToSnakeCase(property.Name));
                            sb.AppendLine();
                        }
                        sb.AppendLine();
                    }
                }
                else
                {
                    // Regular class generation
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.Append("\"\"\"");
                        sb.AppendLine(modelClass.Description);
                        sb.AppendLine("\"\"\"");
                    }

                    sb.Append("class ");
                    sb.Append(modelClass.Name);
                    sb.AppendLine(":");

                    sb.AppendLine("    \"\"\"");
                    sb.Append(modelClass.Name);
                    sb.AppendLine(" model");
                    sb.AppendLine("    \"\"\"");
                    sb.AppendLine();
                    sb.AppendLine("    def __init__(self,");

                    foreach (var property in modelClass.Properties)
                    {
                        sb.Append("                 ");
                        sb.Append(ToSnakeCase(property.Name));
                        if (!property.IsRequired)
                        {
                            sb.Append(": Optional[");
                            sb.Append(ConvertToPythonType(property.Type));
                            sb.Append("]");
                        }
                        else
                        {
                            sb.Append(": ");
                            sb.Append(ConvertToPythonType(property.Type));
                        }
                        sb.AppendLine(" = None,");
                    }

                    sb.AppendLine("                 **kwargs):");
                    foreach (var property in modelClass.Properties)
                    {
                        sb.Append("        self.");
                        sb.Append(ToSnakeCase(property.Name));
                        sb.Append(" = ");
                        sb.Append(ToSnakeCase(property.Name));
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public async Task<string> GenerateInterfaceAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();

            sb.AppendLine("\"\"\"");
            sb.AppendLine("Type hints and interfaces for the API client");
            sb.AppendLine("\"\"\"");
            sb.AppendLine();
            sb.AppendLine("import asyncio");
            sb.AppendLine("from typing import Protocol, Dict, List, Optional, Any");
            sb.AppendLine("from . import models");
            sb.AppendLine();

            sb.Append("class ");
            sb.Append(model.ClientName);
            sb.AppendLine("Interface(Protocol):");

            foreach (var method in model.Methods)
            {
                var parameters = new List<string>();

                foreach (var p in method.Parameters)
                {
                    parameters.Add(ToSnakeCase(p.Name) + ": " +
                        (p.IsRequired ? "" : "Optional[") +
                        ConvertToPythonType(p.Type) +
                        (p.IsRequired ? "" : "]"));
                }

                if (method.RequestBody != null)
                {
                    parameters.Add("data: " + ConvertToPythonType(method.RequestBody.Type));
                }

                if (!string.IsNullOrEmpty(method.Summary))
                {
                    sb.Append("    \"\"\"");
                    sb.AppendLine(method.Summary);
                    sb.AppendLine("    \"\"\"");
                }

                var parameterString = string.Join(", ", parameters);
                var responseType = method.ResponseType == "void" ? "None" :
                    ConvertToPythonType(method.ResponseType);
                var methodName = ToSnakeCase(method.Name);

                sb.Append("    async def ");
                sb.Append(methodName);
                sb.Append("(");
                sb.Append(parameterString);
                sb.Append(") -> ");
                sb.Append(responseType);
                sb.AppendLine(": ...");
                sb.AppendLine();
            }

            sb.AppendLine();

            return sb.ToString();
        }

        public async Task<Dictionary<string, string>> GenerateAllAsync(CodeModel model)
        {
            var result = new Dictionary<string, string>();

            result["__init__.py"] = GenerateInitFile();
            result[model.ClientName.ToSnakeCase() + ".py"] = await GenerateClientAsync(model).ConfigureAwait(false);
            result["models.py"] = await GenerateModelsAsync(model).ConfigureAwait(false);
            result["types.py"] = await GenerateInterfaceAsync(model).ConfigureAwait(false);
            result["setup.py"] = GenerateSetupPy(model);
            result["requirements.txt"] = GenerateRequirementsTxt();

            return result;
        }

        private async Task GenerateMethod(StringBuilder sb, ApiMethod method)
        {
            await Task.Yield();

            // Method documentation
            if (!string.IsNullOrEmpty(method.Summary))
            {
                sb.Append("    \"\"\"");
                sb.AppendLine(method.Summary);
                sb.AppendLine("    \"\"\"");
            }

            var parameters = new List<string>();
            var pathParams = new List<string>();
            var queryParams = new List<string>();
            var headerParams = new List<string>();

            foreach (var param in method.Parameters)
            {
                var paramName = ToSnakeCase(param.Name);
                parameters.Add(paramName + ": " +
                    (param.IsRequired ? "" : "Optional[") +
                    ConvertToPythonType(param.Type) +
                    (param.IsRequired ? "" : "]"));

                switch (param.Location)
                {
                    case "path":
                        pathParams.Add(paramName);
                        break;
                    case "query":
                        queryParams.Add(param.Name + "=" + paramName);
                        break;
                    case "header":
                        headerParams.Add(("'" + param.Name + "', " + paramName));
                        break;
                }
            }

            if (method.RequestBody != null)
            {
                parameters.Add("data: " + ConvertToPythonType(method.RequestBody.Type));
            }

            var parameterString = string.Join(", ", parameters);
            var responseType = method.ResponseType == "void" ? "None" :
                ConvertToPythonType(method.ResponseType);
            var methodName = ToSnakeCase(method.Name);

            sb.Append("    async def ");
            sb.Append(methodName);
            sb.Append("(");
            sb.Append(parameterString);
            sb.Append(") -> ");
            sb.Append(responseType);
            sb.AppendLine(":");

            // Method implementation
            sb.AppendLine("        if not self.session:");
            sb.AppendLine("            raise RuntimeError(\"Client session not initialized. Use 'async with' context manager.\")");
            sb.AppendLine();

            // Build URL with path parameters
            var url = method.Path;
            if (!string.IsNullOrEmpty(url))
            {
                sb.Append("        url = f\"");
                sb.Append(url);
                sb.AppendLine("\"");

                // Format path parameters
                if (pathParams.Any())
                {
                    foreach (var param in pathParams)
                    {
                        var queryParam = method.Parameters.First(p => ToSnakeCase(p.Name) == param);
                        var formatStr = "        url = url.format(" + ToSnakeCase(queryParam.Name) + "=" + param + ")";
                        sb.AppendLine(formatStr);
                    }
                }
            }
            else
            {
                sb.AppendLine("        url = \"\"");
            }

            sb.AppendLine();

            // Handle query parameters and API key authentication
            if (queryParams.Any())
            {
                sb.AppendLine("        params = {}");
                foreach (var param in queryParams.Skip(1))  // Skip the first =
                {
                    sb.Append("        params['");
                    sb.Append(param);
                    sb.AppendLine("'] = locals().get('" + param.Split('=')[0] + "')");
                }
                sb.AppendLine();

                // Handle API key in query
                sb.AppendLine("        # Handle API key authentication in query");
                sb.AppendLine("        if (self.auth_config.type == AuthenticationType.API_KEY and");
                sb.AppendLine("            self.auth_config.location == AuthenticationLocation.QUERY and");
                sb.AppendLine("            self.auth_value):");
                sb.Append("            params[self.auth_config.name or 'apiKey'] = self.auth_value");
                sb.AppendLine();
                sb.AppendLine("        if params:");
                sb.AppendLine("            query_string = '&'.join([f'{k}={v}' for k, v in params.items()])");
                sb.AppendLine("            url += '?' + query_string");
            }

            sb.AppendLine();

            // Prepare request data
            if (method.RequestBody != null)
            {
                sb.AppendLine("        request_data = json.dumps(data.__dict__ if hasattr(data, '__dict__') else data)");
            }

            // Execute request
            sb.Append("        async with self.session.");
            sb.Append(method.HttpMethod.ToLower());
            sb.Append("(url");

            if (method.RequestBody != null)
            {
                sb.Append(", data=request_data");
            }

            sb.AppendLine(") as response:");
            sb.AppendLine("            if response.status >= 400:");
            sb.AppendLine("                raise Exception(f'Request failed with status {response.status}')");
            sb.AppendLine();
            sb.AppendLine("            if response.status == 204:  # No Content");
            sb.AppendLine("                return None");
            sb.AppendLine();
            sb.AppendLine("            response_data = await response.json()");
            sb.AppendLine("            return response_data");
        }

        private static string ConvertToPythonType(string csharpType)
        {
            return csharpType.ToLower() switch
            {
                "string" => "str",
                "int" => "int",
                "long" => "int",
                "double" => "float",
                "float" => "float",
                "bool" or "boolean" => "bool",
                "datetime" => "datetime",
                "date" => "datetime",
                "guid" => "str",
                var list when list.Contains("list<") || list.Contains("ienumerable<") =>
                    "List[" + list.Replace("List<", "").Replace("IEnumerable<", "").Replace(">", "") + "]",
                var dict when dict.Contains("dictionary<") || dict.Contains("map<") =>
                    "Dict[str, Any]",
                _ => csharpType
            };
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0 && char.IsUpper(input[i]))
                {
                    result.Append('_');
                }
                result.Append(char.ToLower(input[i]));
            }
            return result.ToString();
        }

        private string GenerateInitFile()
        {
            return "\"\"\"\nREST API Generator - Auto-generated Python client\n\"\"\"\n";
        }

        private string GenerateSetupPy(CodeModel model)
        {
            return $@"{{
    ""name"": ""{model.Namespace?.ToLower().Replace(".", "-")}-api-client"",
    ""version"": ""1.0.0"",
    ""description"": ""Auto-generated REST API client"",
    ""packages"": [""{model.Namespace?.ToLower().Replace(".", "_")}_api""],
    ""python_requires"": "">=3.7"",
}}";
        }

        private string GenerateRequirementsTxt()
        {
            return "aiohttp==3.9.1\n";
        }
    }
}

// Extension method for string to snake_case conversion
public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i]))
            {
                result.Append('_');
            }
            result.Append(char.ToLower(input[i]));
        }
        return result.ToString();
    }
}
