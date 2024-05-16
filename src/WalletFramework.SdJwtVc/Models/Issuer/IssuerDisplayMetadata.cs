using Newtonsoft.Json;

namespace WalletFramework.SdJwtVc.Models.Issuer
{
    /// <summary>
    ///     Represents the visual representations for the Issuer.
    /// </summary>
    public class IssuerDisplayMetadata
    {
        /// <summary>
        ///     Gets or sets the name of the Issuer.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the locale, which represents the specific culture or region.
        /// </summary>
        [JsonProperty("locale")]
        public string? Locale { get; set; }
        
        /// <summary>
        ///     Gets or sets the logo, which represents the specific culture or region..
        /// </summary>
        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
        public IssuerLogo? Logo { get; set; }
        
        /// <summary>
        ///     Represents the Logo of the Issuer.
        /// </summary>
        public class IssuerLogo
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
}
