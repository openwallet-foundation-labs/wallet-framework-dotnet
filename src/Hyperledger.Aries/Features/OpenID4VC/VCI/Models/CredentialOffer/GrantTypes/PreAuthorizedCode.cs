#nullable enable

using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialOffer.GrantTypes
{
    /// <summary>
    ///     Represents the parameters for the 'pre-authorized_code' grant type.
    /// </summary>
    public class PreAuthorizedCode
    {
        /// <summary>
        ///     Gets or sets a boolean value specifying whether the Credential Issuer expects presentation of a user PIN along with
        ///     the Token Request in a Pre-Authorized Code Flow.
        /// </summary>
        [JsonProperty("user_pin_required")]
        public bool? UserPinRequired { get; set; }

        /// <summary>
        ///     Gets or sets the pre-authorized code representing the Credential Issuer's authorization for the Wallet to obtain
        ///     Credentials of a certain type.
        /// </summary>
        [JsonProperty("pre-authorized_code")]
        public string Value { get; set; } = null!;

        /// <summary>
        ///     Gets or sets a description of the user PIN that may be required along with the Token Request in a Pre-Authorized
        ///     Code Flow.
        /// </summary>
        [JsonProperty("user_pin_description")]
        public string? UserPinDescription { get; set; }
    }
}
