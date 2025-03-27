using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public class ClientJwksConverter : JsonConverter<Option<JwkSet>>
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override Option<JwkSet> ReadJson(JsonReader reader, Type objectType, Option<JwkSet> existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        return JwkSet.FromJObject(jObject).ToOption();
    }

    public override void WriteJson(JsonWriter writer, Option<JwkSet> value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
