#nullable enable

using System.Collections.Generic;
using System.Linq;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential.Attributes;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer
{
    /// <summary>
    ///     Represents the metadata of an OpenID4VCI Credential Issuer.
    /// </summary>
    public class OidIssuerMetadata
    {
        /// <summary>
        ///     Gets or sets a dictionary which maps a CredentialMetadataId to its credential metadata.
        /// </summary>
        [JsonProperty("credential_configurations_supported")]
        public Dictionary<string, OidCredentialMetadata> CredentialConfigurationsSupported { get; set; } = null!;

        /// <summary>
        ///     Gets or sets a list of display properties of a Credential Issuer for different languages.
        /// </summary>
        [JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
        public List<OidIssuerDisplay>? Display { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the Credential Issuer's Credential Endpoint.
        /// </summary>
        [JsonProperty("credential_endpoint")]
        public string CredentialEndpoint { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the identifier of the Credential Issuer.
        /// </summary>
        [JsonProperty("credential_issuer")]
        public string CredentialIssuer { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the identifier of the OAuth 2.0 Authorization Server that the Credential Issuer relies on for
        ///     authorization. If this property is omitted, it is assumed that the entity providing the Credential Issuer
        ///     is also acting as the Authorization Server. In such cases, the Credential Issuer's
        ///     identifier is used as the OAuth 2.0 Issuer value to obtain the Authorization Server
        ///     metadata.
        /// </summary>
        [JsonProperty("authorization_server", NullValueHandling = NullValueHandling.Ignore)]
        public string? AuthorizationServer { get; set; }

        /// <summary>
        ///     Gets the display properties of a given Credential for different languages.
        /// </summary>
        /// <param name="credentialMetadataId">The credentialMetadataId to retrieve the display properties for.</param>
        /// <returns>
        ///     A list of display properties for the specified Credential or null if the Credential is not found in the
        ///     metadata.
        /// </returns>
        public List<OidCredentialDisplay>? GetCredentialDisplay(string credentialMetadataId) 
            => CredentialConfigurationsSupported[credentialMetadataId].Display;

        /// <summary>
        ///     Gets the claim attributes of a given Credential.
        /// </summary>
        /// <param name="credentialMetadataId">The credentialMetadataId to retrieve the claim attributes for.</param>
        /// <returns>
        ///     A dictionary of attribute names and their corresponding display properties for the specified Credential, or
        ///     null if the Credential is not found in the metadata.
        /// </returns>
        public Dictionary<string, OidClaim>? GetCredentialClaims(string credentialMetadataId) =>
            CredentialConfigurationsSupported[credentialMetadataId].Claims;

        /// <summary>
        ///     Gets the localized attribute names of a given Credential for a specific locale.
        /// </summary>
        /// <param name="credentialMetadataId">The credentialMetadataId to retrieve the localized attribute names for.</param>
        /// <param name="locale">The locale to retrieve the attribute names in (e.g., "en-US").</param>
        /// <returns>
        ///     A list of localized attribute names for the specified Credential and locale, or null if no matching attributes
        ///     are found.
        /// </returns>
        public List<string>? GetLocalizedCredentialAttributeNames(string credentialMetadataId, string locale)
        {
            var displayNames = new List<string>();

            var matchingCredential = CredentialConfigurationsSupported[credentialMetadataId];

            if (matchingCredential == null)
                return null;

            var localeDisplayNames = matchingCredential.Claims
                .SelectMany(subject => subject.Value.Display)
                .Where(display => display.Locale == locale)
                .Select(display => display.Name);

            displayNames.AddRange(localeDisplayNames!);

            return displayNames.Count > 0 ? displayNames : null;
        }

        /// <summary>
        ///     Gets the localized alias name of the Credential Issuer for a specific locale.
        /// </summary>
        /// <param name="locale">The locale to retrieve the issuer alias name in (e.g., "en-US").</param>
        /// <returns>
        ///     The localized alias name for the specified locale, or null if no matching alias is found.
        /// </returns>
        public string? GetLocalizedIssuerAlias(string locale)
        {
            if (Display == null)
                return string.Empty;

            var display = Display.FirstOrDefault(display => display.Locale == locale);
            return display?.Name;
        }
    }
}
