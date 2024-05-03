#nullable enable

using Newtonsoft.Json;

namespace Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.CredentialResponse
{
    /// <summary>
    ///     Represents a Credential Response. The response can be either immediate or deferred. In the synchronous response,
    ///     the issued Credential is immediately returned to the client. In the deferred response, an acceptance token
    ///     is sent to the client, which will be used later to retrieve the Credential once it's ready.
    /// </summary>
    public class OidCredentialResponse
    {
        /// <summary>
        ///     OPTIONAL. JSON integer denoting the lifetime in seconds of the c_nonce.
        /// </summary>
        [JsonProperty("c_nonce_expires_in")]
        public int? CNonceExpiresIn { get; set; }

        /// <summary>
        ///     OPTIONAL. A JSON string containing a security token subsequently used to obtain a Credential.
        ///     MUST be present when credential is not returned.
        /// </summary>
        [JsonProperty("acceptance_token")]
        public string? AcceptanceToken { get; set; }

        /// <summary>
        ///     OPTIONAL. JSON string containing a nonce to be used to create a proof of possession of key material
        ///     when requesting a Credential.
        /// </summary>
        [JsonProperty("c_nonce")]
        public string? CNonce { get; set; }

        /// <summary>
        ///     OPTIONAL. Contains issued Credential. MUST be present when acceptance_token is not returned.
        ///     MAY be a JSON string or a JSON object, depending on the Credential format.
        /// </summary>
        [JsonProperty("credential")]
        public string? Credential { get; set; }
    }
}
