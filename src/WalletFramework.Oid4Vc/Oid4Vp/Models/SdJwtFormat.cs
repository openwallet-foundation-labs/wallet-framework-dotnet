using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record SdJwtFormat
{
    [JsonProperty("sd-jwt_alg_values")]
    public List<string>? IssuerSignedJwtAlgValues { get; init; }
    
    [JsonProperty("kb-jwt_alg_values")]
    public List<string>? KeyBindingJwtAlgValues { get; init; }
};
