using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WalletFramework.Core.Json.Converters;

public interface IValueTypeDecoder<out T>
{
    public T Decode(JToken token);
}

public sealed class ValueTypeJsonConverter<T> : JsonConverter<T>
{
    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        var str = value!.ToString();
        writer.WriteValue(str);
    }

    public override T ReadJson(
        JsonReader reader,
        Type objectType,
        T? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) => throw new NotImplementedException();
}

public sealed class ValueTypeJsonConverter<T, TDecoder> : JsonConverter<T> where TDecoder : IValueTypeDecoder<T>
{
    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        var str = value!.ToString();
        writer.WriteValue(str);
    }

    public override T ReadJson(
        JsonReader reader,
        Type objectType,
        T? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var decoder = (TDecoder)Activator.CreateInstance(typeof(TDecoder));
        return decoder.Decode(token);
    }
}
