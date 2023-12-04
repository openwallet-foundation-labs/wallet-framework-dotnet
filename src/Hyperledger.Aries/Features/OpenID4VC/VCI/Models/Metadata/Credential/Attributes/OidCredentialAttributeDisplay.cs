#nullable enable

using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential.Attributes
{
    /// <summary>
    ///     Represents the visual representations for the credential attribute.
    /// </summary>
    public class OidCredentialAttributeDisplay
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
