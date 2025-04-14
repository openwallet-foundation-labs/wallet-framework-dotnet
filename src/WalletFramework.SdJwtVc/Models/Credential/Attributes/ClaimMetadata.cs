using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WalletFramework.SdJwtVc.Models.Credential.Attributes;

/// <summary>
///     Represents the specifics about a claim.
/// </summary>
public class ClaimMetadata   {
    
    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public List<string>? Path { get; set; }
    
    /// <summary>
    ///     Gets or sets the list of display properties associated with a specific credential attribute.
    /// </summary>
    /// <value>
    ///     The list of display properties. Each display property provides information on how the credential attribute should
    ///     be displayed.
    /// </value>
    [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
    public List<ClaimDisplay>? Display { get; set; }

    /// <summary>
    ///     String value determining type of value of the claim. A non-exhaustive list of valid values defined by this
    ///     specification are string, number, and image media types such as image/jpeg.
    /// </summary>
    [JsonProperty("value_type", NullValueHandling = NullValueHandling.Ignore)]
    public string? ValueType { get; set; }
    
    /// <summary>
    ///     String value determining type of value of the claim. A non-exhaustive list of valid values defined by this
    ///     specification are string, number, and image media types such as image/jpeg.
    /// </summary>
    [JsonProperty("mandatory", NullValueHandling = NullValueHandling.Ignore)]
    public string? Mandatory { get; set; }
    
    [JsonProperty("nested_claims", NullValueHandling = NullValueHandling.Ignore)]
    [JsonExtensionData]
    public Dictionary<string, JToken>? NestedClaims { get; set; }
}

