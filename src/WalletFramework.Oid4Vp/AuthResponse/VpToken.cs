using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vp.Models;

namespace WalletFramework.Oid4Vp.AuthResponse;

public record VpToken(Dictionary<CredentialQueryId, List<Presentation>> Presentations)
{
    public string AsJsonString()
    {
        return AsJObject().ToString();
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
}

public static class VpTokenFun
{
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

// Can be either Mdoc DeviceResponse or SD-JWT Presentation Format; we are currently lacking a strong type
// for this
public record Presentation(string Value);
