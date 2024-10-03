using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;

namespace WalletFramework.SdJwtVc.Models.Vct;

/// <summary>
///     Represents the specifics about a claim.
/// </summary>
public class ClaimMetadata
{
    /// <summary>
    ///     Gets or sets the claim or claims that are being addressed.
    /// </summary>
    [JsonProperty("path")]
    public string[] Path { get; set; }
}
