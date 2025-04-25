using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vp.Jwk;

public record JwkSet
{
    private JwkSet(IEnumerable<JsonWebKey> jwkSet) => Value = jwkSet.ToList();

    private List<JsonWebKey> Value { get; }

    public IEnumerable<JsonWebKey> AsEnum() => Value;

    public static Validation<JwkSet> FromJObject(JObject json) =>
        from keys in json.GetByKey("keys")
        from keysArray in keys.ToJArray()
        let jwks = keysArray.Select(keyToken => JsonWebKey.Create(keyToken.ToString()))
        select new JwkSet(jwks);

    public static Validation<JwkSet> FromJsonStr(string json)
    {
        JObject jObject;
        try
        {
            jObject = JObject.Parse(json);
        }
        catch (Exception e)
        {
            return new InvalidJsonError(json, e);
        }
        
        return FromJObject(jObject);
    }
}

public static class JwkSetFun
{
    public static JsonWebKey GetEcP256Jwk(this JwkSet jwkSet) => jwkSet.AsEnum().Single(setItem => 
        setItem.Kty == "EC" 
        && setItem.Crv == "P-256");
}
