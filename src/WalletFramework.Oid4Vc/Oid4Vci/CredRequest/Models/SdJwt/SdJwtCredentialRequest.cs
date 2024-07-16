using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models.SdJwt;

public record SdJwtCredentialRequest
{
    public CredentialRequest VciRequest { get; }
    
    /// <summary>
    ///     Gets the verifiable credential type (vct).
    /// </summary>
    [JsonProperty("vct")]
    public Vct Vct { get; }
    
    internal SdJwtCredentialRequest(CredentialRequest vciRequest, Vct vct)
    {
        VciRequest = vciRequest;
        Vct = vct;
    }
}

public static class SdJwtCredentialRequestFun
{
    public static string AsJson(this SdJwtCredentialRequest request)
    {
        var json = new JObject();
        
        var vciRequest = JObject.FromObject(request.VciRequest);
        foreach (var property in vciRequest.Properties())
        {
            json.Add(property);
        }

        var vct = JToken.FromObject(request.Vct);
        json.Add("vct", vct);
        
        return json.ToString();
    }
}
