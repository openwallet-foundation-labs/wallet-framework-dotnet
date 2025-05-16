using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;

public class CredentialSetJsonConverter : JsonConverter<IReadOnlyList<CredentialSetOption>>
{
    public override void WriteJson(JsonWriter writer, IReadOnlyList<CredentialSetOption>? value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var option in value!)
        {
            writer.WriteStartArray();
            foreach (var id in option.Ids)
            {
                writer.WriteValue(id.AsString());
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

    public override IReadOnlyList<CredentialSetOption> ReadJson(JsonReader reader, Type objectType, IReadOnlyList<CredentialSetOption>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var result = array
            .Select(inner => inner.ToObject<List<string>>() ?? [])
            .TraverseAll(CredentialSetOption.FromStrings);
        
        return result.Match(
            list => list.ToList(),
            errors => throw new JsonSerializationException(
                $"Failed to deserialize CredentialSetOption list: {string.Join(", ", errors.SelectMany(e => e.Message))}")
        );
    }
} 
