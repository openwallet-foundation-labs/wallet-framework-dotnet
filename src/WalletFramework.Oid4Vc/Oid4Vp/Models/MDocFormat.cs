using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents the claim format, encapsulating supported algorithms.
/// </summary>
public class MDocFormat
{
    /// <summary>
    ///     Gets the names of supported algorithms.
    /// </summary>
    [JsonProperty("alg")]
    public string[] Alg { get; private set; } = null!;
        
    /// <summary>
    ///     Gets the names of supported proof types.
    /// </summary>
    [JsonProperty("proof_type")]
    public string[] ProofTypes { get; private set; } = null!;
}
