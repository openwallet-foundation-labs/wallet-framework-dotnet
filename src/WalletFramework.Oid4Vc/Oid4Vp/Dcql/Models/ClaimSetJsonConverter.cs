using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

public class ClaimSetJsonConverter : JsonConverter<IReadOnlyList<ClaimSet>>
{
    public override void WriteJson(JsonWriter writer, IReadOnlyList<ClaimSet>? value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var claimSet in value!)
        {
            writer.WriteStartArray();
            foreach (var claimId in claimSet.Claims)
            {
                writer.WriteValue(claimId.AsString());
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

    public override IReadOnlyList<ClaimSet> ReadJson(JsonReader reader, Type objectType, IReadOnlyList<ClaimSet>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var result = array.TraverseAll(token =>
        {
            var innerArray = token.Type == JTokenType.Array 
                ? (JArray)token 
                : new JArray(token);

            return ClaimSetFun.Validate(innerArray);
        });
        
        return result.Match(
            list => list.ToList(),
            errors => throw new JsonSerializationException(
                $"Failed to deserialize ClaimSet list: {string.Join(", ", errors.SelectMany(e => e.Message))}")
        );
    }
} 
