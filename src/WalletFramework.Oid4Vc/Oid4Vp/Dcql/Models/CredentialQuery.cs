using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential query.
/// </summary>
public class CredentialQuery
{
    /// <summary>
    /// This MUST be a string identifying the Credential in the response.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// This MUST be a string that specifies the format of the requested Verifiable Credential.
    /// </summary>
    [JsonProperty("format")]
    public string Format { get; set; } = null!;

    /// <summary>
    /// An object defining additional properties requested by the Verifier.
    /// </summary>
    [JsonProperty("meta")]
    public CredentialMetaQuery? Meta { get; set; }
    
    /// <summary>
    /// An object defining claims in the requested credential.
    /// </summary>
    [JsonProperty("claims")]
    public CredentialClaimQuery[]? Claims { get; set; }
    
    /// <summary>
    /// Represents a collection, where each value contains a collection of identifiers for elements in claims that specifies which combinations of claims for the credential are requested.
    /// </summary>
    [JsonProperty("claim_sets")]
    public string[][]? ClaimSets { get; set; }
}
