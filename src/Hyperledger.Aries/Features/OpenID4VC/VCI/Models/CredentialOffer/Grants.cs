#nullable enable

using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialOffer.GrantTypes;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialOffer
{
    /// <summary>
    ///     Represents the grant types that the Credential Issuer's AS is prepared to process for the credential offer.
    /// </summary>
    public class Grants
    {
        /// <summary>
        ///     Gets or sets the authorization_code grant type parameters. This includes an optional issuer state that is used to
        ///     bind the subsequent Authorization Request with the Credential Issuer to a context set up during previous steps.
        /// </summary>
        [JsonProperty("authorization_code")]
        public AuthorizationCode? AuthorizationCode { get; set; }

        /// <summary>
        ///     Gets or sets the pre-authorized_code grant type parameters. This includes a required pre-authorized code
        ///     representing the Credential Issuer's authorization for the Wallet to obtain Credentials of a certain type, and an
        ///     optional boolean specifying whether a user PIN is required along with the Token Request.
        /// </summary>
        [JsonProperty("urn:ietf:params:oauth:grant-type:pre-authorized_code")]
        public PreAuthorizedCode? PreAuthorizedCode { get; set; }
    }
}
