using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public class ClientJwksConverter : JsonConverter<List<JsonWebKey>>
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, List<JsonWebKey>? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override List<JsonWebKey> ReadJson(
        JsonReader reader,
        Type objectType,
        List<JsonWebKey>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        var validKeysArray =
            from keys in json.GetByKey("keys")
            from keysArray in keys.ToJArray()
            select keysArray;

        var result = validKeysArray.OnSuccess(keysArray =>
        {
            return keysArray.Select(keyToken => JsonWebKey.Create(keyToken.ToString())).ToList();
        }).UnwrapOrThrow();

        return result;
    }
}
