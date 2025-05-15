using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Core.ClaimPaths;

public class ClaimPathJsonConverter : JsonConverter<ClaimPath>
{
    public override bool CanWrite => true;

    public override void WriteJson(JsonWriter writer, ClaimPath value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        foreach (var component in value.GetPathComponents())
        {
            component.Match(
                onKey: k => { writer.WriteValue(k); return 0; },
                onIndex: i => { writer.WriteValue(i); return 0; },
                onSelectAll: _ => { writer.WriteNull(); return 0; }
            );
        }
        writer.WriteEndArray();
    }

    public override ClaimPath ReadJson(JsonReader reader, Type objectType, ClaimPath existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        var validation = ClaimPath.FromJArray(array);
        return validation.UnwrapOrThrow();
    }
} 