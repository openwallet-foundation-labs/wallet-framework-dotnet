using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential set query.
/// </summary>
public class CredentialSetQuery
{
    /// <summary>
    /// Specifies the purpose of the query.
    /// </summary>
    [JsonProperty("purpose")]
    public string Purpose { get; set; } = null!;
    
    /// <summary>
    /// Indicates whether this set of Credentials is required to satisfy the particular use case at the Verifier.
    /// </summary>
    [JsonProperty("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Represents a collection, where each value is a list of Credential query identifiers representing one set Credentials that satisfies the use case.
    /// </summary>
    [JsonProperty("options")]
    public string[][] Options { get; set; } = null!;
}
