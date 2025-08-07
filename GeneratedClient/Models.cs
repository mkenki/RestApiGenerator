using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyTestApiClient.Models
{
    public class TestModel
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

    }

}
