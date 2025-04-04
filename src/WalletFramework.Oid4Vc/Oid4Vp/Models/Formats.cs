using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Formats
{
    [JsonProperty("vc+sd-jwt")]
    public SdJwtFormat? SdJwtVcFormat { get; init; }
    
    [JsonProperty("dc+sd-jwt")]
    public SdJwtFormat? SdJwtDcFormat { get; init; }
    
    [JsonProperty("mso_mdoc")]
    public MDocFormat? MDocFormat { get; init; }
}
