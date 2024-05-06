#nullable enable

using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.Credential
{
    /// <summary>
    ///     Represents the Logo for a Credential.
    /// </summary>
    public class OidCredentialLogo
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
