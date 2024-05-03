using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer
{
    /// <summary>
    ///     Represents the Logo of the Issuer.
    /// </summary>
    public class OidIssuerLogo
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
