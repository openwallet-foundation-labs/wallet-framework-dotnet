using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer
{
    /// <summary>
    ///     Represents the visual representations for the Issuer.
    /// </summary>
    public class OidIssuerDisplay
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
    }
}
