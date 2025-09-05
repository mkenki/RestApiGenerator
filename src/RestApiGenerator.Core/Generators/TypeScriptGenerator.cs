using RestApiGenerator.Core.Models;
using System.Text;
using System.Text.Json;

namespace RestApiGenerator.Core.Generators
{
    public class TypeScriptGenerator : ICodeGenerator
    {
        public async Task<string> GenerateClientAsync(CodeModel model)
        {
            var sb = new StringBuilder();

            // Imports
            sb.AppendLine("// Auto-generated REST API client");
            sb.AppendLine("// Generated from OpenAPI specification");
            sb.AppendLine("");
            sb.AppendLine("import axios, { AxiosInstance, AxiosResponse } from 'axios';");
            if (model.Models.Any())
            {
                sb.Append("import { ");
                sb.Append(string.Join(", ", model.Models.Select(m => m.Name)));
                sb.AppendLine(" } from './models';");
            }
            sb.AppendLine("");

            // Enums for authentication
            sb.AppendLine("export enum AuthenticationType {");
            sb.AppendLine("  NONE = 'none',");
            sb.AppendLine("  API_KEY = 'apiKey',");
            sb.AppendLine("  BEARER = 'bearer',");
            sb.AppendLine("  OAUTH2_AUTHORIZATION_CODE = 'oauth2AuthorizationCode',");
            sb.AppendLine("  OAUTH2_CLIENT_CREDENTIALS = 'oauth2ClientCredentials',");
            sb.AppendLine("  OAUTH2_PASSWORD = 'oauth2Password'");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("export enum AuthenticationLocation {");
            sb.AppendLine("  HEADER = 'header',");
            sb.AppendLine("  QUERY = 'query'");
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("export interface AuthenticationConfig {");
            sb.AppendLine("  type: AuthenticationType;");
            sb.AppendLine("  location?: AuthenticationLocation;");
            sb.AppendLine("  name?: string;");
            sb.AppendLine("}");
            sb.AppendLine("");

            // Main client class
            sb.Append("export class ");
            sb.Append(model.ClientName);
            sb.AppendLine(" {");
            sb.AppendLine("  private axiosInstance: AxiosInstance;");
            sb.AppendLine("  private authConfig: AuthenticationConfig;");
            sb.AppendLine("  private authValue?: string;");
            sb.AppendLine();
            sb.AppendLine("  constructor(authConfig: AuthenticationConfig, authValue?: string, baseURL?: string) {");
            sb.AppendLine("    this.authConfig = authConfig;");
            sb.AppendLine("    this.authValue = authValue;");
            sb.AppendLine();
            sb.AppendLine("    this.axiosInstance = axios.create({");
            sb.Append("      baseURL: baseURL || '");
            sb.Append(model.BaseUrl ?? "");
            sb.AppendLine("',");
            sb.AppendLine("      headers: {");
            sb.AppendLine("        'Content-Type': 'application/json',");
            sb.AppendLine("      },");
            sb.AppendLine("    });");
            sb.AppendLine();
            sb.AppendLine("    // Setup authentication");
            sb.AppendLine("    if (AuthenticationType.BEARER === authConfig.type && authValue) {");
            sb.AppendLine("      this.axiosInstance.defaults.headers.common['Authorization'] = 'Bearer ' + authValue;");
            sb.AppendLine("    } else if (AuthenticationType.OAUTH2_AGENT_CODE === authConfig.type && authValue ||");
            sb.AppendLine("               AuthenticationType.OAUTH2_CLIENT_CREDENTIALS === authConfig.type && authValue ||");
            sb.AppendLine("               AuthenticationType.OAUTH2_PASSWORD === authConfig.type && authValue) {");
            sb.AppendLine("      this.axiosInstance.defaults.headers.common['Authorization'] = 'Bearer ' + authValue;");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine();

            // Generate methods
            foreach (var method in model.Methods)
            {
                await GenerateMethod(sb, method, model.Namespace);
                sb.AppendLine();
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        public async Task<string> GenerateModelsAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();

            // Generate interfaces/models
            foreach (var modelClass in model.Models)
            {
                if (modelClass.IsPolymorphic)
                {
                    // Generate base interface for polymorphic types
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.AppendLine("/**");
                        sb.AppendLine($" * {modelClass.Description}");
                        sb.AppendLine(" */");
                    }
                    sb.AppendLine($"export interface {modelClass.Name} {{");

                    foreach (var property in modelClass.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            sb.AppendLine("  /**");
                            sb.AppendLine($"   * {property.Description}");
                            sb.AppendLine("   */");
                        }
                        sb.Append("  ");
                        sb.Append(ToCamelCase(property.Name));
                        if (!property.IsRequired)
                        {
                            sb.Append("?");
                        }
                        sb.Append(": ");
                        sb.Append(ConvertToTypeScriptType(property.Type));
                        sb.AppendLine(";");
                    }

                    sb.AppendLine("}");
                    sb.AppendLine();

                    // Generate concrete types for subtypes
                    foreach (var subType in modelClass.SubTypes)
                    {
                        if (!string.IsNullOrEmpty(subType.Description))
                        {
                            sb.AppendLine("/**");
                            sb.AppendLine($" * {subType.Description}");
                            sb.AppendLine(" */");
                        }
                        sb.AppendLine($"export interface {subType.Name} extends {modelClass.Name} {{");

                        foreach (var property in subType.Properties)
                        {
                            if (!string.IsNullOrEmpty(property.Description))
                            {
                                sb.AppendLine("  /**");
                                sb.AppendLine($"   * {property.Description}");
                                sb.AppendLine("   */");
                            }
                            sb.AppendLine($"  {ToCamelCase(property.Name)}: {ConvertToTypeScriptType(property.Type)};");
                        }

                        sb.AppendLine("}");
                        sb.AppendLine();
                    }
                }
                else
                {
                    // Regular interfaces
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.AppendLine("/**");
                        sb.AppendLine($" * {modelClass.Description}");
                        sb.AppendLine(" */");
                    }

                    sb.AppendLine($"export interface {modelClass.Name} {{");

                    foreach (var property in modelClass.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            sb.AppendLine("  /**");
                            sb.AppendLine($"   * {property.Description}");
                            sb.AppendLine("   */");
                        }
                        sb.AppendLine($"  {ToCamelCase(property.Name)}{(property.IsRequired ? "" : "?")}: {ConvertToTypeScriptType(property.Type)};");
                    }

                    sb.AppendLine("}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public async Task<string> GenerateInterfaceAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();

            sb.AppendLine("// TypeScript Interface Definition");
            sb.AppendLine($"export interface {model.ClientName}Interface {{");

            foreach (var method in model.Methods)
            {
                var parameters = new List<string>();

                foreach (var p in method.Parameters)
                {
                    parameters.Add($"{ToCamelCase(p.Name)}{(p.IsRequired ? "" : "?")}: {ConvertToTypeScriptType(p.Type)}");
                }

                if (method.RequestBody != null)
                {
                    parameters.Add($"data: {ConvertToTypeScriptType(method.RequestBody.Type)}");
                }

                var parameterString = string.Join(", ", parameters);
                var responseType = method.ResponseType == "void" ? "void" : ConvertToTypeScriptType(method.ResponseType);
                var methodName = ToCamelCase(method.Name);

                if (!string.IsNullOrEmpty(method.Summary))
                {
                    sb.AppendLine("  /**");
                    sb.AppendLine($"   * {method.Summary}");
                    sb.AppendLine("   */");
                }

                sb.AppendLine($"  {methodName}({parameterString}): Promise<{responseType}>;");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        public async Task<Dictionary<string, string>> GenerateAllAsync(CodeModel model)
        {
            var result = new Dictionary<string, string>();

            result[$"{model.ClientName}.ts"] = await GenerateClientAsync(model).ConfigureAwait(false);
            result["models.ts"] = await GenerateModelsAsync(model).ConfigureAwait(false);
            result[$"{model.ClientName}Interface.ts"] = await GenerateInterfaceAsync(model).ConfigureAwait(false);

            // Generate package.json for npm
            result["package.json"] = GeneratePackageJson(model);
            result["tsconfig.json"] = GenerateTsConfig();

            return result;
        }

        private async Task GenerateMethod(StringBuilder sb, ApiMethod method, string namespace1)
        {
            await Task.Yield();

            // Method documentation
            if (!string.IsNullOrEmpty(method.Summary))
            {
                sb.AppendLine("  /**");
                sb.AppendLine($"   * {method.Summary}");
                sb.AppendLine("   */");
            }

            var parameters = new List<string>();
            var queryParams = new List<string>();
            var headerParams = new List<string>();

            foreach (var param in method.Parameters)
            {
                var paramName = ToCamelCase(param.Name);
                parameters.Add($"{paramName}{(param.IsRequired ? "" : "?")}: {ConvertToTypeScriptType(param.Type)}");

                switch (param.Location)
                {
                    case "query":
                        queryParams.Add($"{param.Name}: {paramName}");
                        break;
                    case "header":
                        headerParams.Add($"'{param.Name}': {paramName}");
                        break;
                }
            }

            if (method.RequestBody != null)
            {
                parameters.Add($"data: {ConvertToTypeScriptType(method.RequestBody.Type)}");
            }

            var parameterString = string.Join(", ", parameters);
            var responseType = method.ResponseType == "void" ? "void" : ConvertToTypeScriptType(method.ResponseType);
            var methodName = ToCamelCase(method.Name);

            sb.AppendLine($"  async {methodName}({parameterString}): Promise<{responseType}> {{");

            // Build URL with path parameters
            var url = method.Path;
            var pathParams = method.Parameters.Where(p => p.Location == "path").ToList();

            foreach (var param in pathParams)
            {
                url = url.Replace($"{{{param.Name}}}", $"{{{ToCamelCase(param.Name)}}}");
            }

            var urlString = url;

            // Handle query parameters
            if (queryParams.Any())
            {
                sb.AppendLine("    const params = {");
                foreach (var queryParam in queryParams)
                {
                    sb.AppendLine($"      {queryParam},");
                }
                sb.AppendLine("    };");
urlString = url;

                // Handle API key in query
                sb.AppendLine("    if (this.authConfig.type === 'apiKey' && this.authConfig.location === 'query' && this.authValue) {");
                sb.AppendLine("      params[this.authConfig.name || 'apiKey'] = this.authValue;");
                sb.AppendLine("    }");
            }

            // Build request config
            sb.AppendLine("    const config = {");
            sb.AppendLine($"      method: '{method.HttpMethod}',");
            sb.AppendLine($"      url: `{urlString.Replace("{", "${")}`, ");

            if (queryParams.Any())
            {
                sb.AppendLine("      params,");
            }

            if (method.RequestBody != null)
            {
                sb.AppendLine("      data,");
            }

            sb.AppendLine("    };");

            // Execute request
            sb.AppendLine($"    const response: AxiosResponse<{responseType}> = await this.axiosInstance.request(config);");
            sb.AppendLine("    return response.data;");
            sb.AppendLine("  }");
        }

        private static string ConvertToTypeScriptType(string csharpType)
        {
            return csharpType.ToLower() switch
            {
                "string" => "string",
                "int" => "number",
                "long" => "number",
                "double" => "number",
                "float" => "number",
                "bool" or "boolean" => "boolean",
                "datetime" => "Date",
                "date" => "Date",
                "guid" => "string",
                var list when list.Contains("list<") || list.Contains("ienumerable<") =>
                    list.Replace("List<", "").Replace("IEnumerable<", "").Replace(">", "") + "[]",
                var dict when dict.Contains("dictionary<") || dict.Contains("map<") =>
                    "{ [key: string]: any }",
                _ => csharpType
            };
        }

        private static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        private string GeneratePackageJson(CodeModel model)
        {
            return $@"{{
  ""name"": ""{model.Namespace?.ToLower().Replace(".", "-")}-api-client"",
  ""version"": ""1.0.0"",
  ""description"": ""Auto-generated REST API client"",
  ""main"": ""{model.ClientName}.js"",
  ""types"": ""{model.ClientName}.d.ts"",
  ""scripts"": {{
    ""build"": ""tsc"",
    ""test"": ""echo \""No tests specified\""""
  }},
  ""keywords"": [""api"", ""client"", ""typescript"", ""rest""],
  ""author"": ""RestApiGenerator"",
  ""license"": ""MIT"",
  ""dependencies"": {{
    ""axios"": ""^1.6.0""
  }},
  ""devDependencies"": {{
    ""typescript"": ""^5.0.0"",
    ""@types/node"": ""^20.0.0""
  }}
}}";
        }

        private string GenerateTsConfig()
        {
            return @"{
  ""compilerOptions"": {
    ""target"": ""ES2020"",
    ""module"": ""CommonJS"",
    ""lib"": [""ES2020""],
    ""outDir"": ""./dist"",
    ""rootDir"": ""./"",
    ""strict"": true,
    ""esModuleInterop"": true,
    ""skipLibCheck"": true,
    ""forceConsistentCasingInFileNames"": true,
    ""declaration"": true,
    ""declarationMap"": true,
    ""sourceMap"": true
  },
  ""include"": [""*.ts""],
  ""exclude"": [""node_modules"", ""dist""]
}";
        }
    }
}
