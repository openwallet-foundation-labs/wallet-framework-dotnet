using Newtonsoft.Json;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

public class CredentialQueryIdJsonConverter : JsonConverter<CredentialQueryId>
{
    public override void WriteJson(JsonWriter writer, CredentialQueryId? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.AsString());
    }

    public override CredentialQueryId ReadJson(JsonReader reader, Type objectType, CredentialQueryId? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is string str)
        {
            var result = CredentialQueryId.Create(str);
            return result.Match(
                success => success,
                errors => throw new JsonSerializationException(
                    $"Failed to deserialize CredentialQueryId: {string.Join(", ", errors.Select(e => e.Message))}")
            );
        }
        else
        {
            throw new JsonSerializationException($"Expected string but got {reader.TokenType}");
        }
    }
} 
