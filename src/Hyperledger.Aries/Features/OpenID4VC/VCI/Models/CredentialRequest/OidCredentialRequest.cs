using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Credential;
using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialRequest
{
    /// <summary>
    ///     Represents a credential request made by a client to the Credential Endpoint.
    ///     This request contains the format of the credential, the type of credential,
    ///     and a proof of possession of the key material the issued credential shall be bound to.
    /// </summary>
    public class OidCredentialRequest
    {
        /// <summary>
        ///     Gets or sets the proof of possession of the key material the issued credential shall be bound to.
        /// </summary>
        [JsonProperty("proof")]
        public OidProofOfPossession? Proof { get; set; }

        /// <summary>
        ///     Gets or sets the format of the credential to be issued.
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the Credential Definition.
        /// </summary>
        [JsonProperty("credential_definition")]
        public OidCredentialDefinition CredentialDefinition { get; set; } = null!;
    }
}
