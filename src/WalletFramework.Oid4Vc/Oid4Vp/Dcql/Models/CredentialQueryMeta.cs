using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// The credential query meta.
/// </summary>
public class CredentialQueryMeta
{
    /// <summary>
    /// Specifies allowed values for the type of the requested Verifiable credential.
    /// </summary>
    [JsonProperty("vct_values")]
    public string[] Vcts { get; set; } = null!;
    
    /// <summary>
    /// Specifies an allower value for the doctype of the requested Verifiable credential.
    /// </summary>
    [JsonProperty("doctype_value")]
    public string Doctype { get; set; } = null!;
}
