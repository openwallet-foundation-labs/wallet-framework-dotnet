using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

public class SimpleRenderingMethod
{
    /// <summary>
    ///     Gets or sets the information about the logo to be displayed for the type.
    /// </summary>
    [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
    public LogoMetadata? Logo { get; set; }
    
    /// <summary>
    ///     Gets or sets the background color for the credential.
    /// </summary>
    [JsonProperty("background_color", NullValueHandling = NullValueHandling.Ignore)]
    public string? BackgroundColor { get; set; }
    
    /// <summary>
    ///     Gets or sets the text color for the credential.
    /// </summary>
    [JsonProperty("text_color", NullValueHandling = NullValueHandling.Ignore)]
    public string? TextColor { get; set; }
}
