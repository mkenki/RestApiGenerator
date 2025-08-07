using System;
using System.Collections.Generic;
using System.Text; // Added for Encoding
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestApiGenerator.Core.Generators.JsonConverters
{
    public class PolymorphicConverter<TBase> : JsonConverter<TBase>
    {
        private readonly string _discriminatorPropertyName;
        private readonly Dictionary<string, Type> _typeMapping;

        public PolymorphicConverter(string discriminatorPropertyName, Dictionary<string, Type> typeMapping)
        {
            _discriminatorPropertyName = discriminatorPropertyName ?? throw new ArgumentNullException(nameof(discriminatorPropertyName));
            _typeMapping = typeMapping ?? throw new ArgumentNullException(nameof(typeMapping));
        }

        public override TBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                if (!document.RootElement.TryGetProperty(_discriminatorPropertyName, out JsonElement discriminatorElement))
                {
                    throw new JsonException($"Discriminator property '{_discriminatorPropertyName}' not found.");
                }

                var discriminatorValue = discriminatorElement.GetString();

                if (discriminatorValue == null || !_typeMapping.TryGetValue(discriminatorValue, out Type? targetType))
                {
                    throw new JsonException($"Unknown discriminator value '{discriminatorValue}' for property '{_discriminatorPropertyName}'.");
                }

                // Create a new reader from the document's root element
                var rawText = document.RootElement.GetRawText();
                var newReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(rawText));

                // Deserialize to the specific target type
                return (TBase?)JsonSerializer.Deserialize(ref newReader, targetType, options);
            }
        }

        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
        {
            // For polymorphic serialization, you might want to write the discriminator property
            // and then serialize the actual type.
            // This is a simplified implementation.
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
