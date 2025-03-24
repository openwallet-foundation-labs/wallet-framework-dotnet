using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential query claim.
/// </summary>
public class CredentialClaimQuery
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
    public string[]? Path { get; set; }
    
    /// <summary>
    /// A collection of strings, integers or booleans values that specifies the expected values of the claim.
    /// </summary>
    [JsonProperty("values")]
    public string[]? Values { get; set; }
    
    /// <summary>
    /// For mDoc format (up to VP Draft 23) this MUST be a string that specifies the namespace of the data element within the mdoc. 
    /// </summary>
    [JsonProperty("namespace")]
    public string? Namespace { get; set; }
    
    /// <summary>
    /// Fot mDoc format (up to VP Draft 23) this MUST be a string that specifies the data element identifier of the data element within the provided namespace in the mDoc.
    /// </summary>
    [JsonProperty("claim_name")]
    public string? ClaimName { get; set; }
}
