using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

public class CredentialQueryIdListJsonConverter : JsonConverter<IReadOnlyList<CredentialQueryId>>
{
    public override void WriteJson(JsonWriter writer, IReadOnlyList<CredentialQueryId>? value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var id in value!)
        {
            writer.WriteValue(id.AsString());
        }
        writer.WriteEndArray();
    }

    public override IReadOnlyList<CredentialQueryId> ReadJson(JsonReader reader, Type objectType, IReadOnlyList<CredentialQueryId>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var result = array.TraverseAll(token => CredentialQueryId.Create(token.ToString()));
            
        return result.Match(
            list => list.ToArray(),
            errors => throw new JsonSerializationException(
                $"Failed to deserialize CredentialQueryId list: {string.Join(", ", errors.SelectMany(e => e.Message))}")
        );
    }
} 