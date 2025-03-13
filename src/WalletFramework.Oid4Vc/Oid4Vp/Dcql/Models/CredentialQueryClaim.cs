using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential query claim.
/// </summary>
public class CredentialQueryClaim
{
    /// <summary>
    /// This MUST be a string identifying the particular claim.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// A collection of strings representing a claims path pointer that specifies the path to a claim.
    /// </summary>
    [JsonProperty("path")]
    public string[] Path { get; set; } = null!;
    
    /// <summary>
    /// A collection of strings, integers or booleans values that specifies the expected values of the claim.
    /// </summary>
    [JsonProperty("values")]
    public string[] Values { get; set; } = null!;
}
