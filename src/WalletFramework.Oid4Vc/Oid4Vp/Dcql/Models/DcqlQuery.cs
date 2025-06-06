using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
///     Represents constraints on the combinations of credentials and claims that articulate what Verifier requires
/// </summary>
public record DcqlQuery
{
    /// <summary>
    ///     Represents a collection of Credential queries
    /// </summary>
    [JsonProperty("credentials", Required = Required.Always)]
    public CredentialQuery[] CredentialQueries { get; set; } = null!;

    /// <summary>
    ///     Represents credential set queries that specifies additional constraints
    /// </summary>
    [JsonProperty("credential_sets")]
    public CredentialSetQuery[]? CredentialSetQueries { get; set; }
}
