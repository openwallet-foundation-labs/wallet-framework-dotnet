using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// Represents a claim identifier used in DCQL.
/// </summary>
[JsonConverter(typeof(ClaimIdentifierJsonConverter))]
public record ClaimIdentifier
{
    private string Value { get; }

    [JsonConstructor]
    private ClaimIdentifier(string value) => Value = value;

    public string AsString() => Value;

    public static Validation<ClaimIdentifier> Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new StringIsNullOrWhitespaceError<ClaimIdentifier>();

        return new ClaimIdentifier(value);
    }
}

public class ClaimIdentifierJsonConverter : JsonConverter<ClaimIdentifier?>
{
    public override ClaimIdentifier? ReadJson(JsonReader reader, Type objectType, ClaimIdentifier? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
            {
                var value = (string)reader.Value!;
                return ClaimIdentifier.Validate(value).ToOption().ToNullable();
            }
            case JsonToken.StartObject:
            {
                var obj = JObject.Load(reader);
                var value = obj["value"]?.ToString();
                return ClaimIdentifier.Validate(value).ToOption().ToNullable();
            }
            default:
                return null;
        }
    }

    public override void WriteJson(JsonWriter writer, ClaimIdentifier? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("value");
        writer.WriteValue(value?.AsString());
        writer.WriteEndObject();
    }
} 
