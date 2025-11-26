using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;

[JsonConverter(typeof(VpTokenConverter))]
public record VpToken(Dictionary<CredentialQueryId, List<Presentation>> Presentations)
{
    public string AsString()
    {
        var jObject = new JObject();
        
        foreach (var pair in Presentations)
        {
            jObject[pair.Key.AsString()] = new JArray(pair.Value.Select(presentation => presentation.Value));
        }
        
        return jObject.ToString();
    }

    public JObject AsJObject()
    {
        var jObject = new JObject();
        foreach (var pair in Presentations)
        {
            jObject[pair.Key.AsString()] = new JArray(pair.Value.Select(presentation => presentation.Value));
        }
        return jObject;
    }
    
    public static VpToken FromPresentationMaps(IEnumerable<PresentationMap> maps)
    {
        var dict = maps
            .GroupBy(map => CredentialQueryId.Create(map.Identifier).UnwrapOrThrow())
            .ToDictionary(
                map => map.Key,
                map => map
                    .Select(presentationMap => new Presentation(presentationMap.Presentation))
                    .ToList()
            );
        
        return new VpToken(dict);
    }
}

public class VpTokenConverter : JsonConverter<VpToken>
{
    public override void WriteJson(JsonWriter writer, VpToken? value, JsonSerializer serializer)
    {
        var jObject = new JObject();
        foreach (var pair in value!.Presentations)
        {
            jObject[pair.Key.AsString()] = new JArray(pair.Value.Select(presentation => presentation.Value));
        }
        jObject.WriteTo(writer);
    }

    public override VpToken ReadJson(JsonReader reader, Type objectType, VpToken? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        
        if (token.Type == JTokenType.Object)
        {
            var jObject = (JObject)token;
            var presentations = new Dictionary<CredentialQueryId, List<Presentation>>();
            
            foreach (var property in jObject.Properties())
            {
                var credentialQueryId = CredentialQueryId.Create(property.Name).UnwrapOrThrow();
                var presentationList = new List<Presentation>();
                
                if (property.Value is JArray array)
                {
                    foreach (var item in array)
                    {
                        presentationList.Add(new Presentation(item.ToString()));
                    }
                }
                
                presentations[credentialQueryId] = presentationList;
            }
            
            return new VpToken(presentations);
        }
        else
        {
            throw new JsonSerializationException($"Unexpected token type: {token.Type}");
        } 
    }
}

// Can be either Mdoc DeviceResponse or SD-JWT Presentation Format; we are currently lacking a strong type
// for this
public record Presentation(string Value);
