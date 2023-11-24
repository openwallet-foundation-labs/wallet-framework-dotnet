using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialRequest
{
    /// <summary>
    ///     Represents the proof of possession of the key material that the issued credential is bound to.
    ///     This contains the JWT that acts as the proof of possession, with the proof type being "jwt".
    /// </summary>
    public class OidProofOfPossession
    {
        /// <summary>
        ///     Gets or sets the JWT that acts as the proof of possession of the key material the issued credential is bound to.
        /// </summary>
        [JsonProperty("jwt")]
        public string Jwt { get; set; }

        /// <summary>
        ///     Gets or sets the type of proof, expected to be "jwt".
        /// </summary>
        [JsonProperty("proof_type")]
        public string ProofType { get; set; }
    }
}
