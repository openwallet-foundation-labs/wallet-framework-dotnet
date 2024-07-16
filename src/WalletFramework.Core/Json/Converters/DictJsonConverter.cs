using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WalletFramework.Core.Json.Converters;

public sealed class DictJsonConverter<T1, T2> : JsonConverter<Dictionary<T1, T2>>
{
    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, Dictionary<T1, T2>? dict, JsonSerializer serializer)
    {
        var dictJson = new JObject();
        foreach (var (key, config) in dict!)
        {
            var x = JObject.FromObject(config!);
            dictJson.Add(key!.ToString(), x);
        }
        serializer.Serialize(writer, dictJson);
    }

    public override Dictionary<T1, T2> ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<T1, T2>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) =>
        throw new NotImplementedException();
}
