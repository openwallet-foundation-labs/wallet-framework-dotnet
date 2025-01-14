using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Formats
{
    [JsonProperty("vc+sd-jwt")]
    public SdJwtFormat? SdJwtFormat { get; init; }
    
    [JsonProperty("dc+jwt")]
    public MDocFormat? MDocFormat { get; init; }
}
