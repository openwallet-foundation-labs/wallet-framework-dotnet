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
    [JsonProperty("issuerauth_alg_values")]
    public string[]? IssuerAuthAlgValues { get; init; }
        
    /// <summary>
    ///     Gets the names of supported proof types.
    /// </summary>
    [JsonProperty("deviceauth_alg_values")]
    public string[]? DeviceAuthAlgValues { get; init; }
}
