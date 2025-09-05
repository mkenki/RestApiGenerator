using RestApiGenerator.Core.Models;
using System.Text;
using System.Text.Json;

namespace RestApiGenerator.Core.Generators
{
    public class JavaGenerator : ICodeGenerator
    {
        public async Task<string> GenerateClientAsync(CodeModel model)
        {
            var sb = new StringBuilder();

            // Package declaration
            sb.AppendLine($"package {model.Namespace.ToLower()};");
            sb.AppendLine();
            sb.AppendLine("import org.apache.http.client.HttpClient;");
            sb.AppendLine("import org.apache.http.client.methods.*;");
            sb.AppendLine("import org.apache.http.impl.client.HttpClients;");
            sb.AppendLine("import org.apache.http.HttpEntity;");
            sb.AppendLine("import org.apache.http.entity.StringEntity;");
            sb.AppendLine("import org.apache.http.util.EntityUtils;");
            sb.AppendLine("import org.apache.http.HttpResponse;");
            sb.AppendLine("import org.apache.http.client.methods.HttpGet;");
            sb.AppendLine("import org.apache.http.client.methods.HttpPost;");
            sb.AppendLine("import org.apache.http.client.methods.HttpPut;");
            sb.AppendLine("import org.apache.http.client.methods.HttpDelete;");
            sb.AppendLine("import org.apache.http.client.methods.HttpPatch;");
            sb.AppendLine("import org.apache.http.client.entity.UrlEncodedFormEntity;");
            sb.AppendLine("import org.apache.http.message.BasicNameValuePair;");
            sb.AppendLine("import org.apache.http.NameValuePair;");
            sb.AppendLine("import com.fasterxml.jackson.databind.ObjectMapper;");
            sb.AppendLine("import com.fasterxml.jackson.core.JsonProcessingException;");
            sb.AppendLine("import java.io.IOException;");
            sb.AppendLine("import java.io.UnsupportedEncodingException;");
            sb.AppendLine("import java.util.Map;");
            sb.AppendLine("import java.util.HashMap;");
            sb.AppendLine("import java.util.List;");
            sb.AppendLine("import java.util.ArrayList;");
            sb.AppendLine("import java.net.URLEncoder;");
            sb.AppendLine($"import {model.Namespace.ToLower()}.models.*;");
            sb.AppendLine();

            // Documentation
            sb.AppendLine("/**");
            sb.AppendLine(" * Auto-generated REST API client");
            sb.AppendLine(" * Generated from OpenAPI specification");
            sb.AppendLine(" */");
            sb.AppendLine($"public class {model.ClientName} {{");
            sb.AppendLine();
            sb.AppendLine("    private final HttpClient httpClient;");
            sb.AppendLine("    private final ObjectMapper objectMapper;");
            sb.AppendLine("    private final AuthenticationConfig authenticationConfig;");
            sb.AppendLine("    private final String authenticationValue;");
            sb.AppendLine("    private static final String BASE_URL = \"{model.BaseUrl}\";");
            sb.AppendLine();

            // Constructor
            sb.AppendLine($"    public {model.ClientName}(AuthenticationConfig authenticationConfig, String authenticationValue) {{");
            sb.AppendLine("        this.httpClient = HttpClients.createDefault();");
            sb.AppendLine("        this.objectMapper = new ObjectMapper();");
            sb.AppendLine("        this.authenticationConfig = authenticationConfig;");
            sb.AppendLine("        this.authenticationValue = authenticationValue;");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Generate methods
            foreach (var method in model.Methods)
            {
                await GenerateMethod(sb, method, model.Namespace);
                sb.AppendLine();
            }

            // Helper methods
            sb.AppendLine("    private <T> T sendRequest(String method, String url, Object body, Class<T> responseType, Map<String, String> headers) throws IOException, JsonProcessingException {");
            sb.AppendLine("        HttpRequestBase request;");
            sb.AppendLine("        ");
            sb.AppendLine("        switch (method.toUpperCase()) {");
            sb.AppendLine("            case \"GET\":");
            sb.AppendLine("                request = new HttpGet(url);");
            sb.AppendLine("                break;");
            sb.AppendLine("            case \"POST\":");
            sb.AppendLine("                request = new HttpPost(url);");
            sb.AppendLine("                break;");
            sb.AppendLine("            case \"PUT\":");
            sb.AppendLine("                request = new HttpPut(url);");
            sb.AppendLine("                break;");
            sb.AppendLine("            case \"DELETE\":");
            sb.AppendLine("                request = new HttpDelete(url);");
            sb.AppendLine("                break;");
            sb.AppendLine("            case \"PATCH\":");
            sb.AppendLine("                request = new HttpPatch(url);");
            sb.AppendLine("                break;");
            sb.AppendLine("            default:");
            sb.AppendLine("                throw new IllegalArgumentException(\"Unsupported HTTP method: \" + method);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // Set default content type");
            sb.AppendLine("        request.setHeader(\"Content-Type\", \"application/json\");");
            sb.AppendLine("        request.setHeader(\"Accept\", \"application/json\");");
            sb.AppendLine();
            sb.AppendLine("        // Apply authentication");
            sb.AppendLine("        if (AuthenticationType.BEARER.equals(authenticationConfig.getType())) {");
            sb.AppendLine("            request.setHeader(\"Authorization\", \"Bearer \" + authenticationValue);");
            sb.AppendLine("        } else if (AuthenticationType.API_KEY.equals(authenticationConfig.getType())) {");
            sb.AppendLine("            if (AuthenticationLocation.HEADER.equals(authenticationConfig.getLocation())) {");
            sb.AppendLine("                request.setHeader(authenticationConfig.getName(), authenticationValue);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // Add custom headers");
            sb.AppendLine("        if (headers != null) {");
            sb.AppendLine("            headers.forEach(request::setHeader);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // Set request body");
            sb.AppendLine("        if (body != null && (request instanceof HttpPost || request instanceof HttpPut)) {");
            sb.AppendLine("            String jsonBody = objectMapper.writeValueAsString(body);");
            sb.AppendLine("            ((HttpEntityEnclosingRequestBase) request).setEntity(new StringEntity(jsonBody, \"UTF-8\"));");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        HttpResponse response = httpClient.execute(request);");
            sb.AppendLine("        int statusCode = response.getStatusLine().getStatusCode();");
            sb.AppendLine();
            sb.AppendLine("        if (statusCode >= 200 && statusCode < 300) {");
            sb.AppendLine("            if (Void.class.equals(responseType)) {");
            sb.AppendLine("                return null;");
            sb.AppendLine("            }");
            sb.AppendLine("            HttpEntity entity = response.getEntity();");
            sb.AppendLine("            if (entity != null) {");
            sb.AppendLine("                String responseBody = EntityUtils.toString(entity, \"UTF-8\");");
            sb.AppendLine("                return objectMapper.readValue(responseBody, responseType);");
            sb.AppendLine("            }");
            sb.AppendLine("            return null;");
            sb.AppendLine("        } else {");
            sb.AppendLine("            throw new RuntimeException(\"API call failed with status: \" + statusCode);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    private String buildUrlWithQueryParams(String url, Map<String, Object> queryParams) throws UnsupportedEncodingException {");
            sb.AppendLine("        if (queryParams == null || queryParams.isEmpty()) {");
            sb.AppendLine("            return url;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        StringBuilder sb = new StringBuilder(url);");
            sb.AppendLine("        if (url.contains(\"?\")) {");
            sb.AppendLine("            sb.append(\"&\");");
            sb.AppendLine("        } else {");
            sb.AppendLine("            sb.append(\"?\");");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        boolean isFirst = true;");
            sb.AppendLine();
            sb.AppendLine("        for (Map.Entry<String, Object> entry : queryParams.entrySet()) {");
            sb.AppendLine("            if (!isFirst) {");
            sb.AppendLine("                sb.append(\"&\");");
            sb.AppendLine("            }");
            sb.AppendLine("            isFirst = false;");
            sb.AppendLine("            sb.append(URLEncoder.encode(entry.getKey(), \"UTF-8\"));");
            sb.AppendLine("            sb.append(\"=\");");
            sb.AppendLine("            sb.append(URLEncoder.encode(entry.getValue().toString(), \"UTF-8\"));");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // Handle API key in query");
            sb.AppendLine("        if (AuthenticationType.API_KEY.equals(authenticationConfig.getType()) &&");
            sb.AppendLine("            AuthenticationLocation.QUERY.equals(authenticationConfig.getLocation()) &&");
            sb.AppendLine("            authenticationValue != null) {");
            sb.AppendLine("            if (!isFirst) {");
            sb.AppendLine("                sb.append(\"&\");");
            sb.AppendLine("            }");
            sb.AppendLine("            sb.append(URLEncoder.encode(authenticationConfig.getName(), \"UTF-8\"));");
            sb.AppendLine("            sb.append(\"=\");");
            sb.AppendLine("            sb.append(URLEncoder.encode(authenticationValue, \"UTF-8\"));");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return sb.toString();");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }

        public async Task<string> GenerateModelsAsync(CodeModel model)
        {
            await Task.Yield();
            var sb = new StringBuilder();

            // Package declaration
            sb.AppendLine($"package {model.Namespace.ToLower()}.models;");
            sb.AppendLine();
            sb.AppendLine("import com.fasterxml.jackson.annotation.JsonProperty;");
            sb.AppendLine("import java.util.List;");
            sb.AppendLine("import java.util.Map;");
            sb.AppendLine("import java.time.LocalDateTime;");
            sb.AppendLine("import java.time.LocalDate;");
            sb.AppendLine();

            // Add enums used by authentication
            sb.AppendLine("public enum AuthenticationType {");
            sb.AppendLine("    NONE,");
            sb.AppendLine("    API_KEY,");
            sb.AppendLine("    BEARER");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("public enum AuthenticationLocation {");
            sb.AppendLine("    NONE,");
            sb.AppendLine("    HEADER,");
            sb.AppendLine("    QUERY");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("public class AuthenticationConfig {");
            sb.AppendLine("    private AuthenticationType type;");
            sb.AppendLine("    private AuthenticationLocation location;");
            sb.AppendLine("    private String name;");
            sb.AppendLine();
            sb.AppendLine("    public AuthenticationConfig() {}");
            sb.AppendLine();
            sb.AppendLine("    public AuthenticationType getType() { return type; }");
            sb.AppendLine("    public void setType(AuthenticationType type) { this.type = type; }");
            sb.AppendLine();
            sb.AppendLine("    public AuthenticationLocation getLocation() { return location; }");
            sb.AppendLine("    public void setLocation(AuthenticationLocation location) { this.location = location; }");
            sb.AppendLine();
            sb.AppendLine("    public String getName() { return name; }");
            sb.AppendLine("    public void setName(String name) { this.name = name; }");
            sb.AppendLine("}");
            sb.AppendLine();

            foreach (var modelClass in model.Models)
            {
                if (modelClass.IsPolymorphic)
                {
                    // Generate interface for polymorphic base type
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.AppendLine("/**");
                        sb.AppendLine($" * {modelClass.Description}");
                        sb.AppendLine(" */");
                    }
                    sb.AppendLine($"public interface {modelClass.Name} {{");
                    foreach (var property in modelClass.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            sb.AppendLine("    /**");
                            sb.AppendLine($"     * {property.Description}");
                            sb.AppendLine("     */");
                        }
                        sb.AppendLine($"    {ConvertToJavaType(property.Type, property.IsRequired)} get{ToPascalCase(property.Name)}();");
                        sb.AppendLine($"    void set{ToPascalCase(property.Name)}({ConvertToJavaType(property.Type, property.IsRequired)} value);");
                        sb.AppendLine();
                    }
                    sb.AppendLine("}");
                    sb.AppendLine();

                    // Generate concrete classes for each subtype
                    foreach (var subType in modelClass.SubTypes)
                    {
                        if (!string.IsNullOrEmpty(subType.Description))
                        {
                            sb.AppendLine("/**");
                            sb.AppendLine($" * {subType.Description}");
                            sb.AppendLine(" */");
                        }
                        sb.AppendLine($"public class {subType.Name} implements {modelClass.Name} {{");
                        foreach (var property in subType.Properties)
                        {
                            if (!string.IsNullOrEmpty(property.Description))
                            {
                                sb.AppendLine("    /**");
                                sb.AppendLine($"     * {property.Description}");
                                sb.AppendLine("     */");
                            }
                            if (!string.IsNullOrEmpty(property.JsonPropertyName))
                            {
                                sb.AppendLine($"    @JsonProperty(\"{property.JsonPropertyName}\")");
                            }
                            sb.AppendLine($"    private {ConvertToJavaType(property.Type, property.IsRequired)} {ToCamelCase(property.Name)};");
                            sb.AppendLine();
                        }

                        // Generate getters and setters
                        foreach (var property in subType.Properties)
                        {
                            var pascalCaseName = ToPascalCase(property.Name);
                            var javaType = ConvertToJavaType(property.Type, property.IsRequired);
                            sb.AppendLine($"    public {javaType} get{pascalCaseName}() {{");
                            sb.AppendLine($"        return {ToCamelCase(property.Name)};");
                            sb.AppendLine("    }");
                            sb.AppendLine();
                            sb.AppendLine($"    public void set{pascalCaseName}({javaType} value) {{");
                            sb.AppendLine($"        this.{ToCamelCase(property.Name)} = value;");
                            sb.AppendLine("    }");
                            sb.AppendLine();
                        }
                        sb.AppendLine("}");
                        sb.AppendLine();
                    }
                }
                else
                {
                    // Existing class generation logic for non-polymorphic models
                    if (!string.IsNullOrEmpty(modelClass.Description))
                    {
                        sb.AppendLine("/**");
                        sb.AppendLine($" * {modelClass.Description}");
                        sb.AppendLine(" */");
                    }

                    sb.AppendLine($"public class {modelClass.Name} {{");

                    foreach (var property in modelClass.Properties)
                    {
                        if (!string.IsNullOrEmpty(property.Description))
                        {
                            sb.AppendLine("    /**");
                            sb.AppendLine($"     * {property.Description}");
                            sb.AppendLine("     */");
                        }

                        if (!string.IsNullOrEmpty(property.JsonPropertyName))
                        {
                            sb.AppendLine($"    @JsonProperty(\"{property.JsonPropertyName}\")");
                        }

                        sb.AppendLine($"    private {ConvertToJavaType(property.Type, property.IsRequired)} {ToCamelCase(property.Name)};");
                        sb.AppendLine();
                    }

                    // Generate getters and setters
                    foreach (var property in modelClass.Properties)
                    {
                        var pascalCaseName = ToPascalCase(property.Name);
                        var javaType = ConvertToJavaType(property.Type, property.IsRequired);
                        sb.AppendLine($"    public {javaType} get{pascalCaseName}() {{");
                        sb.AppendLine($"        return {ToCamelCase(property.Name)};");
                        sb.AppendLine("    }");
                        sb.AppendLine();
                        sb.AppendLine($"    public void set{pascalCaseName}({javaType} value) {{");
                        sb.AppendLine($"        this.{ToCamelCase(property.Name)} = value;");
                        sb.AppendLine("    }");
                        sb.AppendLine();
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

            // Package declaration
            sb.AppendLine($"package {model.Namespace.ToLower()};");
            sb.AppendLine();
            sb.AppendLine("import java.util.concurrent.CompletableFuture;");
            sb.AppendLine("import java.util.Map;");
            sb.AppendLine();
            sb.AppendLine("/**");
            sb.AppendLine(" * REST API Client Interface");
            sb.AppendLine(" */");
            sb.AppendLine($"public interface {model.ClientName}Interface {{");

            foreach (var method in model.Methods)
            {
                var parameters = new List<string>();
                foreach (var p in method.Parameters)
                {
                    parameters.Add($"{ConvertToJavaType(p.Type, p.IsRequired)} {ToCamelCase(p.Name)}");
                }

                if (method.RequestBody != null)
                {
                    parameters.Add($"{ConvertToJavaType(method.RequestBody.Type, true)} {ToCamelCase(method.RequestBody.Name)}");
                }

                var parameterString = string.Join(", ", parameters);
                var responseType = method.ResponseType == "void" ? "Void" : ConvertToJavaType(method.ResponseType, true);

                if (!string.IsNullOrEmpty(method.Summary))
                {
                    sb.AppendLine("    /**");
                    sb.AppendLine($"     * {method.Summary}");
                    sb.AppendLine("     */");
                }

                sb.AppendLine($"    {responseType} {ToCamelCase(method.Name)}({parameterString});");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        public async Task<Dictionary<string, string>> GenerateAllAsync(CodeModel model)
        {
            var result = new Dictionary<string, string>();

            result[$"{model.ClientName}.java"] = await GenerateClientAsync(model).ConfigureAwait(false);
            result["Models.java"] = await GenerateModelsAsync(model).ConfigureAwait(false);
            result[$"{model.ClientName}Interface.java"] = await GenerateInterfaceAsync(model).ConfigureAwait(false);

            return result;
        }

        private async Task GenerateMethod(StringBuilder sb, ApiMethod method, string namespace1)
        {
            await Task.Yield();

            // Method documentation
            if (!string.IsNullOrEmpty(method.Summary))
            {
                sb.AppendLine("    /**");
                sb.AppendLine($"     * {method.Summary}");
                sb.AppendLine("     */");
            }

            var parameters = new List<string>();
            var pathParams = new List<string>();
            var queryParams = new List<String>();
            var headerParams = new List<String>();

            foreach (var param in method.Parameters)
            {
                parameters.Add($"{ConvertToJavaType(param.Type, param.IsRequired)} {ToCamelCase(param.Name)}");

                switch (param.Location)
                {
                    case "path":
                        pathParams.Add($"java.lang.String.valueOf({ToCamelCase(param.Name)})");
                        break;
                    case "query":
                        queryParams.Add($"\"{param.Name}\", {ToCamelCase(param.Name)}");
                        break;
                    case "header":
                        headerParams.Add($"\"{param.Name}\", java.lang.String.valueOf({ToCamelCase(param.Name)})");
                        break;
                }
            }

            if (method.RequestBody != null)
            {
                parameters.Add($"{ConvertToJavaType(method.RequestBody.Type, true)} {ToCamelCase(method.RequestBody.Name)}");
            }

            var parameterString = string.Join(", ", parameters);
            var responseType = method.ResponseType == "void" ? "Void" : ConvertToJavaType(method.ResponseType, true);
            var methodName = ToCamelCase(method.Name);

            sb.AppendLine($"    public {responseType} {methodName}({parameterString}) {{");

            // Build URL
            var url = method.Path;
            var formattedUrl = $"\"{url}\"";

            if (pathParams.Any())
            {
                formattedUrl = $"java.lang.String.format(\"{url}\", {string.Join(", ", pathParams)})";
            }

            sb.AppendLine($"        String url = {formattedUrl};");
            sb.AppendLine();

            // Build query parameters
            if (queryParams.Any())
            {
                sb.AppendLine("        Map<String, Object> queryParams = new HashMap<>();");
                foreach (var queryParam in queryParams)
                {
                    sb.AppendLine($"        queryParams.put({queryParam});");
                }
                sb.AppendLine("        url = buildUrlWithQueryParams(url, queryParams);");
                sb.AppendLine();
            }

            // Build headers
            if (headerParams.Any())
            {
                sb.AppendLine("        Map<String, String> headers = new HashMap<>();");
                foreach (var headerParam in headerParams)
                {
                    sb.AppendLine($"        headers.put({headerParam});");
                }
                sb.AppendLine($"        return sendRequest(\"{method.HttpMethod}\", url, {(method.RequestBody != null ? ToCamelCase(method.RequestBody.Name) : "null")}, {responseType}.class, headers);");
            }
            else
            {
                sb.AppendLine($"        return sendRequest(\"{method.HttpMethod}\", url, {(method.RequestBody != null ? ToCamelCase(method.RequestBody.Name) : "null")}, {responseType}.class, null);");
            }

            sb.AppendLine("    }");
        }

        private static string ConvertToJavaType(string csharpType, bool isRequired)
        {
            return csharpType.ToLower() switch
            {
                "string" => isRequired ? "String" : "String",
                "int" => isRequired ? "int" : "Integer",
                "long" => isRequired ? "long" : "Long",
                "double" => isRequired ? "double" : "Double",
                "float" => isRequired ? "float" : "Float",
                "bool" or "boolean" => isRequired ? "boolean" : "Boolean",
                "datetime" => "java.time.LocalDateTime",
                "date" => "java.time.LocalDate",
                "guid" => "String",
                var list when list.Contains("list<") || list.Contains("ienumerable<") =>
                    list.Replace("List<", "List<").Replace("IEnumerable<", "List<").Replace(">", ">") +
                    (isRequired ? "" : "?"),
                var dict when dict.Contains("dictionary<") ||
                            dict.Contains("map<") =>
                    dict.Replace("Dictionary<", "Map<").Replace("IDictionary<", "Map<").Replace("string,", "String,"),
                _ => csharpType
            };
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
