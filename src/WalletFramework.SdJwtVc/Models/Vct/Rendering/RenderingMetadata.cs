using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.Vct;

/// <summary>
///     Represents the rendering information of a specific vc type.
/// </summary>
public class RenderingMetadata
{
    /// <summary>
    ///     Gets or sets the information about the logo to be displayed for the type.
    /// </summary>
    [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
    public LogoMetadata? Logo { get; set; }
    
    /// <summary>
    ///     Gets or sets the background color for the Credential.
    /// </summary>
    [JsonProperty("background_color", NullValueHandling = NullValueHandling.Ignore)]
    public string? BackgroundColor { get; set; }
}
