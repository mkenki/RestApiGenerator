using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeneratedApiClient.Models
{
    public class Pet
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

    }

    public class CreatePetRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

    }

}
