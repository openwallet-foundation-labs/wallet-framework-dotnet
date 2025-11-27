using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;

public class VpTokenJsonConverter : JsonConverter<VpToken>
{
    public override VpToken ReadJson(
        JsonReader reader,
        Type objectType,
        VpToken? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var dict = new Dictionary<CredentialQueryId, List<Presentation>>();

        foreach (var property in jObject.Properties())
        {
            var credentialQueryId = CredentialQueryId.Create(property.Name).UnwrapOrThrow();
            var presentations = property.Value switch
            {
                JArray array => array.Select(token => new Presentation(token.ToString())).ToList(),
                _ => [new Presentation(property.Value.ToString())]
            };
            dict[credentialQueryId] = presentations;
        }

        return new VpToken(dict);
    }

    public override void WriteJson(JsonWriter writer, VpToken? value, JsonSerializer serializer)
    {
        value!.AsJObject().WriteTo(writer);
    }
}


