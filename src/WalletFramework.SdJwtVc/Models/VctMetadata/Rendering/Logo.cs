using static WalletFramework.SdJwtVc.Models.VctMetadata.Rendering

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

/// <summary>
///     Represents the information about the logo to be displayed for the type.
/// </summary>
public class Logo
{
    /// <summary>
    ///     Gets or sets the URI pointing to the logo image.
    /// </summary>
    public Uri? Uri { get; set; }
    
    /// <summary>
    ///     Gets or sets the alternative text for the logo image.
    /// </summary>
    public string? AltText { get; set; }
}

public static class LogoJsonKeys
{
    public const string UriJsonName = "uri";
    public const string AltTextJsonName = "alt_text";
}
