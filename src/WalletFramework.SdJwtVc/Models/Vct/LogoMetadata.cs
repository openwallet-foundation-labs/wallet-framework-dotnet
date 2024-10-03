using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.Vct.Models;

/// <summary>
///     Represents the information about the logo to be displayed for the type.
/// </summary>
public class LogoMetadata
{
    /// <summary>
    ///     Gets or sets the URI pointing to the logo image.
    /// </summary>
    [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
    public Uri? Uri { get; set; }
    
    /// <summary>
    ///     Gets or sets the integrity metadata for the logo image.
    /// </summary>
    [JsonProperty("uri#integrity", NullValueHandling = NullValueHandling.Ignore)]
    public string? UriIntegrity { get; set; }
    
    /// <summary>
    ///     Gets or sets the alternative text for the logo image.
    /// </summary>
    [JsonProperty("alt_text", NullValueHandling = NullValueHandling.Ignore)]
    public string? AltText { get; set; }
}
