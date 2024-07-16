using LanguageExt;
using Newtonsoft.Json;
using OneOf;

namespace WalletFramework.Core.Json.Converters;

public class OneOfJsonConverter<TOneOf, T1, T2> : JsonConverter<TOneOf> where TOneOf : OneOfBase<T1, T2>
{
    public override void WriteJson(JsonWriter writer, TOneOf? oneOf, JsonSerializer serializer)
    {
        oneOf!.Match(
            t1 =>
            {
                serializer.Serialize(writer, t1);
                return Unit.Default;
            },
            t2 =>
            {
                serializer.Serialize(writer, t2);
                return Unit.Default;
            });
    }

    public override TOneOf ReadJson(JsonReader reader, Type objectType, TOneOf? existingValue, bool hasExistingValue,
        JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override bool CanRead => false;
}
