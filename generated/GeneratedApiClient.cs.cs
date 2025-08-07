using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GeneratedApiClient.Models;
using RestApiGenerator.Core.Generators.JsonConverters;

namespace GeneratedApiClient
{
    public class GeneratedApiClient : IGeneratedApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly AuthenticationConfig _authenticationConfig;
        private readonly string? _authenticationValue;

        public GeneratedApiClient(HttpClient httpClient, AuthenticationConfig authenticationConfig, string? authenticationValue)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authenticationConfig = authenticationConfig ?? throw new ArgumentNullException(nameof(authenticationConfig));
            _authenticationValue = authenticationValue;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            if (_authenticationConfig.Type == AuthenticationType.Bearer)
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authenticationValue);
            }
            else if (_authenticationConfig.Type == AuthenticationType.ApiKey && 
                     _authenticationConfig.Location == AuthenticationLocation.Header)
            {
                _httpClient.DefaultRequestHeaders.Add(_authenticationConfig.Name, _authenticationValue);
            }
            
            if (_httpClient.BaseAddress == null)
                _httpClient.BaseAddress = new Uri("https://petstore.swagger.io/v2");
        }

        /// <summary>
        /// List all pets
        /// </summary>
        public async Task<List<Pet>> ListPets()
        {
            var url = "/pets";

            var request = new HttpRequestMessage(HttpMethod.GET, url);

            return await SendRequestAsync<List<Pet>>(request, cancellationToken);
        }

        /// <summary>
        /// Create a pet
        /// </summary>
        public async Task<Pet> CreatePet(CreatePetRequest request)
        {
            var url = "/pets";

            var request = new HttpRequestMessage(HttpMethod.POST, url);
            request.Content = CreateJsonContent(request);

            return await SendRequestAsync<Pet>(request, cancellationToken);
        }

        /// <summary>
        /// Get a pet by ID
        /// </summary>
        public async Task<Pet> GetPetById(long petId)
        {
            var url = "/pets/" + petId + "";

            var request = new HttpRequestMessage(HttpMethod.GET, url);

            return await SendRequestAsync<Pet>(request, cancellationToken);
        }

        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_authenticationConfig.Type == AuthenticationType.ApiKey && 
                _authenticationConfig.Location == AuthenticationLocation.Query && 
                !string.IsNullOrEmpty(_authenticationValue))
            {
                var uriBuilder = new UriBuilder(request.RequestUri);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query[_authenticationConfig.Name] = _authenticationValue;
                uriBuilder.Query = query.ToString();
                request.RequestUri = uriBuilder.Uri;
            }
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrEmpty(content))
                return default(T);
                
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        private StringContent CreateJsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
