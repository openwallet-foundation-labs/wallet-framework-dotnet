using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.Credential;

/// <summary>
///     Represents the visual representations for the credential.
/// </summary>
public record SdJwtDisplay
{
    /// <summary>
    ///     Gets or sets the logo associated with this Credential.
    /// </summary>
    [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
    public SdJwtLogo? Logo { get; set; }

    /// <summary>
    ///     Gets or sets the name of the Credential.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the background color for the Credential.
    /// </summary>
    [JsonProperty("background_color", NullValueHandling = NullValueHandling.Ignore)]
    public string? BackgroundColor { get; set; }

    /// <summary>
    ///     Gets or sets the locale, which represents the specific culture or region.
    /// </summary>
    [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
    public string? Locale { get; set; }

    /// <summary>
    ///     Gets or sets the text color for the Credential.
    /// </summary>
    [JsonProperty("text_color", NullValueHandling = NullValueHandling.Ignore)]
    public string? TextColor { get; set; }
        
    /// <summary>
    ///     Represents the Logo for a Credential.
    /// </summary>
    public class SdJwtLogo
    {
        /// <summary>
        ///     Gets or sets the alternate text that describes the logo image. This is typically used for accessibility purposes.
        /// </summary>
        [JsonProperty("alt_text")]
        public string? AltText { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the logo image.
        /// </summary>
        [JsonProperty("uri")]
        public Uri Uri { get; set; } = null!;
    }
}
