using LanguageExt;
using Newtonsoft.Json;

namespace WalletFramework.Core.Json.Converters;

public sealed class OptionJsonConverter<T> : JsonConverter<Option<T>>
{
    public override void WriteJson(JsonWriter writer, Option<T> option, JsonSerializer serializer)
    {
        option.Match(
            t =>
            {
                serializer.Serialize(writer, t);
            },
            () => serializer.Serialize(writer, null)
        );
    }

    public override Option<T> ReadJson(
        JsonReader reader,
        Type objectType,
        Option<T> existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override bool CanRead => false;
}
