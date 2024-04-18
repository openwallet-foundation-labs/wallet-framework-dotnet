using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models;

public class ClaimDisplayInfo
{
    /// <summary>
    ///     Gets or sets the list of display properties associated with a specific credential attribute.
    /// </summary>
    /// <value>
    ///     The list of display properties. Each display property provides information on how the credential attribute should
    ///     be displayed.
    /// </value>
    [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
    public List<AttributeDisplay>? Display { get; set; }

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
    
    public class AttributeDisplay
    {
        /// <summary>
        ///     Gets or sets the name for the credential attribute.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the locale, which represents the specific culture or region.
        /// </summary>
        [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
        public string? Locale { get; set; }
    }
}
