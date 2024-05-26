using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Credential.Attributes;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.CredentialRequest
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
        ///     Gets or sets the dictionary representing the attributes of the credential in different languages.
        /// </summary>
        [JsonProperty("claims")]
        public Dictionary<string, OidClaim>? Claims { get; set; }
        
        /// <summary>
        ///     Gets or sets the verifiable credential type (vct).
        /// </summary>
        [JsonProperty("vct")]
        public string Vct { get; set; } = null!;
    }
}
